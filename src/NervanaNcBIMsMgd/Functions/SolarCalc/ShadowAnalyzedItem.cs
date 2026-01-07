using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.Geometry;

namespace NervanaNcBIMsMgd.Functions.SolarCalc
{
    public class ShadowAnalyzedItem
    {
        public Point3d BasePosition { get; set; }  // Base of the column on ground
        public double Height { get; set; }         // Height of column

        public ShadowAnalyzedItem(double x, double y, double height)
        {
            BasePosition = new Point3d(x, y, 0);
            Height = height;
        }

        public ShadowAnalyzedItem(Point3d position)
        {
            BasePosition = new Point3d(position.X, position.Y, 0);
            Height = position.Z;
        }
    }
}
