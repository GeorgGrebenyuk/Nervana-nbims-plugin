using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.Geometry;

using NervanaCommonMgd;

namespace NervanaNcMgd.Common.Geometry
{
    internal static class PointInContourChecking
    {
        public static bool IsPointInPolygon(Point3d? point, Point3d[]? contour)
        {
            if (point == null) return false;
            if (contour == null) return false;

            TraceWriter.Log($"Start IsPointInPolygon {point.ToString()} plg with {contour.Length} vertices", LogType.Add);

            bool inside = false;
            int count = contour.Length;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Point3d pi = contour[i];
                Point3d pj = contour[j];

                // Check if the point's y-coordinate is between the y-coordinates of the edge
                if (((pi.Y > point.Value.Y) != (pj.Y > point.Value.Y)) &&
                    (point.Value.X < (pj.X - pi.X) * (point.Value.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
