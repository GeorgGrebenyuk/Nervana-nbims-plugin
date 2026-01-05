using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Geometry
{
    enum GeometryVariant
    {
        Point,
        Contour,
        Line
    }

    /// <summary>
    /// Представление объекта модели с вырожденной геометрией
    /// </summary>
    internal class ObjectWithGeometry
    {
        public ObjectId ObjectId { get; set; }
        public GeometryVariant GeometryType { get; set; }

        public object? Geometry { get; set; }
        public Point3d? AsPoint()
        {
            if (GeometryType == GeometryVariant.Point) return this.Geometry as Point3d?;
            return null;
        }

        public Point3d[]? AsPoints()
        {
            if (GeometryType == GeometryVariant.Contour | GeometryType == GeometryVariant.Line) return this.Geometry as Point3d[];
            return null;
        }

        public Extents3d? Bounds { get; set; }

        public bool Contains(ObjectWithGeometry? otherGeometry)
        {
            if (otherGeometry == null || otherGeometry.Geometry == null) return false;
            if (otherGeometry.GeometryType == GeometryVariant.Point)
            {
                if (this.GeometryType != GeometryVariant.Contour || this.AsPoints() == null) return false;
                Point3d? targetPointRaw = otherGeometry.AsPoint();
                if (targetPointRaw == null) return false;
                Point3d targetPoint = targetPointRaw.Value;

                if (Bounds != null)
                {
                    if (
                        Bounds.Value.MinPoint.X! <= targetPoint.X ||
                        Bounds.Value.MinPoint.Y! <= targetPoint.Y ||
                        Bounds.Value.MinPoint.Z! <= targetPoint.Z ||
                        Bounds.Value.MaxPoint.X! >= targetPoint.X ||
                        Bounds.Value.MaxPoint.Y! >= targetPoint.Y ||
                        Bounds.Value.MaxPoint.Z! >= targetPoint.Z) return false;
                }
                return PointInContourChecking.IsPointInPolygon(targetPoint, this.AsPoints());
            }
            else if (otherGeometry.GeometryType == GeometryVariant.Contour | otherGeometry.GeometryType == GeometryVariant.Line)
            {
                Point3d[]? targetLine = otherGeometry.AsPoints();
                if (targetLine == null) return false;

                for (int i = 0; i < targetLine.Length; i++)
                {
                    if (!Contains(targetLine[i])) return false;
                }

                return true;
            }

            return false;
        }
    }
}
