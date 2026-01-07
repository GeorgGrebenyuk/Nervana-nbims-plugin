using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public class ShadowResult
    {
        public Point3d ShadowEnd { get; set; }      // End point of shadow on ground
        public double ShadowLength { get; set; }    // Length of shadow
        public double ShadowDirectionX { get; set; } // Shadow direction vector X
        public double ShadowDirectionY { get; set; } // Shadow direction vector Y
        public double ShadowAngle { get; set; }     // Angle of shadow from North (degrees)
    }

}
