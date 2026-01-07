using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public class SunCalculator
    {
        private const double Deg2Rad = Math.PI / 180.0;
        private const double Rad2Deg = 180.0 / Math.PI;

        public static SunEvents CalculateSunEvents(DateTime date, double latitude, double longitude, int timeZoneOffset)
        {
            // This is a simplified calculation - for production use, consider more accurate algorithms
            DateTime utcDate = date.Date.ToUniversalTime().AddHours(-timeZoneOffset);

            // Approximate calculations (simplified)
            double julianDay = SunCalculator.CalculateJulianDay(utcDate);
            double julianCentury = (julianDay - 2451545.0) / 36525.0;

            // Simplified sunrise/sunset calculation
            double solarNoon = CalculateSolarNoon(longitude, timeZoneOffset, julianCentury);
            double hourAngle = CalculateSunriseHourAngle(latitude, julianCentury);

            DateTime solarNoonTime = date.Date.AddHours(solarNoon);
            TimeSpan sunriseOffset = TimeSpan.FromHours(-hourAngle / 15.0);
            TimeSpan sunsetOffset = TimeSpan.FromHours(hourAngle / 15.0);

            return new SunEvents
            {
                Sunrise = solarNoonTime + sunriseOffset,
                Sunset = solarNoonTime + sunsetOffset,
                SolarNoon = solarNoonTime
            };
        }

        private static double CalculateSolarNoon(double longitude, int timeZoneOffset, double julianCentury)
        {
            // Simplified solar noon calculation
            return (720 - 4 * longitude - timeZoneOffset * 60) / 1440.0 * 24.0;
        }

        private static double CalculateSunriseHourAngle(double latitude, double julianCentury)
        {
            double declination = CalculateDeclination(julianCentury);
            double latRad = latitude * Math.PI / 180.0;
            double decRad = declination * Math.PI / 180.0;

            // Sunrise hour angle (simplified - uses 0.833° for atmospheric refraction)
            double cosHourAngle = (Math.Cos(90.833 * Math.PI / 180.0) - Math.Sin(latRad) * Math.Sin(decRad))
                                / (Math.Cos(latRad) * Math.Cos(decRad));

            return Math.Acos(cosHourAngle) * 180.0 / Math.PI;
        }

        private static double CalculateDeclination(double julianCentury)
        {
            // Simplified declination calculation
            double obliq = 23.439 - 0.0000004 * julianCentury;
            double lambda = 280.466 + 36000.77 * julianCentury;
            return Math.Asin(Math.Sin(obliq * Math.PI / 180.0) * Math.Sin(lambda * Math.PI / 180.0)) * 180.0 / Math.PI;
        }

        public static List<SolarPosition>? GetSolarPositionsPerDay(DateTime dayOfYear, double latitude, double longitude, int timeZoneOffset)
        {
            List<SolarPosition> solarPos = new List<SolarPosition>();
            var sunInfo = CalculateSunEvents(dayOfYear, latitude, longitude, timeZoneOffset);

            for (int hour = sunInfo.Sunrise.Hour; hour <= sunInfo.Sunset.Hour; hour++)
            {
                DateTime time = new DateTime(dayOfYear.Year, dayOfYear.Month, dayOfYear.Day, hour, 0, 0);
                SolarPosition pos = SunCalculator.CalculateSunPosition(time, latitude, longitude, timeZoneOffset);
                pos.Hour = hour;
                solarPos.Add(pos);

                Debug.WriteLine($"{hour:00}:00 - Alt: {pos.Altitude:00.0}°, Az: {pos.Azimuth:000.0}°");
            }

            return solarPos;
        }

        public static SolarPosition CalculateSunPosition(DateTime dateTime, double latitude, double longitude, int timeZoneOffset = 0)
        {
            // Convert to UTC if local time is provided
            DateTime utcDateTime = dateTime.ToUniversalTime().AddHours(-timeZoneOffset);

            // Calculate Julian Day
            double julianDay = CalculateJulianDay(utcDateTime);

            // Calculate Julian Century
            double julianCentury = CalculateJulianCentury(julianDay);

            // Calculate geometric mean longitude, anomaly, and eccentricity
            double geomMeanLong = CalculateGeomMeanLongitude(julianCentury);
            double geomMeanAnom = CalculateGeomMeanAnomaly(julianCentury);
            double eccentEarthOrbit = CalculateEccentricityEarthOrbit(julianCentury);

            // Calculate equation of center, true longitude, and true anomaly
            double eqOfCenter = CalculateEquationOfCenter(julianCentury, geomMeanAnom);
            double trueLong = geomMeanLong + eqOfCenter;
            double trueAnom = geomMeanAnom + eqOfCenter;

            // Calculate apparent longitude
            double appLong = CalculateApparentLongitude(julianCentury, trueLong);

            // Calculate mean obliquity and oblique correction
            double meanObliqEcliptic = CalculateMeanObliquityOfEcliptic(julianCentury);
            double obliqCorrection = CalculateObliqueCorrection(julianCentury, meanObliqEcliptic);

            // Calculate right ascension and declination
            double rightAscension = CalculateRightAscension(appLong, obliqCorrection);
            double declination = CalculateDeclination(appLong, obliqCorrection);

            // Calculate equation of time
            double equationOfTime = CalculateEquationOfTime(julianCentury, geomMeanLong, geomMeanAnom, eccentEarthOrbit);

            // Calculate hour angle
            double hourAngle = CalculateHourAngle(utcDateTime, longitude, equationOfTime);

            // Calculate solar zenith and azimuth
            return CalculateSolarPosition(latitude, declination, hourAngle);
        }

        public static double CalculateJulianDay(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            double hour = date.Hour + date.Minute / 60.0 + date.Second / 3600.0;

            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            int A = year / 100;
            int B = 2 - A + (A / 4);

            return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5 + hour / 24.0;
        }

        private static double CalculateJulianCentury(double julianDay)
        {
            return (julianDay - 2451545.0) / 36525.0;
        }

        private static double CalculateGeomMeanLongitude(double julianCentury)
        {
            double l0 = 280.46646 + julianCentury * (36000.76983 + julianCentury * 0.0003032);
            return l0 % 360.0;
        }

        private static double CalculateGeomMeanAnomaly(double julianCentury)
        {
            return 357.52911 + julianCentury * (35999.05029 - 0.0001537 * julianCentury);
        }

        private static double CalculateEccentricityEarthOrbit(double julianCentury)
        {
            return 0.016708634 - julianCentury * (0.000042037 + 0.0000001267 * julianCentury);
        }

        private static double CalculateEquationOfCenter(double julianCentury, double geomMeanAnom)
        {
            double mRad = geomMeanAnom * Deg2Rad;
            return Math.Sin(mRad) * (1.914602 - julianCentury * (0.004817 + 0.000014 * julianCentury))
                 + Math.Sin(2 * mRad) * (0.019993 - 0.000101 * julianCentury)
                 + Math.Sin(3 * mRad) * 0.000289;
        }

        private static double CalculateApparentLongitude(double julianCentury, double trueLong)
        {
            return trueLong - 0.00569 - 0.00478 * Math.Sin((125.04 - 1934.136 * julianCentury) * Deg2Rad);
        }

        private static double CalculateMeanObliquityOfEcliptic(double julianCentury)
        {
            return 23.0 + (26.0 + (21.448 - julianCentury * (46.815 + julianCentury * (0.00059 - julianCentury * 0.001813))) / 60.0) / 60.0;
        }

        private static double CalculateObliqueCorrection(double julianCentury, double meanObliqEcliptic)
        {
            return meanObliqEcliptic + 0.00256 * Math.Cos((125.04 - 1934.136 * julianCentury) * Deg2Rad);
        }

        private static double CalculateRightAscension(double appLong, double obliqCorrection)
        {
            double lRad = appLong * Deg2Rad;
            double obRad = obliqCorrection * Deg2Rad;

            double ra = Math.Atan2(Math.Cos(obRad) * Math.Sin(lRad), Math.Cos(lRad));
            return NormalizeAngle(ra * Rad2Deg, 360.0);
        }

        private static double CalculateDeclination(double appLong, double obliqCorrection)
        {
            double lRad = appLong * Deg2Rad;
            double obRad = obliqCorrection * Deg2Rad;
            return Math.Asin(Math.Sin(obRad) * Math.Sin(lRad)) * Rad2Deg;
        }

        private static double CalculateEquationOfTime(double julianCentury, double geomMeanLong, double geomMeanAnom, double eccentEarthOrbit)
        {
            double epsilon = CalculateMeanObliquityOfEcliptic(julianCentury);
            double y = Math.Tan(epsilon * Deg2Rad / 2.0);
            y *= y;

            double sinM = Math.Sin(geomMeanAnom * Deg2Rad);
            double sin2M = Math.Sin(2 * geomMeanAnom * Deg2Rad);
            double sin3M = Math.Sin(3 * geomMeanAnom * Deg2Rad);
            double sin4M = Math.Sin(4 * geomMeanAnom * Deg2Rad);

            double Etime = y * sin2M - 2.0 * eccentEarthOrbit * sinM
                         + 4.0 * eccentEarthOrbit * y * sinM * Math.Cos(2 * geomMeanAnom * Deg2Rad)
                         - 0.5 * y * y * sin4M - 1.25 * eccentEarthOrbit * eccentEarthOrbit * sin2M;

            return Etime * Rad2Deg * 4.0; // Convert to minutes of time
        }

        private static double CalculateHourAngle(DateTime utcTime, double longitude, double equationOfTime)
        {
            double totalMinutes = utcTime.Hour * 60 + utcTime.Minute + utcTime.Second / 60.0;
            double trueSolarTime = (totalMinutes + equationOfTime + 4.0 * longitude) % 1440.0;

            double hourAngle = trueSolarTime / 4.0 - 180.0;
            if (hourAngle < -180.0) hourAngle += 360.0;

            return hourAngle;
        }

        private static SolarPosition CalculateSolarPosition(double latitude, double declination, double hourAngle)
        {
            double latRad = latitude * Deg2Rad;
            double decRad = declination * Deg2Rad;
            double haRad = hourAngle * Deg2Rad;

            // Calculate solar zenith angle
            double cosZenith = Math.Sin(latRad) * Math.Sin(decRad) + Math.Cos(latRad) * Math.Cos(decRad) * Math.Cos(haRad);
            double zenith = Math.Acos(cosZenith) * Rad2Deg;

            // Calculate solar altitude
            double altitude = 90.0 - zenith;

            // Calculate solar azimuth
            double cosAzimuth = (Math.Sin(decRad) - Math.Sin(latRad) * Math.Cos(zenith * Deg2Rad))
                              / (Math.Cos(latRad) * Math.Sin(zenith * Deg2Rad));

            double azimuth = Math.Acos(cosAzimuth) * Rad2Deg;

            // Adjust azimuth based on hour angle
            if (hourAngle > 0) azimuth = 360.0 - azimuth;

            return new SolarPosition
            {
                Altitude = altitude,
                Azimuth = azimuth,
                Zenith = zenith
            };
        }

        private static double NormalizeAngle(double angle, double range)
        {
            while (angle < 0) angle += range;
            while (angle >= range) angle -= range;
            return angle;
        }
    }
}
