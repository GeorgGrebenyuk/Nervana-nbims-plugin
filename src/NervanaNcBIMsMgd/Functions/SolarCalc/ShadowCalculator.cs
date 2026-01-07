using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public static class ShadowCalculator
    {
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;

        /// <summary>
        /// Calculate shadow for a vertical column based on solar position
        /// </summary>
        /// <param name="column">The vertical column</param>
        /// <param name="solarPosition">Solar position (altitude and azimuth)</param>
        /// <param name="groundElevation">Ground elevation (default 0)</param>
        /// <returns>Shadow result with end point and properties</returns>
        public static ShadowResult CalculateColumnShadow(ShadowAnalyzedItem column, SolarPosition solarPosition, double groundElevation = 0)
        {
            // Convert solar angles to radians
            double altitudeRad = solarPosition.Altitude * Deg2Rad;
            double azimuthRad = solarPosition.Azimuth * Deg2Rad;

            // If sun is below horizon, no shadow
            if (solarPosition.Altitude <= 0)
            {
                return new ShadowResult
                {
                    ShadowEnd = new Point3d(column.BasePosition.X, column.BasePosition.Y, groundElevation),
                    ShadowLength = 0,
                    ShadowDirectionX = 0,
                    ShadowDirectionY = 0,
                    ShadowAngle = 0
                };
            }

            // Calculate shadow length using trigonometry
            // shadow_length = height / tan(altitude)
            double shadowLength = column.Height / Math.Tan(altitudeRad);

            // Calculate shadow direction components
            // Azimuth: 0° = North, 90° = East, 180° = South, 270° = West
            double shadowDirectionX = Math.Sin(azimuthRad) * shadowLength;
            double shadowDirectionY = Math.Cos(azimuthRad) * shadowLength;

            // Calculate shadow end point
            // Shadow extends in the direction opposite to the sun's azimuth
            double shadowEndX = column.BasePosition.X - shadowDirectionX;
            double shadowEndY = column.BasePosition.Y - shadowDirectionY;

            return new ShadowResult
            {
                ShadowEnd = new Point3d(shadowEndX, shadowEndY, groundElevation),
                ShadowLength = shadowLength,
                ShadowDirectionX = -shadowDirectionX, // Direction FROM column TO shadow end
                ShadowDirectionY = -shadowDirectionY,
                ShadowAngle = solarPosition.Azimuth + 180 // Opposite direction of sun
            };
        }

        /// <summary>
        /// Check if a point is in shadow of a column
        /// </summary>
        public static bool IsPointInShadow(Point3d point, ShadowAnalyzedItem column, SolarPosition solarPosition, double tolerance = 0.1)
        {
            ShadowResult shadow = CalculateColumnShadow(column, solarPosition);

            // Simple line intersection check between point and shadow line
            // For production, use proper geometric intersection tests

            double dx = point.X - column.BasePosition.X;
            double dy = point.Y - column.BasePosition.Y;

            double distanceToColumn = Math.Sqrt(dx * dx + dy * dy);
            double shadowAngle = Math.Atan2(dy, dx) * Rad2Deg;

            // Normalize angles
            double normalizedShadowAngle = (shadow.ShadowAngle + 360) % 360;
            double normalizedPointAngle = (shadowAngle + 360) % 360;

            double angleDifference = Math.Abs(normalizedShadowAngle - normalizedPointAngle);
            angleDifference = Math.Min(angleDifference, 360 - angleDifference);

            return angleDifference < 5 && distanceToColumn <= shadow.ShadowLength + tolerance;
        }

        
        private static double Distance2D(Point3d a, Point3d b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        
    }
}
