using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

namespace NervanaNcMgd.Common.Geometry
{
    public static class CommonTools
    {
        public static (Point3dCollection, DoubleCollection) ConvertVerticesFromList(List<Point3d> vertices)
        {
            // Если конечная точка = нчальной, то надо пропустить
            int arrlength = vertices.Count;
            if (vertices[0] == vertices.Last()) arrlength -= 1;

            Point3d[] pointsTmp = new Point3d[arrlength];
            DoubleCollection doubles = new DoubleCollection(vertices.Count);
            for (int i = 0; i < arrlength; i++)
            {
                pointsTmp[i] = vertices[i];
                doubles.Add(0);
            }

            Point3dCollection points = new Point3dCollection(pointsTmp);
            return (points, doubles);
        }
    }
}
