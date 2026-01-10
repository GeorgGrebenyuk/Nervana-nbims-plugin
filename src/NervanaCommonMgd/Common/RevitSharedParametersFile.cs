using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NervanaCommonMgd.Common
{
    /// <summary>
    /// Описание файла Общих параметров Revit
    /// </summary>
    public class RevitSharedParametersFile
    {
        public class MetadataDefinition
        {
            public Version Version { get; set; }

            public MetadataDefinition()
            {
                Version = new Version(2, 1);
            }
        }

        public class GroupDefinition
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"GROUP\t{Id}\t{Name}";
            }
        }


        public enum ParamDataTypeVariant
        {
            Text,
            Integer,
            Angle,
            Area,
            CostPerArea,
            Distance,
            Length,
            MassDensity,
            Number,
            RotationAngle,
            Slope,
            Speed,
            Time,
            Volume,
            Currency, // валюта
            URL,
            Material,
            FilePattern,
            Image,
            Boolean, //Yes/No in native
            MultilineText
        }

        public class ParamDefinition
        {
            public Guid UID { get; set; } 
            public string Name { get; set; } 
            
            public string DataTypeRaw { get; set;}
            public ParamDataTypeVariant? GetDataType()
            {
                if (Enum.TryParse(DataTypeRaw, out ParamDataTypeVariant data)) return data;
                return null;
            }

            public CSoftParameterTypeVariant GetCSoftPropType()
            {
                CSoftParameterTypeVariant paramType = CSoftParameterTypeVariant.String;
                ParamDataTypeVariant? revitParamType = GetDataType();
                if (revitParamType != null)
                {
                    switch (revitParamType)
                    {
                        case ParamDataTypeVariant.Integer:
                        case ParamDataTypeVariant.Number:
                        case ParamDataTypeVariant.Boolean:
                            paramType = CSoftParameterTypeVariant.Integer;
                            break;
                        case ParamDataTypeVariant.Angle:
                        case ParamDataTypeVariant.Area:
                        case ParamDataTypeVariant.CostPerArea:
                        case ParamDataTypeVariant.Distance:
                        case ParamDataTypeVariant.Length:
                        case ParamDataTypeVariant.MassDensity:
                        case ParamDataTypeVariant.RotationAngle:
                        case ParamDataTypeVariant.Slope:
                        case ParamDataTypeVariant.Speed:
                        case ParamDataTypeVariant.Volume:
                        case ParamDataTypeVariant.Currency:
                            paramType = CSoftParameterTypeVariant.Double;
                            break;
                        case ParamDataTypeVariant.Text:
                        case ParamDataTypeVariant.Material:
                        case ParamDataTypeVariant.FilePattern:
                        case ParamDataTypeVariant.MultilineText:
                            paramType = CSoftParameterTypeVariant.String;
                            break;
                        case ParamDataTypeVariant.URL:
                            paramType = CSoftParameterTypeVariant.URL;
                            break;
                    }
                }
                return paramType;
            }

            public void SetDataTypeFromCSoftParamType(CSoftParameterTypeVariant paramType)
            {
                ParamDataTypeVariant revitType = ParamDataTypeVariant.Text;
                switch (paramType)
                {
                    case CSoftParameterTypeVariant.String:
                    case CSoftParameterTypeVariant.TextField:
                        revitType = ParamDataTypeVariant.Text;
                        break;
                    case CSoftParameterTypeVariant.Integer:
                        revitType = ParamDataTypeVariant.Integer;
                        break;
                    case CSoftParameterTypeVariant.Double:
                        revitType = ParamDataTypeVariant.Text;
                        break;
                    case CSoftParameterTypeVariant.DateTime:
                        revitType = ParamDataTypeVariant.Time;
                        break;
                    case CSoftParameterTypeVariant.ListWithType:
                    case CSoftParameterTypeVariant.List:
                    case CSoftParameterTypeVariant.DynamicList:
                        revitType  = ParamDataTypeVariant.Text;
                        break;
                    case CSoftParameterTypeVariant.Calculated:
                        revitType = ParamDataTypeVariant.Text;
                        break;
                    case CSoftParameterTypeVariant.URL:
                        revitType = ParamDataTypeVariant.URL;
                        break;
                    case CSoftParameterTypeVariant.RGBColor:
                        revitType = ParamDataTypeVariant.URL;
                        break;
                }
                DataTypeRaw = revitType.ToString();
            }


            public string? DataCategory { get; set; }

            public int Group { get; set; }
            public bool Visible { get; set; }
            public string? Description { get; set; }
            public bool UserModifiable { get; set; }
            public bool HideWhenNoValue { get; set; }

            public ParamDefinition()
            {
                UID = Guid.NewGuid();
                Name = "";

                DataTypeRaw = ParamDataTypeVariant.Text.ToString();
                Visible = true;
                Description = "";
                UserModifiable = true;
                HideWhenNoValue = false;
            }


            public override string ToString()
            {
                return $"PARAM\t" +
                    $"{UID.ToString("D")}\t" +
                    $"{Name}\t" +
                    $"{DataTypeRaw}\t" +
                    $"{DataCategory}\t" +
                    $"{Group}\t" +
                    $"{convertBoolToNum(Visible)}\t" +
                    $"{Description}\t" +
                    $"{convertBoolToNum(UserModifiable)}\t" +
                    $"{convertBoolToNum(HideWhenNoValue)}";
            }

        }

        public MetadataDefinition Metadata { get; set; }

        public List<GroupDefinition> Groups { get; set; }

        public List<ParamDefinition> Parameters { get; set; }

        public RevitSharedParametersFile()
        {
            Groups = new List<GroupDefinition>();
            Parameters = new List<ParamDefinition>();
            Metadata = new MetadataDefinition();
        }

        public static RevitSharedParametersFile? LoadFrom(string path)
        {
            if (!File.Exists(path)) return null;
            string[] spfRaw = File.ReadAllLines(path);

            RevitSharedParametersFile RevitSPF = new RevitSharedParametersFile();

            foreach (string str in  spfRaw)
            {
                if (str.StartsWith("#")) continue;
                if (!str.Contains("\t")) continue;

                string[] arr = str.Split('\t');
                if (str.StartsWith("META"))
                {
                    RevitSPF.Metadata.Version = new Version(int.Parse(arr[1]), int.Parse(arr[2]));
                }
                else if (str.StartsWith("GROUP"))
                {
                    GroupDefinition groupDef = new GroupDefinition()
                    {
                        Id = int.Parse(arr[1]),
                        Name = arr[2]
                    };
                    RevitSPF.Groups.Add(groupDef);  
                }
                else if (str.StartsWith("PARAM"))
                {
                    ParamDefinition paramDef = new ParamDefinition()
                    {
                        UID = Guid.Parse(arr[1]),
                        Name = arr[2],
                        DataTypeRaw = arr[3],
                        DataCategory = arr[4],
                        Group = int.Parse(arr[5]),
                        Visible = getBoolFromString(arr[6]),
                        Description = arr[7],
                        UserModifiable = getBoolFromString(arr[8]),
                        HideWhenNoValue = getBoolFromString(arr[9])
                    };
                    RevitSPF.Parameters.Add(paramDef);  
                }
            }


            return RevitSPF;
        }

        private static bool getBoolFromString(string str)
        {
            if (str == "1") return true;
            return false;
        }

        private static int convertBoolToNum(bool value)
        {
            if (value == true) return 1;
            return 0;
        }


        public void Save(string path)
        {
            StringBuilder spf = new StringBuilder();

            spf.AppendLine("# This is a Revit shared parameter file. ");
            spf.AppendLine("# Was created programmatically by Nervana-app (https://github.com/GeorgGrebenyuk/Nervana-nbims-plugin)");

            spf.AppendLine("*META\tVERSION\tMINVERSION");
            spf.AppendLine($"META\t{this.Metadata.Version.Major}\tMINVERSION{this.Metadata.Version.Minor}");

            spf.AppendLine("*GROUP\tID\tNAME");
            foreach (var groupDef in this.Groups)
            {
                spf.AppendLine(groupDef.ToString());
            }

            spf.AppendLine("*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE");
            foreach (var paramDef in this.Parameters)
            {
                spf.AppendLine(paramDef.ToString());
            }

            File.WriteAllText(path, spf.ToString());
        }


    }


}
