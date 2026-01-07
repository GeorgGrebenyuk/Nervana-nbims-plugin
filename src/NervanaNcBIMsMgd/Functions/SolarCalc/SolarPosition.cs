using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public class SolarPosition
    {
        public double Azimuth { get; set; }    // Degrees from North (0-360)
        public double Altitude { get; set; }   // Degrees above horizon (0-90)
        public double Zenith { get; set; }     // Degrees from vertical (90 - altitude)

        /// <summary>
        /// Для какого часа дня
        /// </summary>
        public int Hour { get; set; } = -1;
    }
}
