using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

using NervanaNcBIMsMgd.Extensions;
using NervanaCommonMgd;

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
                offsetedPlines = pline2d.GetOffsetCurves(offset * -1.0);
            }
            catch { }
            if (offsetedPlines == null) return null;

            TraceWriter.Log($"GetOffsetCurves = {offsetedPlines.Count} шт. ", LogType.Add);

            using Transaction tr = Utils.CurrentDoc.Database.TransactionManager.StartTransaction();

            Point3d[]? targetPs = null;
            foreach (Entity offsetedPlineObject in offsetedPlines)
            {
                Polyline? offsetedPline = offsetedPlineObject as Polyline;
                if (offsetedPline == null) continue;

                BIMStructureMgd.Common.Utilities.AddEntityToDatabase(Utils.CurrentDoc.Database, tr, offsetedPline);
                

                targetPs = offsetedPline.ToVertexes();
            }
            tr.Commit();

            return targetPs;
            return null;
        }

        private Point3d[] mVertexes;
    }
}
