using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcMgd.Common.Geometry
{
    public class BoundingBox
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public double MinZ { get; set; }
        public double MaxZ { get; set; }


        public double[] GetMinPoint() { return new double[] { MinX, MinY, MinZ };}
        public double[] GetMaxPoint() { return new double[] { MaxX, MaxY, MaxZ }; }
        public double[] GetCentroid()
        {
            return new double[] { (MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ) / 2 };
        }

        public static BoundingBox CalculateFromPoints(IEnumerable<Point3d> points)
        {
            return CalculateFromPoints(points.Select(a => new Vector3d(a.X, a.Y, a.Z)));
        }
        public static BoundingBox CalculateFromPoints(IEnumerable<Vector3d> points)
        {
            return new BoundingBox
            {
                MinX = points.Min(v => v.X),
                MaxX = points.Max(v => v.X),
                MinY = points.Min(v => v.Y),
                MaxY = points.Max(v => v.Y),
                MinZ = points.Min(v => v.Z),
                MaxZ = points.Max(v => v.Z)
            };
        }


        public static BoundingBox GetBBoxFrom(IEnumerable<BoundingBox> bboxes)
        {

            double[] x = new double[bboxes.Count() * 2];
            double[] y = new double[bboxes.Count() * 2];
            double[] z = new double[bboxes.Count() * 2];

            int counter = 0;
            foreach (BoundingBox bbox in bboxes)
            {
                x[counter] = bbox.MinX;
                x[counter] = bbox.MaxX;
                y[counter] = bbox.MinY;
                y[counter] = bbox.MaxY;
                z[counter] = bbox.MinZ;
                z[counter] = bbox.MaxZ;
                counter +=2;
            }

            return new BoundingBox() { MinX = x.Min(), MaxX = x.Max(), MinY = y.Min(),  MaxY = y.Max(), MinZ = z.Min(), MaxZ = z.Max() };
        }
    }
}
