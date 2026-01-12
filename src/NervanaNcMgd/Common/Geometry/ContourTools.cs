using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using Teigha.Geometry;

using NervanaCommonMgd;
using NervanaNcMgd.Common;
using NervanaNcMgd.Common.Extensions;

namespace NervanaNcMgd.Common.Geometry
{
    public class ContourTools
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

            using (Transaction tr = CommonUtils.CurrentDoc.Database.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable? acBlkTbl;
                acBlkTbl = tr.GetObject(CommonUtils.CurrentDoc.Database.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return null;

                // Open the Block table record Model space for write
                BlockTableRecord? acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;
                if (acBlkTblRec == null) return null;

                Point3d[]? targetPs = null;
                foreach (Entity offsetedPlineObject in offsetedPlines)
                {
                    Polyline? offsetedPline = offsetedPlineObject as Polyline;
                    if (offsetedPline == null) continue;

                    acBlkTblRec.AppendEntity(offsetedPline);
                    tr.AddNewlyCreatedDBObject(offsetedPline, true);

                    targetPs = offsetedPline.ToVertexes();
                }
                tr.Commit();

                return targetPs;
            }
        }

        private Point3d[] mVertexes;
    }
}
