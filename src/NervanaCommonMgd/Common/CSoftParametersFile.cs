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

    [XmlRoot(ElementName = "PARAMETERS")]
    public partial class CSoftParametersFile
    {
        public class CategoryDefinition
        {
            [XmlAttribute("order")]
            public int Order { get; set; }

            [XmlAttribute("categoryOrder")]
            public int CategoryOrder { get; set; }

            [XmlText]
            public string CategoryName { get; set; }
        }

        public class CategoryDefinitionColletion
        {
            [XmlElement("CATEGORY")]
            public List<CategoryDefinition> CategoriesList { get; set; }

            public CategoryDefinitionColletion()
            {
                CategoriesList = new List<CategoryDefinition>();
            }
        }

        public class ValueDefinition
        {
            [XmlElement("DATA")]
            public object Data { get; set; }

            [XmlElement("COMMENT")]
            public string? Caption { get; set; }
        }

        public class ValueDefinitionColletion
        {
            [XmlElement("VALUE")]
            public List<ValueDefinition> ValuesList { get; set; }

            public ValueDefinitionColletion()
            {
                ValuesList = new List<ValueDefinition>();
            }
        }



        public class ParameterDefinition
        {
            [XmlAttribute("type")]
            public int ParamTypeRaw { get; set; }

            [XmlIgnore]
            public CSoftParameterTypeVariant ParamType
            {
                get
                {
                    return (CSoftParameterTypeVariant)Enum.Parse(typeof(CSoftParameterTypeVariant), ParamTypeRaw.ToString());
                }
                set
                {
                    ParamTypeRaw = (int)value;
                }
            }

            [XmlAttribute("prescision")]
            public int Prescision { get; set; }

            [XmlAttribute("readonly")]
            public int ReadOnly { get; set; }

            [XmlIgnore]
            public bool IsReadOnly
            {
                get
                {
                    if (ReadOnly == 1) return true;
                    return false;
                }
                set
                {
                    if (value) ReadOnly = 1;
                    else ReadOnly = 0;
                }
            }

            [XmlAttribute("accuracy")]
            public int Accuracy { get; set; }

            [XmlAttribute("valueType")]
            public int ValueTypeRaw { get; set; }

            [XmlIgnore]
            public CSoftParameterTypeVariant ValueType
            {
                get
                {
                    return (CSoftParameterTypeVariant)Enum.Parse(typeof(CSoftParameterTypeVariant), ValueTypeRaw.ToString());
                }
                set
                {
                    ValueTypeRaw = (int)value;
                }
            }


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

            [XmlElement("CATEGORIES")]
            public CategoryDefinitionColletion Categories { get; set; }

            [XmlElement("VALUES")]
            public ValueDefinitionColletion? Values { get; set; }

            public static ParameterDefinition CreateDefault()
            {
                ParameterDefinition paramDef = new ParameterDefinition();
                paramDef.ParamTypeRaw = (int)CSoftParameterTypeVariant.String;
                paramDef.Prescision = -1;
                paramDef.IsReadOnly = false;
                paramDef.Accuracy = -1;
                paramDef.ValueTypeRaw = (int)CSoftParameterTypeVariant.String;
                paramDef.SysNameMeasureUnit = "";
                paramDef.SysNameMeasureUnitBase = "";

                paramDef.Categories = new CategoryDefinitionColletion();

                return paramDef;

            }

            public ParameterDefinition()
            {

            }
        }

        [XmlElement("PARAMETER")]
        public List<ParameterDefinition> Parameters { get; set; }

        public CSoftParametersFile()
        {
            Parameters = new List<ParameterDefinition>();
        }

        public static CSoftParametersFile? LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {

                    var serializer = new XmlSerializer(typeof(CSoftParametersFile));
                    return (CSoftParametersFile)serializer.Deserialize(stream);
                }
            }
            return null;
        }

        public void Save(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(typeof(CSoftParametersFile));
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }

    }
}
