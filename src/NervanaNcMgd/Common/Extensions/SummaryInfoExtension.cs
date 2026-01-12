using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teigha.DatabaseServices;

namespace NervanaNcMgd.Common.Extensions
{
    public class ProjectTransformPararameters
    {
        public const string FieldX = "TRANSFORM_POINT_X";
        public const string FieldY = "TRANSFORM_POINT_Y";
        public const string FieldZ = "TRANSFORM_POINT_Z";
        public const string FieldAngle = "TRANSFORM_Angle";
        public double X;
        public double Y;
        public double Z;
        public double AngleRad;
        //public string ProjectedCoordinateSystem;

        public ProjectTransformPararameters()
        {
            X = 0;
            Y = 0;
            Z = 0;
            AngleRad = 0;
            //ProjectedCoordinateSystem = "";
        }

        public override string ToString()
        {
            return $"{X};{Y};{Z};{AngleRad}";
        }
    }

    internal static class SummaryInfoExtension
    {
        public static ProjectTransformPararameters GetTransformPararameters(this DatabaseSummaryInfo summaryInfo)
        {
            ProjectTransformPararameters transformParams = new ProjectTransformPararameters();
            IDictionaryEnumerator customProps = summaryInfo.CustomProperties;
            while (customProps.MoveNext())
            {
                if (customProps.Key == null || customProps.Value == null) continue;
                string propKey = customProps.Key.ToString() ?? "";
                string propData = customProps.Value.ToString() ?? "0.0";

                if (propKey == ProjectTransformPararameters.FieldX) transformParams.X = double.Parse(propData, CultureInfo.InvariantCulture);
                if (propKey == ProjectTransformPararameters.FieldY) transformParams.Y = double.Parse(propData, CultureInfo.InvariantCulture);
                if (propKey == ProjectTransformPararameters.FieldZ) transformParams.Z = double.Parse(propData, CultureInfo.InvariantCulture);
                if (propKey == ProjectTransformPararameters.FieldAngle) transformParams.AngleRad = double.Parse(propData, CultureInfo.InvariantCulture);
            }

            return transformParams;
            
        }
    }
}
