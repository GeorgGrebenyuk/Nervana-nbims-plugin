
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using BIMStructureMgd.DatabaseObjects;

using NervanaCommonMgd;

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
            if (boundary[boundary.Count - 1] != boundary[0]) boundary.Add(boundary[0]);
            return boundary.ToArray();
        }

        public static Extents3d GetBounds2(this SpaceEntity spaceEntity, double offset = 0.0)
        {
            Point3d[] geom = GetBoundary(spaceEntity);

            double[] x = geom.Select(point => point.X).ToArray();
            double[] y = geom.Select(point => point.Y).ToArray();
            double[] z = geom.Select(point => point.Z).ToArray();

            Extents3d ext = new Extents3d(new Point3d(x.Min() - offset, y.Min() - offset, spaceEntity.FloorValue), new Point3d(x.Max() + offset, y.Max() + offset, spaceEntity.FloorValue + spaceEntity.Height));

            TraceWriter.Log("Extents calced = " + ext.ToString(), LogType.Add);
            return ext;

        }
    }
}
