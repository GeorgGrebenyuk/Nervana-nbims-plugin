using BIMStructureMgd.DatabaseObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Extensions
{
    internal static class ArcBuildingWallExtension
    {
        public static Arc GetBaseline2(this ArcBuildingWall arcWall)
        {
            Point3d startPt = arcWall.StartPoint;
            Point3d endPt = arcWall.EndPoint;
            double radius = arcWall.Radius;

            // Calculate center point
            Vector3d chord = endPt - startPt;
            double chordLength = chord.Length;

            // Calculate center point
            Vector3d chordDir = chord.GetNormal();
            Vector3d perpendicular = chordDir.CrossProduct(Vector3d.ZAxis);

            double h = Math.Sqrt(radius * radius - chordLength * chordLength / 4);
            Point3d midPoint = startPt + chord / 2;

            // Two possible center points
            Point3d center1 = midPoint + perpendicular * h;
            Point3d center2 = midPoint - perpendicular * h;

            // Выбор конкретного варианта будет из длины дуги
            double length1 = CalculateArcLength(center1, startPt, endPt, radius);
            double length2 = CalculateArcLength(center2, startPt, endPt, radius);

            double CalculateArcLength(Point3d center, Point3d point1, Point3d point2, double radius)
            {
                // Calculate vectors from center to points
                var v1 = new Vector2d(point1.X - center.X, point1.Y - center.Y);
                var v2 = new Vector2d(point2.X - center.X, point2.Y - center.Y);

                // Calculate angle between vectors
                double dotProduct = v1.X * v2.X + v1.Y * v2.Y;
                double magnitudeProduct = v1.Length * v2.Length;
                double cosAngle = dotProduct / magnitudeProduct;

                // Handle floating point precision issues
                cosAngle = Math.Max(-1, Math.Min(1, cosAngle));

                // Calculate central angle in radians
                double angle = Math.Acos(cosAngle);

                // Calculate arc length: s = r * θ
                double arcLength = radius * angle;

                return arcLength;
            }

            Point3d centerTrue;
            double length1_check = Math.Abs(1 - length1 / arcWall.Length);
            double length2_check = Math.Abs(1 - length2 / arcWall.Length);

            if (length1_check > length2_check) centerTrue = center1;
            else centerTrue = center2;

            // Calculate angles
            Vector3d startVec = startPt - centerTrue;
            Vector3d endVec = endPt - centerTrue;

            double startAngle = Math.Atan2(startVec.Y, startVec.X);
            double endAngle = Math.Atan2(endVec.Y, endVec.X);

            // Create arc
            return new Arc(centerTrue, radius, arcWall.Angle, arcWall.Angle);
        }
    }
}
