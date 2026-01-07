using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Geometry
{
    /// <summary>
    /// Powered by Deepseek
    /// </summary>
    public class DelaunayTriangulation
    {

        public class Triangle
        {
            public Point3d A { get; set; }
            public Point3d B { get; set; }
            public Point3d C { get; set; }

            public Triangle(Point3d a, Point3d b, Point3d c)
            {
                A = a;
                B = b;
                C = c;
            }

            public bool ContainsPoint(Point3d point)
            {
                return A.Equals(point) || B.Equals(point) || C.Equals(point);
            }

            public bool ContainsEdge(Point3d p1, Point3d p2)
            {
                return (A.Equals(p1) || B.Equals(p1) || C.Equals(p1)) &&
                       (A.Equals(p2) || B.Equals(p2) || C.Equals(p2));
            }

            public List<(Point3d, Point3d)> GetEdges()
            {
                return new List<(Point3d, Point3d)>
                {
                    (A, B),
                    (B, C),
                    (C, A)
                };
            }

            // Check if this triangle contains contour at specified level
            public bool ContainsContour(double level)
            {
                int above = 0;
                int below = 0;

                if (A.Z >= level) above++;
                else below++;

                if (B.Z >= level) above++;
                else below++;

                if (C.Z >= level) above++;
                else below++;

                return above > 0 && below > 0 && above < 3;
            }

            public override string ToString()
            {
                return $"[{A}, {B}, {C}]";
            }

            public override bool Equals(object? obj)
            {
                if (obj is Triangle other)
                {
                    var points1 = new List<Point3d> { A, B, C }.OrderBy(p => p.X).ThenBy(p => p.Y).ThenBy(p => p.Z).ToList();
                    var points2 = new List<Point3d> { other.A, other.B, other.C }.OrderBy(p => p.X).ThenBy(p => p.Y).ThenBy(p => p.Z).ToList();

                    return points1[0].Equals(points2[0]) && points1[1].Equals(points2[1]) && points1[2].Equals(points2[2]);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
            }
        }

        public static List<Triangle> Triangulate(IEnumerable<Point3d> points)
        {
            if (points == null || points.Count() < 3)
                throw new ArgumentException("At least 3 points are required for triangulation");

            // Create a super triangle that contains all points
            var superTriangle = CreateSuperTriangle(points);
            var triangles = new List<Triangle> { superTriangle };

            // Add points one by one
            foreach (var point in points)
            {
                var badTriangles = new List<Triangle>();

                // Find all triangles that are no longer valid due to the new point
                foreach (var triangle in triangles)
                {
                    if (IsPointInCircumcircle(triangle, point))
                    {
                        badTriangles.Add(triangle);
                    }
                }

                var polygon = new List<(Point3d, Point3d)>();

                // Find the boundary of the polygonal hole
                foreach (var triangle in badTriangles)
                {
                    foreach (var edge in triangle.GetEdges())
                    {
                        bool shared = false;
                        foreach (var otherTriangle in badTriangles)
                        {
                            if (!triangle.Equals(otherTriangle) && otherTriangle.ContainsEdge(edge.Item1, edge.Item2))
                            {
                                shared = true;
                                break;
                            }
                        }

                        if (!shared)
                        {
                            polygon.Add(edge);
                        }
                    }
                }

                // Remove bad triangles
                triangles.RemoveAll(t => badTriangles.Contains(t));

                // Create new triangles from the point to each edge of the polygon
                foreach (var edge in polygon)
                {
                    var newTriangle = new Triangle(edge.Item1, edge.Item2, point);
                    triangles.Add(newTriangle);
                }
            }

            // Remove triangles that contain vertices from the super triangle
            triangles.RemoveAll(t =>
                t.ContainsPoint(superTriangle.A) ||
                t.ContainsPoint(superTriangle.B) ||
                t.ContainsPoint(superTriangle.C));

            return triangles;
        }

        /// <summary>
        /// Calculates the external border (convex hull) of the Delaunay triangulation
        /// </summary>
        /// <param name="triangles">List of triangles from Delaunay triangulation</param>
        /// <returns>Ordered list of points representing the external border (convex hull)</returns>
        public static List<Point3d> CalculateExternalBorder(List<Triangle> triangles)
        {
            if (triangles == null || triangles.Count == 0)
                return new List<Point3d>();

            // Collect all unique edges and count their occurrences
            var edgeCount = new Dictionary<string, int>();
            var edgeToPoints = new Dictionary<string, (Point3d, Point3d)>();

            foreach (var triangle in triangles)
            {
                foreach (var edge in triangle.GetEdges())
                {
                    // Create a normalized key for the edge (always store in consistent order)
                    var normalizedEdge = NormalizeEdge(edge.Item1, edge.Item2);
                    var key = $"{normalizedEdge.Item1.X},{normalizedEdge.Item1.Y},{normalizedEdge.Item1.Z}-{normalizedEdge.Item2.X},{normalizedEdge.Item2.Y},{normalizedEdge.Item2.Z}";

                    if (edgeCount.ContainsKey(key))
                    {
                        edgeCount[key]++;
                    }
                    else
                    {
                        edgeCount[key] = 1;
                        edgeToPoints[key] = (edge.Item1, edge.Item2);
                    }
                }
            }

            // External edges are those that appear only once
            var externalEdges = edgeToPoints
                .Where(kvp => edgeCount[kvp.Key] == 1)
                .Select(kvp => kvp.Value)
                .ToList();

            if (externalEdges.Count == 0)
                return new List<Point3d>();

            return BuildOrderedBorder(externalEdges);
        }

        /// <summary>
        /// Normalizes an edge by ordering the points consistently
        /// </summary>
        private static (Point3d, Point3d) NormalizeEdge(Point3d p1, Point3d p2)
        {
            // Order points by X, then Y, then Z
            if (p1.X < p2.X ||
                (Math.Abs(p1.X - p2.X) < 1e-10 && p1.Y < p2.Y) ||
                (Math.Abs(p1.X - p2.X) < 1e-10 && Math.Abs(p1.Y - p2.Y) < 1e-10 && p1.Z < p2.Z))
            {
                return (p1, p2);
            }
            return (p2, p1);
        }

        /// <summary>
        /// Builds an ordered list of points from a collection of external edges
        /// </summary>
        private static List<Point3d> BuildOrderedBorder(List<(Point3d, Point3d)> externalEdges)
        {
            if (externalEdges.Count == 0)
                return new List<Point3d>();

            var borderPoints = new List<Point3d>();
            var edgeDict = new Dictionary<Point3d, List<Point3d>>();

            // Build adjacency dictionary
            foreach (var (p1, p2) in externalEdges)
            {
                if (!edgeDict.ContainsKey(p1))
                    edgeDict[p1] = new List<Point3d>();
                if (!edgeDict.ContainsKey(p2))
                    edgeDict[p2] = new List<Point3d>();

                edgeDict[p1].Add(p2);
                edgeDict[p2].Add(p1);
            }

            // Find starting point (point with minimum X, then minimum Y)
            var startPoint = edgeDict.Keys
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .First();

            Point3d? currentPoint = startPoint;
            Point3d? previousPoint = null;
            borderPoints.Add(startPoint);

            // Traverse the border
            do
            {
                if (currentPoint == null) break;

                var neighbors = edgeDict[currentPoint.Value];
                Point3d? nextPoint;

                if (previousPoint == null)
                {
                    // For first point, pick any neighbor
                    nextPoint = neighbors.First();
                }
                else
                {
                    // Find the neighbor that's not the previous point
                    nextPoint = neighbors.FirstOrDefault(n => !n.Equals(previousPoint));

                    // If all neighbors are previous points (shouldn't happen in convex hull), break
                    if (nextPoint == null)
                        break;
                }

                if (nextPoint != null) borderPoints.Add(nextPoint.Value);
                previousPoint = currentPoint;
                currentPoint = nextPoint;

            } while (!currentPoint.Equals(startPoint) && borderPoints.Count <= edgeDict.Count);

            // Remove the last point if it's the same as the first (closed loop)
            if (borderPoints.Count > 1 && borderPoints.Last().Equals(borderPoints.First()))
            {
                borderPoints.RemoveAt(borderPoints.Count - 1);
            }

            return borderPoints;
        }

        /// <summary>
        /// Alternative method to calculate external border directly from points using gift wrapping algorithm
        /// </summary>
        public static List<Point3d> CalculateConvexHull(IEnumerable<Point3d> points)
        {
            if (points == null || points.Count() < 3)
                return points?.ToList() ?? new List<Point3d>();

            // Find the point with the smallest Y coordinate (and smallest X if tie)
            var startPoint = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();
            var hull = new List<Point3d> { startPoint };

            var currentPoint = startPoint;
            Point3d nextPoint;

            do
            {
                nextPoint = points.ElementAt(0);

                foreach (var candidate in points)
                {
                    if (candidate.Equals(currentPoint) || candidate.Equals(nextPoint))
                        continue;

                    var crossProduct = CrossProduct(currentPoint, nextPoint, candidate);

                    if (nextPoint.Equals(currentPoint) || crossProduct > 0 ||
                        (Math.Abs(crossProduct) < 1e-10 &&
                         DistanceSquared(currentPoint, candidate) > DistanceSquared(currentPoint, nextPoint)))
                    {
                        nextPoint = candidate;
                    }
                }

                currentPoint = nextPoint;

                if (!currentPoint.Equals(startPoint))
                    hull.Add(currentPoint);

            } while (!currentPoint.Equals(startPoint) && hull.Count <= points.Count());

            return hull;
        }

        private static double CrossProduct(Point3d a, Point3d b, Point3d c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private static double DistanceSquared(Point3d a, Point3d b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        private static Triangle CreateSuperTriangle(IEnumerable<Point3d> points)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            double dx = maxX - minX;
            double dy = maxY - minY;
            double deltaMax = Math.Max(dx, dy);
            double midX = (minX + maxX) / 2;
            double midY = (minY + maxY) / 2;

            var p1 = new Point3d(midX - 20 * deltaMax, midY - deltaMax, 0);
            var p2 = new Point3d(midX, midY + 20 * deltaMax, 0);
            var p3 = new Point3d(midX + 20 * deltaMax, midY - deltaMax, 0);

            return new Triangle(p1, p2, p3);
        }

        private static bool IsPointInCircumcircle(Triangle triangle, Point3d point)
        {
            // Use the more numerically stable determinant method
            return IsPointInCircumcircleDeterminant(triangle, point);
        }

        private static bool IsPointInCircumcircleDeterminant(Triangle triangle, Point3d point)
        {
            Point3d a = triangle.A;
            Point3d b = triangle.B;
            Point3d c = triangle.C;
            Point3d d = point;

            double[,] matrix = {
            {a.X - d.X, a.Y - d.Y, (a.X - d.X) * (a.X - d.X) + (a.Y - d.Y) * (a.Y - d.Y)},
            {b.X - d.X, b.Y - d.Y, (b.X - d.X) * (b.X - d.X) + (b.Y - d.Y) * (b.Y - d.Y)},
            {c.X - d.X, c.Y - d.Y, (c.X - d.X) * (c.X - d.X) + (c.Y - d.Y) * (c.Y - d.Y)}
        };

            // Calculate determinant of the matrix
            double det = matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[2, 1] * matrix[1, 2]) -
                        matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[2, 0] * matrix[1, 2]) +
                        matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[2, 0] * matrix[1, 1]);

            // For counter-clockwise triangles, point is inside circumcircle if determinant > 0
            return det > 0;
        }
    }
}
