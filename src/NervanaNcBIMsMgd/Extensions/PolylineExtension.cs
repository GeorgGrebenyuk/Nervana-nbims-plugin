using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;
using Teigha.DatabaseServices;

namespace NervanaNcBIMsMgd.Extensions
{
    internal static class PolylineExtension
    {
        public static Point3d[] ToVertexes(this Polyline ncadPolyline)
        {
            Point3d[] result = new Point3d[ncadPolyline.NumberOfVertices];
            for (int plineVertexIndex = 0; plineVertexIndex < ncadPolyline.NumberOfVertices; plineVertexIndex++)
            {
                Point3d plineVettex = ncadPolyline.GetPoint3dAt(plineVertexIndex);
                result[plineVertexIndex] = plineVettex;
            }
            return result;
        }

        public static Point3d GetCentroid(this Polyline ncadPolyline)
        {
            int plineSize = ncadPolyline.NumberOfVertices;
            double[] x = new double[plineSize];
            double[] y = new double[plineSize];
            double[] z = new double[plineSize];
            for (int plineVertexIndex = 0; plineVertexIndex < plineSize; plineVertexIndex++)
            {
                Point3d plineVettex = ncadPolyline.GetPoint3dAt(plineVertexIndex);
                x[plineVertexIndex] = plineVettex.X;
                y[plineVertexIndex] = plineVettex.Y;
                z[plineVertexIndex] = plineVettex.Z;
            }

            return new Point3d(x.Sum() / plineSize, y.Sum() / plineSize, z.Sum() / plineSize);
        }
    }
}
