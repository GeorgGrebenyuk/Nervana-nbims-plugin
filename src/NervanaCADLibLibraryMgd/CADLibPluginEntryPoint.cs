using CADLib;

namespace NervanaCADLibLibraryMgd
{
    public class CADLibPluginEntryPoint
    {
        public static ICADLibPlugin RegisterPlugin(PluginsManager manager)
        {
            CADLibData.CADLIB_MainForm = manager.MainForm;
            CADLibData.CADLIB_Library = manager.Library;
            CADLibData.CADLIB_mainDBBrowser = manager.MainDBBrowser;

            return new Nervana_CADLibLibraryLoader();
        }
    }
}
