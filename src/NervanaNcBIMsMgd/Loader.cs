using Teigha.Runtime;

namespace NervanaNcBIMsMgd
{
    public class Loader : IExtensionApplication
    {
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


        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void Terminate()
        {
            throw new System.NotImplementedException();
        }
    }
}
