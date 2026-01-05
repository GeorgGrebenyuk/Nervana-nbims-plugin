using Teigha.Runtime;

using NervanaNcBIMsMgd.Functions;

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
    }
}
