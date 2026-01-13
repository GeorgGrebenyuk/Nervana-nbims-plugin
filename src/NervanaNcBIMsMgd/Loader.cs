using System.Linq;
using System;
using System.Runtime.InteropServices;

using Teigha.Runtime;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;

using BIMStructureMgd.DatabaseObjects;
using System.Net.NetworkInformation;

using NervanaNcBIMsMgd.UI.Windows;
using NervanaNcBIMsMgd.Functions;
using NervanaCommonMgd.Configs;

namespace NervanaNcBIMsMgd
{

    public class Loader : IExtensionApplication
    {
        public void Initialize()
        {

        }

        public void Terminate()
        {

        }

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

        [CommandMethod("Nervana_Room_CreateByAutoContour")]
        public void command_Nervana_Room_CreateByAutoContour()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_CreateByAutoContour).Start();
        }

        [CommandMethod("Nervana_Room_CopyObjectsTo")]
        public void command_Nervana_Room_CopyObjectsTo()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_CopyObjectsTo).Start();
        }

        [CommandMethod("Nervana_Room_CreteByFloors")]
        public void command_Nervana_Room_CreteByFloors()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_CreateByFloors).Start();
        }

        [CommandMethod("Nervana_Room_ToFloors")]
        public void command_Nervana_Room_ToFloors()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_ToFloors).Start();
        }

        [CommandMethod("Nervana_Room_CreateByPlines")]
        public void command_Nervana_Room_CreateByPlines()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_CreateByPlines).Start();
        }

        [CommandMethod("Nervana_Room_LinkWalls")]
        public void command_Nervana_Room_LinkWalls()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_LinkWalls).Start();
        }

        [CommandMethod("Nervana_Room_LinkObjects")]
        public void command_Nervana_Room_LinkObjects()
        {
            RoomFuncs.CreateFor(RoomFuncVariant.Nervana_Room_LinkObjects).Start();
        }

        
        #endregion

        #region Команды в группе "Утилиты"
        [CommandMethod("Nervana_MakeElevationConceptual")]
        public void command_Nervana_MakeElevationConceptual()
        {
            Tin2Conceptual func = new Tin2Conceptual();
            ElevationImporterSettings sett = func.InitSettings();
            func.Import(sett);
        }

        [CommandMethod("Nervana_OpeningsPlacer")]
        public void command_Nervana_OpeningsPlacer()
        {
            OpeningPlacer func = new OpeningPlacer();
            func.SetPlaces();
            func.Start();
        }

        [CommandMethod("Nervana_AssemblyRefsExplorer")]
        public void command_Nervana_AssemblyRefsExplorer()
        {
            NervanaUI_PaletteManager.CreatePalette(PaletteType.AssemblyRefsExplorer);
        }

        #endregion


        #region Команды в группе "Аналитика"
        [CommandMethod("Nervana_Analytic_ShadowsBySunCreator")]
        public void command_Nervana_Analytic_ShadowsBySunCreator()
        {
            ShadowsBySunCreator func = new ShadowsBySunCreator();
            ShadowCalcParametersConfig settings = func.InitParameters();
            func.Start(settings);
        }
        #endregion





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
