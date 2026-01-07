using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.DatabaseServices;
using Teigha.Geometry;

using BIMStructureMgd.DatabaseObjects;

using NervanaNcBIMsMgd.Extensions;
using NervanaNcBIMsMgd.Functions.SolarCalc;
using NervanaNcBIMsMgd.Geometry;
using NervanaCommonMgd.Configs;
using NervanaCommonMgd;

namespace NervanaNcBIMsMgd.Functions
{

    public class ParametersShadowsBySunCreator
    {
        public const string HourPerDay = "Bri4ka. Час дня (положение солнца) ";
    }

    /// <summary>
    /// Вспомогательный класс для обработки функции "Построение теней от Солнца"
    /// </summary>
    public class ShadowsBySunCreator
    {
        public ShadowsBySunCreator()
        {

        }

        public void Start(ShadowCalcParametersConfig sunParameters)
        {
            pSolarPositions = SunCalculator.GetSolarPositionsPerDay(sunParameters.Date, sunParameters.Latitude, sunParameters.Longitude, sunParameters.TimeZoneOffset);

            if (pSolarPositions == null) return;

            //Анализируем только точки кровли (верхнюю грань) и перекрытия
            //TODO: дать пользователю выбор, на основе чего анализировать
            //Для отладки смотрим только на кровлю, потом надо включить и перекрытия
            ObjectIdCollection analyzedObjects = Utils.SelectObjectsByTypes(
                new Type[] {
                    typeof(BuildingRoof),
                    typeof(ArcBuildingWall),
                    typeof(LinearBuildingWall),
                    typeof(BuildingSlab)
                });
            if (analyzedObjects.Count < 1) return;

            List<ShadowAnalyzedItem> analyzedPoints = new List<ShadowAnalyzedItem>();

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId entId in analyzedObjects)
                {
                    Entity? entInstance = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                    if (entInstance == null) continue;

                    Point3d[] geomPoints = new Point3d[] { };

                    if (entInstance.GetType() == typeof(BuildingRoof))
                    {
                        BuildingRoof? entInstanceAsRoof = entInstance as BuildingRoof;
                        if (entInstanceAsRoof == null) continue;

                        geomPoints = entInstanceAsRoof.GetContour().ToVertexes();
                        var roofCenter = entInstanceAsRoof.GetContour().GetCentroid();
                        // Добавляем вершину кровли как центроид на высоте кровли. Доступа через API к коньку кровли нет.

                        geomPoints = geomPoints.Concat(new Point3d[] { new Point3d(roofCenter.X, roofCenter.Y, entInstanceAsRoof.Height)  }).ToArray();
                    }
                    else if (entInstance.GetType() == typeof(BuildingWallBase))
                    {
                        BuildingWallBase? entInstanceAsWallBase = entInstance as BuildingWallBase;
                        if (entInstanceAsWallBase == null) continue;
                        geomPoints = new Point3d[] {
                            entInstanceAsWallBase.StartPoint,
                            entInstanceAsWallBase.EndPoint,
                            new Point3d(entInstanceAsWallBase.StartPoint.X, entInstanceAsWallBase.StartPoint.Y, entInstanceAsWallBase.Height),
                            new Point3d(entInstanceAsWallBase.EndPoint.X, entInstanceAsWallBase.EndPoint.Y, entInstanceAsWallBase.Height)
                        };
                    }
                    else if (entInstance.GetType() == typeof(BuildingSlab))
                    {
                        BuildingSlab? entInstanceAsFloor = entInstance as BuildingSlab;
                        if (entInstanceAsFloor == null) continue;

                        geomPoints = entInstanceAsFloor.GetContour().ToVertexes();

                    }

                    if (geomPoints.Any())
                    {
                        foreach (var geomPoint in geomPoints)
                        {
                            analyzedPoints.Add(new ShadowAnalyzedItem(geomPoint));
                        }
                    }
                }
                tr.Abort();
            }

            TraceWriter.Log($"Анализ точек солнца закончен!. Найдено {analyzedPoints.Count} точек");

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable? acBlkTbl;
                acBlkTbl = tr.GetObject(Utils.CurrentDoc.Database.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return;

                // Open the Block table record Model space for write
                BlockTableRecord? acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                if (acBlkTblRec == null) return;

                foreach (var solarPoint in pSolarPositions)
                {
                    List<ShadowResult> shadowResults = new List<ShadowResult>();
                    foreach (var shadowRawInfo in analyzedPoints)
                    {
                        shadowResults.Add(ShadowCalculator.CalculateColumnShadow(shadowRawInfo, solarPoint, sunParameters.GroundElevation));
                    }

                    //Для построенных теней нужно сформировать внешнюю границу
                    var vertices = shadowResults.Select(p => p.ShadowEnd);
                    //var triangles = DelaunayTriangulation.Triangulate(vertices);
                    //var extContour = DelaunayTriangulation.CalculateExternalBorder(triangles);

                    var extContour = DelaunayTriangulation.CalculateConvexHull(vertices);
                    var bbox = BoundingBox.CalculateFromPoints(vertices);

                    TraceWriter.Log($"Закончен расчет для данного положения Солнца!. Найдено {vertices.Count()} точек");

                    Hatch hatchDef = new Hatch();
                    hatchDef.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                    hatchDef.ColorIndex = 1; //Red
                    hatchDef.Transparency = new Teigha.Colors.Transparency(80);

                    // Add hatch to database
                    acBlkTblRec.AppendEntity(hatchDef);
                    tr.AddNewlyCreatedDBObject(hatchDef, true);

                    TraceWriter.Log($"Создано определение штриховки");

                    var extContourData = CommonTools.ConvertVerticesFromList(extContour);

                    Polyline2d extContourEntity = new Polyline2d(Poly2dType.SimplePoly, extContourData.Item1, 0, true, 0, 0, extContourData.Item2);

                    if (extContourEntity != null)
                    {
                        extContourEntity.Closed = true;

                        acBlkTblRec.AppendEntity(extContourEntity);
                        tr.AddNewlyCreatedDBObject(extContourEntity, true);

                        hatchDef.AppendLoop(HatchLoopTypes.External | HatchLoopTypes.Polyline, new ObjectIdCollection(new ObjectId[] { extContourEntity.Id }));
                        extContourEntity.Erase();

                        TraceWriter.Log($"Геометрия добавлена в штриховку!");

                        DBText sunLabel = new DBText();
                        sunLabel.TextString = $"Расчетный час: " + solarPoint.Hour.ToString();
                        sunLabel.Position = new Point3d(bbox.GetCentroid()[0], bbox.GetCentroid()[1], bbox.GetCentroid()[2]);

                        acBlkTblRec.AppendEntity(sunLabel);
                        tr.AddNewlyCreatedDBObject(sunLabel, true);
                    }

                    hatchDef.EvaluateHatch(true);
                }

                tr.Commit();
            }
        }

        private List<SolarPosition>? pSolarPositions;
    }
}
