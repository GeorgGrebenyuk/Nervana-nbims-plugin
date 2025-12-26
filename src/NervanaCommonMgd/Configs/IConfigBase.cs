using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NervanaCommonMgd.Configs
{
    public abstract class IConfigBase
    {
        public virtual void Save()
        {
            IConfigBase.SaveTo(null, this);
        }

        public static object? LoadFrom<ConfigType>(string? path)
        {
            if (path == null) path = GetDefaultPath<ConfigType>();
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    var serializer = new XmlSerializer(typeof(ConfigType));
                    object? serResult = serializer.Deserialize(stream);
                    return serResult;
                }
            }
            return null;
        }

        private static Tuple<string, object?>? LoadFromWithDialogue0<ConfigType>(string? initialDir = null)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор конфигурационного файла";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";

            if (initialDir != null && Directory.Exists(initialDir)) openFileDialog.InitialDirectory = initialDir;
            else openFileDialog.InitialDirectory = Path.Combine(AppConfiguration.GetConfigsFolderPath(), typeof(ConfigType).Name);

            if (!Directory.Exists(openFileDialog.InitialDirectory)) Directory.CreateDirectory(openFileDialog.InitialDirectory);

            if (openFileDialog.ShowDialog() == true && File.Exists(openFileDialog.FileName))
            {
                return Tuple.Create(openFileDialog.FileName, LoadFrom<ConfigType>(openFileDialog.FileName));
            }
            return null;
        }

        public static object? LoadFromWithDialogue<ConfigType>(string? initialDir = null)
        {
            Tuple<string, object?>? res = LoadFromWithDialogue0<ConfigType>();
            if (res == null) return null;
            return res.Item2;
        }

        public static object? LoadFromWithDialogue2<ConfigType>(ref string path)
        {
            Tuple<string, object?>? res = LoadFromWithDialogue0<ConfigType>();
            if (res == null) return null;
            path = res.Item1;
            return res.Item2;
        }


        public static void SaveTo<ConfigType>(string? path, ConfigType? objectData)
        {
            if (path == null) path = GetDefaultPath<ConfigType>();
            if (objectData == null) return;
            string? dir = Path.GetDirectoryName(path);

            if (dir == null) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            using (var writer = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(typeof(ConfigType));
                serializer.Serialize(writer, objectData);
                writer.Flush();
            }
        }

        public static string? SaveToWithDialogue<ConfigType>(ConfigType objectData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранение файла конфигурации";
            saveFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";
            saveFileDialog.AddExtension = true;

            saveFileDialog.InitialDirectory = Path.Combine(AppConfiguration.GetConfigsFolderPath(), typeof(ConfigType).Name);
            if (!Directory.Exists(saveFileDialog.InitialDirectory)) Directory.CreateDirectory(saveFileDialog.InitialDirectory);

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveTo(saveFileDialog.FileName, objectData);
                return saveFileDialog.FileName;
            }

            return null;
        }

        public static string GetDefaultPath<ConfigType>()
        {
            string dir = Path.Combine(AppConfiguration.GetConfigsFolderPath(), typeof(ConfigType).Name);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "Default.xml");
        }
    }
}
