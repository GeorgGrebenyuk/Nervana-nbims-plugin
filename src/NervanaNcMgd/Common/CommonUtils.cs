using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;

namespace NervanaNcMgd.Common
{
    public class CommonUtils
    {
        public static Document CurrentDoc => HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

        public static ObjectId AddEntityToModelSpace(Transaction tr, Entity ent)
        {
            // Open the Block table for read
            BlockTable? acBlkTbl;
            acBlkTbl = tr.GetObject(CommonUtils.CurrentDoc.Database.BlockTableId,
                                            OpenMode.ForRead) as BlockTable;
            if (acBlkTbl == null) return ObjectId.Null;

            // Open the Block table record Model space for write
            BlockTableRecord? acBlkTblRec;
            acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                            OpenMode.ForWrite) as BlockTableRecord;
            if (acBlkTblRec == null) return ObjectId.Null;

            ObjectId createdId = acBlkTblRec.AppendEntity(ent);
            tr.AddNewlyCreatedDBObject(ent, true);

            return createdId;
        }
    }
}
