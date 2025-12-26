using NervanaNcMgd.UI.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Teigha.DatabaseServices;

namespace NervanaNcMgd.Functions
{
    /// <summary>
    /// Auxiliary class for ExplorerSpace.xaml
    /// </summary>
    internal class MgdExplorerReflection_Handler
    {
        private enum SelectedProcess
        {
            ExploreInto,
            Copy
        }
        private MgdExplorerReflection_Handler() { }
        public MgdExplorerReflection_Handler(object? data)
        {
            //p_Mode = mode;
            p_Data = data;

            p_ExplorerStructure = new MgdExplorerReflection_ExplorerStructure();
            p_ExplorerStructure = MgdExplorerReflection.CreateInstance().Process_Object(data);
        }

        private object? ConvertType(object? source, out bool is_converted)
        {
            is_converted = false;
            if (source == null) return null;
            object converted = source;
            Type sourceType = source.GetType();
            dynamic convertedObj = converted;
            if (sourceType.IsInstanceOfType(typeof(IDictionary)))
            {
                is_converted = true;
                IDictionary convertedObj_Dict = (IDictionary)converted;
                var keys = convertedObj_Dict.Keys.Cast<object>().ToArray();
                var values = convertedObj_Dict.Values.Cast<object>().ToArray();
                List<string> items = new List<string>();
                for (int i = 0; i < keys.Length; i++)
                {
                    items.Add($"{keys[i]}\t{values[i]}");
                }
                if (items.Any()) converted = string.Join(Environment.NewLine, items);
            }
            else if (sourceType.IsInstanceOfType(typeof(IEnumerable)) | (sourceType != null && sourceType.Name.Contains("[]")))
            {
                is_converted = true;
                List<string> items = new List<string>();
                
                try
                {
                    foreach (var item in convertedObj)
                    {
                        items.Add(item.ToString());
                    }
                }
                catch
                {

                }

                if (items.Any()) converted = string.Join(Environment.NewLine, items);
            }

            return converted;
        }

        private void ProcessSelected(SelectedProcess mode, object selectedItem)
        {
            if (selectedItem == null) return;
            EParameter? sel_value = selectedItem as EParameter;
            if (sel_value == null) return;

            if (mode == SelectedProcess.ExploreInto)
            {
                if (sel_value != null && !sel_value.IsCategory && sel_value.PType == EParameter_Type.CanExplore)
                {
                    Nervana_ExplorerSpace newWindow = new Nervana_ExplorerSpace(sel_value.Value);
                    newWindow.Show();
                }
                else
                {
                    bool is_converted = false;
                    object? converted_value = ConvertType(sel_value?.Value, out is_converted);
                   
                    if (is_converted && converted_value != null) MessageBox.Show(converted_value.ToString(), "The content", MessageBoxButton.OK);
                    //Convert types 
                    //Object[] ...
                    //Dictionary<object, object> ...
                }
            }
            else if (mode == SelectedProcess.Copy && sel_value.Value != null)
            {
                bool is_converted = false;
                object? converted_value = ConvertType(sel_value.Value, out is_converted);
                if (converted_value != null) System.Windows.Clipboard.SetText(converted_value.ToString());
            }
        }

        public void ExploreValue(object selectedItem)
        {
            ProcessSelected(SelectedProcess.ExploreInto, selectedItem);
        }

        public void CopySingleValue(object selectedItem)
        {
            ProcessSelected(SelectedProcess.Copy, selectedItem);
        }

        public void CopyAllValues(int tag)
        {
            var props = GetData(tag);
            if (props == null) return;

            bool is_converted = false;

            StringBuilder sb = new StringBuilder();
            foreach (var group in props)
            {
                sb.AppendLine("---" + group.GroupName + "---");
                foreach (var item in group.Parameters)
                {
                    string val = "";
                    object? convertedValue = ConvertType(item.Value, out is_converted);
                    if (convertedValue != null) val = convertedValue?.ToString() ?? "";
                    sb.AppendLine(item.Caption + "\t" + val);
                }
            }
            System.Windows.Clipboard.SetText(sb.ToString());

        }

        public EParametersGroup[]? GetData(int tag)
        {
            return p_ExplorerStructure?.PropertiesStructure[tag] ?? null;
        }


        public List<ETreeItem> Items { get { return this.p_ExplorerStructure?.TreeStructure ?? new List<ETreeItem>(); } }

        private MgdExplorerReflection_ExplorerStructure? p_ExplorerStructure = null;
        //private MgdMode p_Mode;
        private object? p_Data;
    }
}
