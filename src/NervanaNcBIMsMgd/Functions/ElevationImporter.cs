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

using NervanaNcBIMsMgd.Extensions;
using NervanaNcBIMsMgd.Geometry;
using NervanaCommonMgd;
using BIMStructureMgd.DatabaseObjects;
using HostMgd.EditorInput;

namespace NervanaNcBIMsMgd.Functions
{
    public class ElevationImporterSettings
    {
        public double IsolinesStep = 0.5;
        public bool UseCoordsOffset = true;

        public string? SelectedSurfaceName;
        public bool AddExternalBorder = false;
    }

    public class LandXML_SurfaceDef
    {
        public Dictionary<int, Point3d> Pnts { get; set; }
        public int[][] Faces { get; set; }

        public DelaunayTriangulation.Triangle[] Triangles { get; set; }

        public LandXML_SurfaceDef()
        {
            Pnts = new Dictionary<int, Point3d>();
            Faces = new int[][] { };
            Triangles = new DelaunayTriangulation.Triangle[] { };
        }
    }

    internal class ElevationImporter
    {
        public ElevationImporter()
        {
            this.pBuildingTransformInfo = Utils.CurrentDoc.Database.SummaryInfo.GetTransformPararameters();
            this.pAngleSinus = Math.Sin(this.pBuildingTransformInfo.AngleRad);
            this.pAngleCos = Math.Cos(this.pBuildingTransformInfo.AngleRad);

            TraceWriter.Log($"pBuildingTransformInfo {pBuildingTransformInfo.ToString()}");
            readSubDMesh();
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

        private void readSubDMesh()
        {
            // Выбрать в модели сеть
            Editor ed = Utils.CurrentDoc.Editor;

            PromptEntityOptions entSelectionSettings = new PromptEntityOptions("Выберите сеть");
            entSelectionSettings.AddAllowedClass(typeof(SubDMesh), true);

            PromptEntityResult entSelection = ed.GetEntity(entSelectionSettings);
            if (entSelection.Status != PromptStatus.OK) return;

            using (Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                SubDMesh? meshEntity = tr.GetObject(entSelection.ObjectId, OpenMode.ForRead) as SubDMesh;
                if (meshEntity == null) return;

                TraceWriter.Log($"SubDMesh Vertices {meshEntity.NumberOfVertices}");
                TraceWriter.Log($"SubDMesh Faces {meshEntity.NumberOfFaces}");
                TraceWriter.Log($"SubDMesh Faces2 {meshEntity.FaceArray.Count}");

                this.mLandXML_Surface = new LandXML_SurfaceDef();
                

                for (int vertexIndex = 0; vertexIndex < meshEntity.NumberOfVertices; vertexIndex++)
                {
                    this.mLandXML_Surface.Pnts.Add(vertexIndex, ReCalcPoint(meshEntity.Vertices[vertexIndex]));
                }

                //this.mLandXML_Surface.Faces = new int[meshEntity.NumberOfFaces][];
                this.mLandXML_Surface.Triangles = new DelaunayTriangulation.Triangle[meshEntity.NumberOfFaces];
                for (int faceIndex = 0; faceIndex < meshEntity.NumberOfFaces; faceIndex++)
                {
                    int faceIndex2 = faceIndex * 4;
                    int[] faceIndices = new int[] { meshEntity.FaceArray[faceIndex2 + 1] , meshEntity.FaceArray[faceIndex2 + 2], meshEntity.FaceArray[faceIndex2 + 3] };
                    //this.mLandXML_Surface.Faces[faceIndex] = faceIndices;

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
            if (mLandXML_Surface == null) return;

            var allZ = mLandXML_Surface.Pnts.Values.Select(p => p.Z).ToList();

            // Если выбрана опция генерации доп. контура, то нужно пересчитать триангуляцию с учетом новых точек контура
            if (settings.AddExternalBorder)
            {
                var existedSurfaceVertices = mLandXML_Surface.Pnts.Values.Cast<Point3d>().ToList();

                var allX0 = existedSurfaceVertices.Select(p => p.X);
                var allY0 = existedSurfaceVertices.Select(p => p.Y);
                var allZ0 = existedSurfaceVertices.Select(p => p.Z);

                double xMin0 = allX0.Min() - 10.0;
                double xMax0 = allX0.Max() + 10.0;

                double yMin0 = allY0.Min() - 10.0;
                double yMax0 = allY0.Max() + 10.0;

                double minZ0 = allZ0.Min() - 1.0;
                allZ.Add(minZ0);

                existedSurfaceVertices.Add(new Point3d(xMin0, yMin0, minZ0));
                existedSurfaceVertices.Add(new Point3d(xMin0, yMax0, minZ0));
                existedSurfaceVertices.Add(new Point3d(xMax0, yMax0, minZ0));
                existedSurfaceVertices.Add(new Point3d(xMax0, yMin0, minZ0));

                mLandXML_Surface.Triangles = DelaunayTriangulation.Triangulate(existedSurfaceVertices).ToArray();
            }

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
            var polylines = generatorIso.ConnectSegmentsIntoPolylines(isolines);


            TraceWriter.Log($"Generated {isolines.Count} contour segments");
            TraceWriter.Log($"Connected into {polylines.Count} polylines");

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

                int counter = 0;
                foreach (var polyline in polylines)
                {
                    List<Point3d> polyline_CHull = DelaunayTriangulation.CalculateConvexHull(polyline);

                    //TraceWriter.Log($"Prepare create slab {polyline_CHull.Count} vertices raw");
                    //TraceWriter.Log($"Z = {polyline_CHull.First().Z}");

                    int skippedPos = 0;
                    if (polyline_CHull.First() == polyline_CHull.Last()) skippedPos = 1;

                    var polyline_flat = polyline_CHull.Skip(skippedPos).Select(p => new Point2d(p.X, p.Y)).ToList();
                    if (polyline_flat.Count < 3) continue;

                    //TraceWriter.Log($"Prepare create slab {polyline_flat.Count} vertices after check");

                    try
                    {
                        BuildingSlab floorInstance = BuildingSlabFactory.Create(polyline_flat, settings.IsolinesStep);
                        floorInstance.TransformBy(Matrix3d.Displacement(new Vector3d(0, 0, polyline_CHull.First().Z + 300.0) - 
                            new Vector3d(0, 0, floorInstance.BasePoint.Z) ));

                        acBlkTblRec.AppendEntity(floorInstance);
                        tr.AddNewlyCreatedDBObject(floorInstance, true);
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

        private LandXML_SurfaceDef? mLandXML_Surface;
        public bool IsXml = false;
        private ProjectTransformPararameters pBuildingTransformInfo;
    }
}
