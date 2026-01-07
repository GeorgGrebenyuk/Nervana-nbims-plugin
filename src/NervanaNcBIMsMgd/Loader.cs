using Teigha.Runtime;

using NervanaNcBIMsMgd.Functions;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;
using System.Linq;
using NervanaNcBIMsMgd.UI.Windows;
using System;
using BIMStructureMgd.DatabaseObjects;
using System.Runtime.InteropServices;
using Teigha.Geometry;

namespace NervanaNcBIMsMgd
{

    public class Loader : IExtensionApplication
    {
#if DEBUG
        [CommandMethod("Nervana_ParametersEditor", CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public void command_Nervana_ParametersEditor()
        {
            NervanaNcBIMsMgd.Functions.ParametersEditor.Palette.CreatePalette();
        }

        [CommandMethod("Nervana_1", CommandFlags.Redraw | CommandFlags.UsePickSet)]
        public void command_1()
        {
            HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("О-ла-ла");
        }
#endif

        #region Команды в группе "Помещения"

        [CommandMethod("Nervana_CopyObjectsToRoom")]
        public void command_Rooms_CopyObjectsToRoom()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_CopyObjectsToRoom).Start();
        }

        [CommandMethod("Nervana_Room2Floor")]
        public void command_Rooms_Room2Floor()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room2Floor).Start();
        }

        [CommandMethod("Nervana_Floor2Room")]
        public void command_Rooms_Floor2Room()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Floor2Room).Start();
        }

        [CommandMethod("Nervana_Polylines2Room")]
        public void command_Rooms_Polylines2Room()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Polylines2Room).Start();
        }

        [CommandMethod("Nervana_LinkWallsToRoom")]
        public void command_Rooms_LinkWallsToRoom()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_LinkWallsToRoom).Start();
        }

        [CommandMethod("Nervana_LinkObjectsToRoom")]
        public void command_Rooms_LinkObjectsToRoom()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_LinkObjectsToRoom).Start();
        }
        #endregion


        public void Initialize()
        {
            
        }

        public void Terminate()
        {
            
        }

        [CommandMethod("Nervana_AssemblyRefsExplorer")]
        public void command_Nervana_AssemblyRefsExplorer()
        {
            //NervanaUI_PaletteManager.CreatePalette(PaletteType.AssemblyRefsExplorer);
            NervanaUI_PaletteManager2.CreatePalette();
        }

        [CommandMethod("Nervana_OpeningsPlaces")]
        public void command_Nervana_OpeningsPlaces()
        {
            OpeningPlacer func = new OpeningPlacer();
            func.SetPlaces();
            func.Start();
        }

        



        #region Команды для курса по API

        [CommandMethod("command_CheckParametricObject")]
        public void command_CheckParametricObject()
        {
            Document acDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;

            ObjectIdCollection ids = Utils.SelectObjectsByTypes(null);
            if (ids.Count == 0)
            {
                ed.WriteMessage("Объекты не были выбраны!");
                return;
            }

            if (ids.Count > 1)
            {
                ed.WriteMessage("ВЫбрано более одного объектов!");
                return;
            }

            Database db = acDoc.Database;

            ObjectId idObj = ids[0];

            using Transaction tr = db.TransactionManager.StartTransaction();

            DBObject selObj = idObj.GetObject(OpenMode.ForRead);
            var dbParametricObject = selObj as BIMStructureMgd.ObjectProperties.IParametricObject;
            if (dbParametricObject == null)
            {
                ed.WriteMessage("Это НЕ параметрический объект!");
                return;
            }

            ed.WriteMessage("Это параметрический объект!");
        }

        [CommandMethod("command_CheckParametricObjectWithTypes")]
        public void command_CheckParametricObjectWithTypes()
        {
            Document acDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;

            Nervana_SelectEntityTypes2 window = new Nervana_SelectEntityTypes2();
            Type[] targetTypes = new Type[] { };
            if (HostMgd.ApplicationServices.Application.ShowModalWindow(window) == true)
            {
                targetTypes = window.SelectedTypes;
            }

            ObjectIdCollection ids = Utils.SelectObjectsByTypes(null);
            if (ids.Count == 0)
            {
                ed.WriteMessage("Объекты не были выбраны!");
                return;
            }

            if (ids.Count > 1)
            {
                ed.WriteMessage("ВЫбрано более одного объектов!");
                return;
            }

            Database db = acDoc.Database;

            ObjectId idObj = ids[0];

            using Transaction tr = db.TransactionManager.StartTransaction();

            DBObject selObj = idObj.GetObject(OpenMode.ForRead);
            if (targetTypes.Contains(selObj.GetType()))
            {
                ed.WriteMessage($"Это параметрический объект нужного типа {selObj.GetType().Name}!");
            }
            else ed.WriteMessage("Тип объекта не удовлетворяет условиям!");
            
        }
        #endregion
    }
}
