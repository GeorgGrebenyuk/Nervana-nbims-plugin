using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

using NervanaNcBIMsMgd.Extensions;

namespace NervanaNcBIMsMgd.Geometry
{
    internal class ContourTools
    {
        public ContourTools(Point3d[] vertexes)
        {
            mVertexes = vertexes;
        }

        public Point3d[]? OffsetTo(double offset)
        {
            Point3dCollection vertexes2 = new Point3dCollection();
            DoubleCollection doubles = new DoubleCollection();
            foreach (var v in mVertexes)
            {
                vertexes2.Add(v);
                doubles.Add(0.0);
            }

            Polyline2d pline2d = new Polyline2d(Poly2dType.SimplePoly, vertexes2, 0.0, true, 0.0, 0.0, doubles);
            DBObjectCollection? offsetedPlines = null;
            try
            {
                offsetedPlines = pline2d.GetOffsetCurves(offset);
            }
            catch { }

            if (offsetedPlines == null) return null;

            using Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction();

            foreach (Entity offsetedPlineObject in offsetedPlines)
            {
                Polyline? offsetedPline = offsetedPlineObject as Polyline;
                if (offsetedPline == null) continue;

                BIMStructureMgd.Common.Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, offsetedPline);
                tr.Commit();

                return offsetedPline.ToVertexes();
            }
            return null;
        }

        private Point3d[] mVertexes;
    }
}
