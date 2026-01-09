using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace NervanaCommonMgd.Common
{
    public enum CSoftParameterTypeVariant : int
    {
        String = 0,
        Integer = 2,
        Double = 3,
        TextField = 4, //memo in CL (Текстовое поле)
        ListWithType = 5, //wildlist in CL (Список с возможностью ввода)
        List = 6, // (Список значений)
        Calculated = 16, //(расчетный)
        URL = 17, //hyperling in CL (Гиперссылка)
        DynamicList = 21, //dynamicList in CL (Динамический список)
        RGBColor = 30,
        DateTime = 32
    }

    public partial class CSoftParametersFile
    {
        

        [DataContract(Name = "CATEGORY")]
        public class CategoryDefinition
        {
            [XmlAttribute("order")]
            public int Order { get; set; }

            [XmlAttribute("categoryOrder")]
            public int CategoryOrder { get; set; }

            [XmlText]
            public string CategoryName { get; set; }
        }

        [DataContract(Name = "VALUE")]
        public class ValueDefinition
        {
            [XmlElement("DATA")]
            public object Data { get; set; }

            [XmlElement("COMMENT")]
            public string? Caption { get; set; }
        }

        [DataContract(Name = "PARAMETER")]
        public class ParameterDefinition
        {
            [XmlAttribute("type")]
            public CSoftParameterTypeVariant ParamType { get; set; }

            [XmlAttribute("prescision")]
            public int Prescision { get; set; }

            [XmlAttribute("readonly")]
            public bool IsReadOnly { get; set; }

            [XmlAttribute("accuracy")]
            public int Accuracy { get; set; }

            [XmlAttribute("valueType")]
            public CSoftParameterTypeVariant ValueType { get; set; }

            [XmlAttribute("sysNameMeasureUnitBase")]
            public string SysNameMeasureUnitBase { get; set; }

            [XmlAttribute("sysNameMeasureUnit")]
            public string SysNameMeasureUnit { get; set; }


            [XmlElement("NAME")]
            public string Name { get; set; }

            [XmlElement("COMMENT")]
            public string? Caption { get; set; }

            [XmlElement("DEFAULT")]
            public string? DefaultValue { get; set; }

            [XmlArray("CATEGORIES")]
            public List<CategoryDefinition> Categories { get; set; }

            [XmlArray("VALUES")]
            public List<ValueDefinition>? Values { get; set; }

            public static ParameterDefinition CreateDefault()
            {
                ParameterDefinition paramDef = new ParameterDefinition();
                paramDef.ParamType = CSoftParameterTypeVariant.String;
                paramDef.Prescision = -1;
                paramDef.IsReadOnly = false;
                paramDef.Accuracy = -1;
                paramDef.ValueType = CSoftParameterTypeVariant.String;
                paramDef.SysNameMeasureUnit = "";
                paramDef.SysNameMeasureUnitBase = "";

                paramDef.Categories = new List<CategoryDefinition>();

                return paramDef;

            }
        }

        [XmlArray("PARAMETERS")]
        public List<ParameterDefinition> Parameters { get; set;}

        public CSoftParametersFile()
        {
            Parameters = new List<ParameterDefinition>();
        }

        public static CSoftParametersFile? LoadFrom(string path)
        {
            return (CSoftParametersFile?)NervanaCommonMgd.Configs.IConfigBase.LoadFrom<CSoftParametersFile>(path);
        }

        public void Save(string path)
        {
            NervanaCommonMgd.Configs.IConfigBase.SaveTo(path, this);
        }

    }
}
