using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Teigha.Geometry;
using Teigha.DatabaseServices;
using HostMgd.EditorInput;

using BIMStructureMgd.DatabaseObjects;

using NervanaCommonMgd;
using NervanaNcMgd.Common;
using NervanaNcBIMsMgd.Extensions;
using NervanaNcMgd.Common.Geometry;


namespace NervanaNcBIMsMgd.Functions
{
    public class ElevationImporterSettings
    {
        public ObjectId MeshEntityId = ObjectId.Null;
        public bool InsertAsSlabs = false;
        public double IsolinesStep = 0.5;
        //public bool UseCoordsOffset = true;
        //public bool AddExternalBorder = false;
    }

    // бывш. LandXML
    public class TinSurfaceDef
    {
        public Dictionary<int, Point3d> Pnts { get; set; }
        public int[][] Faces { get; set; }

        public DelaunayTriangulation.Triangle[] Triangles { get; set; }

        public TinSurfaceDef()
        {
            Pnts = new Dictionary<int, Point3d>();
            Faces = new int[][] { };
            Triangles = new DelaunayTriangulation.Triangle[] { };
        }

        public double[,] ConvertTinToGrid(double step)
        {
            BoundingBox bbox = GetBoundingBox();
            double stepX = step;
            double stepY = step;

            // Calculate grid dimensions
            int cols = (int)Math.Ceiling((bbox.MaxX - bbox.MinX) / stepX) + 1;
            int rows = (int)Math.Ceiling((bbox.MaxY - bbox.MinY) / stepY) + 1;
            var grid = new double[rows, cols];

            // Precompute triangle bounding boxes for faster lookup
            var triBounds = new (double minx, double maxx, double miny, double maxy)[Faces.Length];
            for (int i = 0; i < Faces.Length; i++)
            {
                var tri = Faces[i];
                double minx = double.MaxValue, maxx = double.MinValue,
                       miny = double.MaxValue, maxy = double.MinValue;
                for (int j = 0; j < 3; j++)
                {
                    var pt = Pnts[tri[j]];
                    minx = Math.Min(minx, pt.X);
                    maxx = Math.Max(maxx, pt.X);
                    miny = Math.Min(miny, pt.Y);
                    maxy = Math.Max(maxy, pt.Y);
                }
                triBounds[i] = (minx, maxx, miny, maxy);
            }

            // Iterate over each grid point
            for (int i = 0; i < rows; i++)
            {
                double y = bbox.MinY + i * stepY;
                for (int j = 0; j < cols; j++)
                {
                    double x = bbox.MinX + j * stepX;

                    // Find containing triangle
                    double z = double.NaN;
                    for (int tIdx = 0; tIdx < Faces.Length; tIdx++)
                    {
                        var bounds = triBounds[tIdx];
                        if (x < bounds.minx || x > bounds.maxx || y < bounds.miny || y > bounds.maxy) continue;

                        var tri = Faces[tIdx];
                        var v0 = Pnts[tri[0]]; // Triangle vertex 0
                        var v1 = Pnts[tri[1]]; // Triangle vertex 1
                        var v2 = Pnts[tri[2]]; // Triangle vertex 2

                        // Compute barycentric coordinates
                        double denominator = (v1.Y - v2.Y) * (v0.X - v2.X) + (v2.X - v1.X) * (v0.Y - v2.Y);
                        double a = ((v1.Y - v2.Y) * (x - v2.X) + (v2.X - v1.X) * (y - v2.Y)) / denominator;
                        double b = ((v2.Y - v0.Y) * (x - v2.X) + (v0.X - v2.X) * (y - v2.Y)) / denominator;
                        double c = 1 - a - b;

                        // Check if point is inside triangle
                        if (a >= 0 && b >= 0 && c >= 0)
                        {
                            z = a * v0.Z + b * v1.Z + c * v2.Z; // Interpolate Z
                            break;
                        }
                    }
                    grid[i, j] = z;
                }
            }
            return grid;
        }

        public BoundingBox GetBoundingBox()
        {
            if (mBBOX == null)
            {
                double minX, maxX, minY, maxY, minZ, maxZ;
                double[] xCoords = Pnts.Values.Select(p => p.X).ToArray();
                double[] yCoords = Pnts.Values.Select(p => p.Y).ToArray();
                double[] zCoords = Pnts.Values.Select(p => p.Z).ToArray();

                minX = xCoords.Min();
                minY = yCoords.Min();
                minZ = zCoords.Min();
                maxX = xCoords.Max();
                maxY = yCoords.Max();
                maxZ = zCoords.Max();

                mBBOX = new BoundingBox()
                {
                    MinX = minX,
                    MinY = minY,
                    MinZ = minZ,
                    MaxX = maxX,
                    MaxY = maxY,
                    MaxZ = maxZ
                };
            }
            return mBBOX;
        }
        private BoundingBox? mBBOX;
    }

