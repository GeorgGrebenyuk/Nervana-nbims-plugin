using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcMgd.Common.Geometry
{
    public class ContourSegment
    {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public double Level { get; set; }

        public ContourSegment(Point3d start, Point3d end, double level)
        {
            Start = start;
            End = end;
            Level = level;
        }
    }

    

    public class IsolineGenerator
    {
        public List<ContourSegment> GenerateIsolines(DelaunayTriangulation.Triangle[] triangles, List<double> levels)
        {
            var contours = new List<ContourSegment>();

            foreach (var triangle in triangles)
            {
                foreach (var level in levels)
                {
                    if (triangle.ContainsContour(level))
                    {
                        var segments = GenerateContourInTriangle(triangle, level);
                        contours.AddRange(segments);
                    }
                }
            }

            return contours;
        }

        private List<ContourSegment> GenerateContourInTriangle(DelaunayTriangulation.Triangle triangle, double level)
        {
            var intersections = new List<Point3d>();
            var edges = new List<(Point3d start, Point3d end, double valStart, double valEnd)>()
            {
                (triangle.A, triangle.B, triangle.A.Z, triangle.B.Z),
                (triangle.A, triangle.C, triangle.A.Z, triangle.C.Z),
                (triangle.C, triangle.B, triangle.C.Z, triangle.B.Z)
            };

            // Find intersections on edges
            foreach (var edge in edges)
            {
                if ((edge.valStart <= level && edge.valEnd >= level) ||
                    (edge.valStart >= level && edge.valEnd <= level))
                {
                    if (edge.valStart == edge.valEnd)
                    {
                        // Edge lies exactly on the contour level
                        // Add both endpoints
                        intersections.Add(edge.start);
                        intersections.Add(edge.end);
                    }
                    else
                    {
                        // Interpolate intersection point
                        double t = (level - edge.valStart) / (edge.valEnd - edge.valStart);
                        var intersection = Lerp(edge.start, edge.end, t);
                        intersections.Add(intersection);
                    }
                }
            }

            var segments = new List<ContourSegment>();

            // Create segments from intersections
            // In a triangle, we can have either:
            // - 2 intersections: single segment
            // - 3 intersections: triangle exactly on level (rare)
            // - 4+ intersections: edge exactly on level
            if (intersections.Count == 2)
            {
                segments.Add(new ContourSegment(intersections[0], intersections[1], level));
            }
            else if (intersections.Count >= 3)
            {
                // Handle cases where vertices lie exactly on contour level
                // Connect points to form closed contour within triangle
                for (int i = 0; i < intersections.Count - 1; i += 2)
                {
                    if (i + 1 < intersections.Count)
                    {
                        segments.Add(new ContourSegment(intersections[i], intersections[i + 1], level));
                    }
                }
            }

            return segments;
        }

        private static Point3d Lerp(Point3d a, Point3d b, double t)
        {
            // When t = 0, returns 'a'
            // When t = 1, returns 'b'
            // When t = 0.5, returns the midpoint between 'a' and 'b'
            return a + (b - a) * t;
        }

        // Optional: Connect segments into continuous polylines
        public List<List<Point3d>> ConnectSegmentsIntoPolylines(List<ContourSegment> segments, double tolerance = 0.0001)
        {
            var polylines = new List<List<Point3d>>();
            var usedSegments = new bool[segments.Count];

            for (int i = 0; i < segments.Count; i++)
            {
                if (!usedSegments[i])
                {
                    var polyline = new List<Point3d>();
                    var currentSegment = segments[i];
                    usedSegments[i] = true;

                    // Start with first segment
                    polyline.Add(currentSegment.Start);
                    polyline.Add(currentSegment.End);

                    // Try to extend from end
                    bool extended;
                    do
                    {
                        extended = false;
                        for (int j = 0; j < segments.Count; j++)
                        {
                            if (!usedSegments[j] && segments[j].Level == currentSegment.Level)
                            {
                                var lastPoint = polyline[polyline.Count - 1];

                                if (PointsEqual(lastPoint, segments[j].Start, tolerance))
                                {
                                    polyline.Add(segments[j].End);
                                    usedSegments[j] = true;
                                    extended = true;
                                    break;
                                }
                                else if (PointsEqual(lastPoint, segments[j].End, tolerance))
                                {
                                    polyline.Add(segments[j].Start);
                                    usedSegments[j] = true;
                                    extended = true;
                                    break;
                                }
                            }
                        }
                    } while (extended);

                    // Try to extend from start (in reverse)
                    do
                    {
                        extended = false;
                        for (int j = 0; j < segments.Count; j++)
                        {
                            if (!usedSegments[j] && segments[j].Level == currentSegment.Level)
                            {
                                var firstPoint = polyline[0];

                                if (PointsEqual(firstPoint, segments[j].End, tolerance))
                                {
                                    polyline.Insert(0, segments[j].Start);
                                    usedSegments[j] = true;
                                    extended = true;
                                    break;
                                }
                                else if (PointsEqual(firstPoint, segments[j].Start, tolerance))
                                {
                                    polyline.Insert(0, segments[j].End);
                                    usedSegments[j] = true;
                                    extended = true;
                                    break;
                                }
                            }
                        }
                    } while (extended);

                    polylines.Add(polyline);
                }
            }

            return polylines;
        }

        private bool PointsEqual(Point3d a, Point3d b, double tolerance)
        {
            return Math.Abs(a.X - b.X) < tolerance &&
                   Math.Abs(a.Y - b.Y) < tolerance &&
                   Math.Abs(a.Z - b.Z) < tolerance;
        }
    }

}
