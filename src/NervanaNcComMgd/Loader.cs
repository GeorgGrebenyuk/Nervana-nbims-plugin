using Teigha.Runtime;

namespace NervanaNcComMgd
{
    public class Loader : IExtensionApplication
    {
        [CommandMethod("Nervana_Parameters_ExplorerPalette", CommandFlags.UsePickSet)]
        public void command_Nervana_Parameters_ExplorerPalette()
        {
            NervanaUI_PaletteManager.CreatePalette(PaletteType.ParametersExplorerSpace);
        }


        public void Initialize()
        {
            
        }

        public void Terminate()
        {

        }
    }
}