    internal class Tin2Conceptual
    {
        public Tin2Conceptual()
        {
            this.pBuildingTransformInfo = CommonUtils.CurrentDoc.Database.SummaryInfo.GetTransformPararameters();
            this.pAngleSinus = Math.Sin(this.pBuildingTransformInfo.AngleRad);
            this.pAngleCos = Math.Cos(this.pBuildingTransformInfo.AngleRad);

            TraceWriter.Log($"pBuildingTransformInfo {pBuildingTransformInfo.ToString()}");
        }

        public ElevationImporterSettings InitSettings()
        {
            ElevationImporterSettings settings = new ElevationImporterSettings();
            // Выбрать в модели сеть
            Editor ed = CommonUtils.CurrentDoc.Editor;

            PromptEntityOptions entSelectionSettings = new PromptEntityOptions("Выберите сеть");
            entSelectionSettings.AddAllowedClass(typeof(SubDMesh), true);

            PromptEntityResult entSelection = ed.GetEntity(entSelectionSettings);
            if (entSelection.Status == PromptStatus.OK) settings.MeshEntityId = entSelection.ObjectId;

            // Задать шаг расчета горизонталей
            settings.IsolinesStep = UserInput.GetUserInput("Укажите шаг горизонталей", 0.5);

            // В виде чего вставлять?
            settings.InsertAsSlabs = UserInput.GetUserInput("Вставлять в виде перекрытий (1) или солидов (0)?", false);

            return settings;
        }


        private Point3d ReCalcPoint(Point3d sourcePoint)
        {
            double x = sourcePoint.X, y = sourcePoint.Y, z = sourcePoint.Z;
            x -= pBuildingTransformInfo.X;
            y -= pBuildingTransformInfo.Y;
            z -= pBuildingTransformInfo.Z;

            x = x * pAngleCos - y * pAngleSinus;
            y = x * pAngleSinus + y * pAngleCos;

            return new Point3d(x, y, z);
        }

        private void readSubDMesh(ObjectId meshId)
        {
            if (meshId.IsNull) return;
            
            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                SubDMesh? meshEntity = tr.GetObject(meshId, OpenMode.ForRead) as SubDMesh;
                if (meshEntity == null) return;

                TraceWriter.Log($"SubDMesh Vertices {meshEntity.NumberOfVertices}");
                TraceWriter.Log($"SubDMesh Faces {meshEntity.NumberOfFaces}");
                TraceWriter.Log($"SubDMesh Faces2 {meshEntity.FaceArray.Count}");

                this.mLandXML_Surface = new TinSurfaceDef();
                
                for (int vertexIndex = 0; vertexIndex < meshEntity.NumberOfVertices; vertexIndex++)
                {
                    this.mLandXML_Surface.Pnts.Add(vertexIndex, ReCalcPoint(meshEntity.Vertices[vertexIndex]));
                }

                this.mLandXML_Surface.Faces = new int[meshEntity.NumberOfFaces][];
                this.mLandXML_Surface.Triangles = new DelaunayTriangulation.Triangle[meshEntity.NumberOfFaces];
                for (int faceIndex = 0; faceIndex < meshEntity.NumberOfFaces; faceIndex++)
                {
                    int faceIndex2 = faceIndex * 4;
                    int[] faceIndices = new int[] { meshEntity.FaceArray[faceIndex2 + 1] , meshEntity.FaceArray[faceIndex2 + 2], meshEntity.FaceArray[faceIndex2 + 3] };
                    this.mLandXML_Surface.Faces[faceIndex] = faceIndices;

                    DelaunayTriangulation.Triangle trg =
                       new DelaunayTriangulation.Triangle(
                           this.mLandXML_Surface.Pnts[faceIndices[0]], this.mLandXML_Surface.Pnts[faceIndices[1]], this.mLandXML_Surface.Pnts[faceIndices[2]]);
                    this.mLandXML_Surface.Triangles[faceIndex] = trg;

                    //TraceWriter.Log($"Triangle {trg.ToString()}");
                }
                TraceWriter.Log($"Triangles Length {this.mLandXML_Surface.Triangles.Length}");
            }
        }



