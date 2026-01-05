using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using HostMgd.EditorInput;
using HostMgd.ApplicationServices;

namespace NervanaNcBIMsMgd
{
    internal class Utils
    {
        public static Document CurrentDoc => HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        public static ObjectIdCollection SelectObjectsByTypes(Type[]? types, string message = "Выберите объекты ...")
        {
            Document acDoc = CurrentDoc;
            Editor ed = acDoc.Editor;

            ObjectIdCollection ids = new ObjectIdCollection();

            PromptSelectionResult selectionResult = acDoc.Editor.SelectImplied();
            SelectionSet SelSet;
            if (selectionResult.Status == PromptStatus.OK)
            {
                SelSet = selectionResult.Value;
                ids = new ObjectIdCollection(SelSet.GetObjectIds());
            }
            else
            {
                ed.WriteMessage(message);
                var SelSet_result = ed.GetSelection();
                if (SelSet_result.Status == PromptStatus.OK)
                {
                    SelSet = SelSet_result.Value;
                    foreach (SelectedObject acSSObj in SelSet)
                    {
                        if (acSSObj != null)
                        {
                            ids.Add(acSSObj.ObjectId);
                        }
                    }
                }
            }

            using Transaction tr = acDoc.Database.TransactionManager.StartTransaction();
            ObjectIdCollection objects = new ObjectIdCollection();
            foreach (ObjectId oneSelectedObjectId in ids)
            {
                DBObject obj = tr.GetObject(oneSelectedObjectId, OpenMode.ForRead) as DBObject;
                if (obj != null)
                {
                    Type t = obj.GetType();
                    bool canAdd = false;
                    if (types == null) canAdd = true;
                    else
                    {
                        foreach (Type type in types)
                        {
                            if (t.IsAssignableFrom(type))
                            {
                                canAdd = true;
                                break;
                            }
                        }
                    }
                    if (canAdd) objects.Add(oneSelectedObjectId);
                }
            }
            return objects;
        }

        public static double? GetUsersDoubleInput(string message)
        {
            Document acDoc = CurrentDoc;
            Editor ed = acDoc.Editor;

            PromptDoubleResult res = ed.GetDouble(message);
            if (res.Status == PromptStatus.OK) return res.Value;
            return null;
        }
    }
}
