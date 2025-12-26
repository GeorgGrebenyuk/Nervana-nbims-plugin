using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NervanaNcMgd.Functions
{
    internal class EParameter 
    {
        public bool IsCategory { get; set; } = false;
        public EParameter_Type PType { get; set; } = EParameter_Type.Common;
        public EValue_Type VType { get; set; } = EValue_Type.Common;
        public string Caption { get; set; }
        public object? Value { get; set; } = null;

        public string CanDeep 
        { 
            get
            {
                if (PType == EParameter_Type.CanExplore) return "+";
                else if (IsCategory) return "Cat";
                else return "";
            }
        }

        public EParameter(string caption, object? data = null)
        {
            Caption = caption;
            Value = data;
        }
    }

    internal class EParametersGroup
    {
        public string GroupName { get; set; }
        public List<EParameter> Parameters { get; set; }

        public EParametersGroup(string groupName)
        {
            GroupName = groupName;
            this.Parameters = new List<EParameter>();
        }
    }

    internal class ETreeItem
    {
        public string Name { get; set; }
        public List<ETreeItem> Items { get; set; }
        public int Tag { get; set; } //индекс будет соответствовать номеру в Properties

        public ETreeItem(string name, int tag)
        {
            this.Name = name;
            this.Tag = tag;
            this.Items = new List<ETreeItem>();
        }
    }

    internal class MgdExplorerReflection_ExplorerStructure
    {
        public List<ETreeItem> TreeStructure { get; set; }
        public List<EParametersGroup[]> PropertiesStructure { get; set; }

        public MgdExplorerReflection_ExplorerStructure()
        {
            TreeStructure = new List<ETreeItem>();
            PropertiesStructure = new List<EParametersGroup[]>();
        }

    }
}
