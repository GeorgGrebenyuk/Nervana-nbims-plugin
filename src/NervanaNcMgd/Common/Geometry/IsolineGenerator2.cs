using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcMgd.Common.Geometry
{
    public class ContourLine
    {
        public List<Point2d> Points { get; set; } = new List<Point2d>();
        public double Elevation { get; set; }
    }

    public static class IsolineGenerator2
    {
        public static List<ContourLine> GenerateContours(double[,] grid, double interval, double baseValue = 0.0)
        {
            var contours = new Dictionary<double, ContourLine>();
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Process each cell in the grid
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < cols - 1; j++)
                {
                    double v0 = grid[i, j];     // Top-left
                    double v1 = grid[i, j + 1]; // Top-right
                    double v2 = grid[i + 1, j + 1]; // Bottom-right
                    double v3 = grid[i + 1, j]; // Bottom-left

                    // Determine all thresholds crossing this cell
                    var thresholds = GetCrossingThresholds(v0, v1, v2, v3, interval, baseValue);

                    foreach (double t in thresholds)
                    {
                        if (!contours.ContainsKey(t))
                        {
                            contours[t] = new ContourLine() { Elevation = t };
                        }

                        // Get edge intersections for this threshold
                        var intersections = GetEdgeIntersections(i, j, v0, v1, v2, v3, t);

                        // Add segments to the contour
                        if (intersections.left != null && intersections.top != null && intersections.right != null && intersections.bottom != null) ConnectSegments(contours[t], (intersections.left.Value, intersections.top.Value, intersections.right.Value, intersections.bottom.Value));

                    }
                }
            }
            return new List<ContourLine>(contours.Values);
        }

        private static List<double> GetCrossingThresholds(double v0, double v1, double v2, double v3, double interval, double baseValue)
        {
            var thresholds = new HashSet<double>();
            double minV = Math.Min(Math.Min(v0, v1), Math.Min(v2, v3));
            double maxV = Math.Max(Math.Max(v0, v1), Math.Max(v2, v3));

            int startIdx = Convert.ToInt32(Math.Ceiling((minV - baseValue) / interval));
            int endIdx = Convert.ToInt32(Math.Floor((maxV - baseValue) / interval));

            for (int k = (int)startIdx; k <= endIdx; k++)
            {
                double t = baseValue + k * interval;
                if (t >= minV && t <= maxV)
                {
                    thresholds.Add(t);
                }
            }
            return new List<double>(thresholds);
        }

        private static (Point2d? left, Point2d? top, Point2d? right, Point2d? bottom) GetEdgeIntersections(
            int i, int j, double v0, double v1, double v2, double v3, double threshold)
        {
            double x = j, y = i;
            Point2d? l = null, t = null, r = null, b = null;

            // Left edge (v0 -> v3)
            if ((v0 >= threshold) != (v3 >= threshold))
            {
                double ratio = (threshold - v0) / (v3 - v0);
                l = new Point2d(x, y + ratio);
            }

            // Top edge (v0 -> v1)
            if ((v0 >= threshold) != (v1 >= threshold))
            {
                double ratio = (threshold - v0) / (v1 - v0);
                t = new Point2d(x + ratio, y);
            }

            // Right edge (v1 -> v2)
            if ((v1 >= threshold) != (v2 >= threshold))
            {
                double ratio = (threshold - v1) / (v2 - v1);
                r = new Point2d(x + 1, y + ratio);
            }

            // Bottom edge (v3 -> v2)
            if ((v3 >= threshold) != (v2 >= threshold))
            {
                double ratio = (threshold - v3) / (v2 - v3);
                b = new Point2d(x + ratio, y + 1);
            }

            return (l, t, r, b);
        }

        private static void ConnectSegments(ContourLine contour,
            (Point2d left, Point2d top, Point2d right, Point2d bottom) intersections)
        {
            var points = new List<Point2d>
            {
                intersections.left,
                intersections.top,
                intersections.right,
                intersections.bottom
            };

            // Add unique non-null pairs as line segments
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (points[i] != null && points[j] != null)
                    {
                        bool exists = false;
                        for (int k = 0; k < contour.Points.Count - 1; k += 2)
                        {
                            if ((PointsEqual(contour.Points[k], points[i]) && PointsEqual(contour.Points[k + 1], points[j])) ||
                                (PointsEqual(contour.Points[k], points[j]) && PointsEqual(contour.Points[k + 1], points[i])))
                            {
                                exists = true;
                                break;
                            }
                        }
                        if (!exists)
                        {
                            contour.Points.Add(points[i]);
                            contour.Points.Add(points[j]);
                        }
                    }
                }
            }
        }

        private static bool PointsEqual(Point2d p1, Point2d p2, double tolerance = 1e-9)
        {
            return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance;
        }
    }
}
