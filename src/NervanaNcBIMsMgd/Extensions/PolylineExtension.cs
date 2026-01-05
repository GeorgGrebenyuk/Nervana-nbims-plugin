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
    }
}
