using System;
using System.IO;

namespace NervanaCommonMgd
{
    public class AppConfiguration
    {
        public const string AppFolderName = "Nervana nanoCAD plugin";
        private const string AppFunctionsConfigsFolderName = "Configs";

        private AppConfiguration()
        {

        }

        public static AppConfiguration CreateInstance()
        {
            if (instance == null) instance = new AppConfiguration();
            return instance;
        }

        public static string GetAppFolderPath()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppFolderName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static string GetConfigsFolderPath()
        {
            string path = Path.Combine(GetAppFolderPath(), AppFunctionsConfigsFolderName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static AppConfiguration? instance = null;
    }
}
