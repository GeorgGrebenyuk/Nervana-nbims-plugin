using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public class SunEvents
    {
        /// <summary>
        /// Время восхода солнца
        /// </summary>
        public DateTime Sunrise { get; set; }

        /// <summary>
        /// Время заката солнца
        /// </summary>
        public DateTime Sunset { get; set; }

        /// <summary>
        /// Время полудня
        /// </summary>
        public DateTime SolarNoon { get; set; }
    }
}
