
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using BIMStructureMgd.DatabaseObjects;

namespace NervanaNcBIMsMgd.Extensions
{
    internal static class SpaceEntityExtension
    {
        public static Point3d GetCentroid(this SpaceEntity spaceEntity)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> z = new List<double>();
            foreach (Polyline pline in spaceEntity.GetFloorContours())
            {
                for (int plineVertexIndex = 0; plineVertexIndex < pline.NumberOfVertices; plineVertexIndex++)
                {
                    Point3d plineVettex = pline.GetPoint3dAt(plineVertexIndex);
                    x.Add(plineVettex.X);
                    y.Add(plineVettex.Y);
                    z.Add(plineVettex.Z);
                }
            }
            int vertsCount = x.Count;
            return new Point3d(x.Sum() / vertsCount, y.Sum() / vertsCount, z.Sum() / vertsCount);
        }

        public static Point3d[] GetBoundary(this SpaceEntity spaceEntity)
        {
            List<Point3d> boundary = new List<Point3d>();
            foreach (Polyline pline in spaceEntity.GetFloorContours())
            {
                for (int plineVertexIndex = 0; plineVertexIndex < pline.NumberOfVertices; plineVertexIndex++)
                {
                    Point2d plineVettex2d = pline.GetPoint2dAt(plineVertexIndex);
                    Point3d plineVettex3d = new Point3d(plineVettex2d.X, plineVettex2d.Y, spaceEntity.FloorValue);
                    if (!boundary.Contains(plineVettex3d)) boundary.Add(plineVettex3d);
                }
            }

            Curve2d c;
            c.GetTrimmedOffset(5, OffsetCurveExtensionType.Chamfer);

            return boundary.ToArray();
        }

        public static Extents3d GetBounds2(this SpaceEntity spaceEntity)
        {
            Point3d[] geom = GetBoundary(spaceEntity);

            double[] x = geom.Select(point => point.X).ToArray();
            double[] y = geom.Select(point => point.Y).ToArray();
            double[] z = geom.Select(point => point.Z).ToArray();

            return new Extents3d(new Point3d(x.Min(), y.Min(), z.Min()), new Point3d(x.Max(), y.Max(), z.Max()));
        }
    }
}