        public void Import(ElevationImporterSettings settings)
        {
            readSubDMesh(settings.MeshEntityId);

            if (mLandXML_Surface == null) return;

            var allZ = mLandXML_Surface.Pnts.Values.Select(p => p.Z).ToList();

            // ФОрмируем Z, для которых нужно вычислить горизонтали
            List<double> isolineLevels = new List<double>();

            double minZ = allZ.Min();
            double maxZ = allZ.Max();

            double minZ2 = Math.Floor(minZ);
            int levelSteps = Convert.ToInt32(Math.Ceiling((maxZ - minZ2) / settings.IsolinesStep));
            for (int levelStep = 0; levelStep < levelSteps; levelStep++)
            {
                isolineLevels.Add(minZ2 + levelStep * settings.IsolinesStep);
            }

            TraceWriter.Log("Generate isolines start");

            // Generate isolines
            IsolineGenerator generatorIso = new IsolineGenerator();
            var isolines = generatorIso.GenerateIsolines(mLandXML_Surface.Triangles, isolineLevels);
            var polylines = generatorIso.ConnectSegmentsIntoPolylines(isolines, 0.1);

            var bbox = mLandXML_Surface.GetBoundingBox();
            double length = new double[] {Math.Abs(bbox.MaxX - bbox.MinX), Math.Abs(bbox.MaxY - bbox.MinY) }.Max();
            double gridStep = 1000000.0 / length / length;

            //TODO: проверить иначе
            //var contours = IsolineGenerator2.GenerateContours(mLandXML_Surface.ConvertTinToGrid(gridStep), settings.IsolinesStep); // Interval of 2 units

            TraceWriter.Log($"Generated {isolines.Count} contour segments");
            TraceWriter.Log($"Connected into {polylines.Count} polylines");

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable? acBlkTbl;
                acBlkTbl = tr.GetObject(CommonUtils.CurrentDoc.Database.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return;

                // Open the Block table record Model space for write
                BlockTableRecord? acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                if (acBlkTblRec == null) return;

                int counter = 0;
                foreach (var polyline in polylines)
                {
                    List<Point3d> targetContourRaw = DelaunayTriangulation.CalculateConvexHull(polyline);

                    //TraceWriter.Log($"Prepare create slab {polyline_CHull.Count} vertices raw");
                    //TraceWriter.Log($"Z = {polyline_CHull.First().Z}");

                    int skippedPos = 0;
                    if (targetContourRaw.First() == targetContourRaw.Last()) skippedPos = 1;

                    List<Point3d> targetContourRaw2 = targetContourRaw.Skip(skippedPos).ToList();



                    List<Point2d> targetContourFlat = targetContourRaw2.Select(p => new Point2d(p.X, p.Y)).ToList();
                    if (targetContourFlat.Count < 3) continue;

                    //Создадим геометрию контура
                    var dataForPline2d = CommonTools.ConvertVerticesFromList(targetContourRaw2);

                    Polyline2d tmpContourPline2d = new Polyline2d(Poly2dType.SimplePoly, dataForPline2d.Item1, targetContourRaw2.First().Z, true, 0, 0, dataForPline2d.Item2);

                    acBlkTblRec.AppendEntity(tmpContourPline2d);
                    tr.AddNewlyCreatedDBObject(tmpContourPline2d, true);

                    //TraceWriter.Log($"Prepare create slab {polyline_flat.Count} vertices after check");

                    try
                    {
                        Entity? createdFloorEntity = null;
                        if (settings.InsertAsSlabs)
                        {
                            BuildingSlab createdFloorAsSlab = BuildingSlabFactory.Create(targetContourFlat, settings.IsolinesStep);
                            createdFloorAsSlab.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, targetContourRaw2.First().Z + 300.0) -
                            new Vector3d(0, 0, createdFloorAsSlab.BasePoint.Z)));

                            createdFloorEntity = createdFloorAsSlab;
                        }
                        else
                        {
                            var plineResult = CommonTools.ConvertVerticesFromList(targetContourRaw2);
                            Polyline2d polyline_flat2 = new Polyline2d(Poly2dType.SimplePoly, plineResult.Item1, 0, true, 0, 0, plineResult.Item2);

                            // Create region from polyline first
                            DBObjectCollection curves = new DBObjectCollection();
                            curves.Add(polyline_flat2.Clone() as Curve);

                            DBObjectCollection regions = new DBObjectCollection();
                            try
                            {
                                regions = Region.CreateFromCurves(curves);

                                Solid3d solid = new Solid3d();
                                if (regions.Count > 0 && regions[0] is Region region)
                                {
                                    // Extrude the region
                                    solid.Extrude(region, settings.IsolinesStep, 0);
                                }
                                solid.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, targetContourRaw.First().Z) -
                           new Vector3d(0, 0, 0)));

                                createdFloorEntity = solid;
                            }
                            catch { }
                        }

                        if (createdFloorEntity == null) continue;
                        

                        acBlkTblRec.AppendEntity(createdFloorEntity);
                        tr.AddNewlyCreatedDBObject(createdFloorEntity, true);
                        //TraceWriter.Log($"Create slab success");
                    }
                    catch (Exception ex)
                    {
                        TraceWriter.Log(ex.Message, LogType.Error);
                    }

                    counter++;
                    //if (counter == 3) break;
                }
                tr.Commit();

                TraceWriter.Log($"All success");
            }
        }

        private double pAngleSinus;
        private double pAngleCos;

        private double pIsolineSteps = 0.5;

        private TinSurfaceDef? mLandXML_Surface;
        public bool IsXml = false;
        private ProjectTransformPararameters pBuildingTransformInfo;
    }
}
