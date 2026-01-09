using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

using CADLibKernel;

using NervanaCommonMgd;
using NervanaCommonMgd.Common;

namespace NervanaCADLibLibraryMgd.Functions.Parameters
{
    /// <summary>
    /// Импорт-экспорт ФОП Revit
    /// </summary>
    internal class RevitSharedParamsIO
    {
        private RevitSharedParamsIO() { }

        public static RevitSharedParamsIO CreateInstance()
        {
            if (mInstance == null) mInstance = new RevitSharedParamsIO();
            return mInstance;
        }

        public void Import()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор файла Revit ФОП";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Revit ФОП (*.txt, *.TXT) | *.txt;*.TXT";

            string revitSFPpath = "";
            if (openFileDialog.ShowDialog() == true)
            {
                revitSFPpath = openFileDialog.FileName;
            }

            if (!File.Exists(revitSFPpath)) return;

            RevitSharedParametersFile? revitSFP = RevitSharedParametersFile.LoadFrom(revitSFPpath);
            if (revitSFP == null)
            {
                MessageBox.Show("Файл Revit ФОП не был успешно обработан!. Функция будет прервана");
                return;
            }

            foreach (RevitSharedParametersFile.ParamDefinition revitparamDef in revitSFP.Parameters)
            {
                ParamDef cadlibParamDef = new ParamDef();
                cadlibParamDef.name = revitparamDef.Name;
                cadlibParamDef.caption = revitparamDef.Name;
                cadlibParamDef.type = (int)revitparamDef.GetCSoftPropType();
                cadlibParamDef.valueType = (int)revitparamDef.GetCSoftPropType();
                cadlibParamDef.Categories = new List<ParamDefCategory>();

                var revitGroups = revitSFP.Groups.Where(group => group.Id == revitparamDef.Group);
                if (revitGroups.Any()) cadlibParamDef.Categories.Add(new ParamDefCategory() { name = revitGroups.First().Name });

                cadlibParamDef.isReadonly = !revitparamDef.UserModifiable;

                CLibParamDefInfo cadlibParamDef2 = new CLibParamDefInfo(cadlibParamDef);
                CADLibData.CADLIB_Library.CreateParamDef(cadlibParamDef2);
            }
        }

        public void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранение файла конфигурации";
            saveFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";
            saveFileDialog.AddExtension = true;

            string revitSFPpath = "";
            if (saveFileDialog.ShowDialog() == true)
            {
                revitSFPpath = saveFileDialog.FileName;
            }

            RevitSharedParametersFile revitSFP = new RevitSharedParametersFile();

            var paramsAll = CADLibData.CADLIB_Library.GetParamDefs();
            Dictionary<int, int> cadlibCategory2RevitGroup = new Dictionary<int, int>();
            foreach (CLibParamDefInfo paramDef in paramsAll )
            {
                RevitSharedParametersFile.ParamDefinition revitparam = new RevitSharedParametersFile.ParamDefinition();
                revitparam.Name = paramDef.mstrName;
                revitparam.Description = paramDef.mstrCaption;

                CSoftParameterTypeVariant? valueType = (CSoftParameterTypeVariant)paramDef.mnValueType;
                if (valueType == null) valueType = CSoftParameterTypeVariant.String;
                revitparam.SetDataTypeFromCSoftParamType(valueType.Value);

                //TODO: файл мэппинга категорий объектов CADLib и Revit
                //revitparam.DataCategory = "...";

                revitparam.Visible = !paramDef.mbCatHidden;
                revitparam.UserModifiable = !paramDef.mbReadonly;

                if (paramDef.mCategories != null)
                {
                    foreach (CLibParamCategoryInfo category in paramDef.mCategories)
                    {
                        if (!cadlibCategory2RevitGroup.ContainsKey(category.idParamCategory))
                        {
                            RevitSharedParametersFile.GroupDefinition revitGroupDef = new RevitSharedParametersFile.GroupDefinition();
                            revitGroupDef.Id = revitSFP.Groups.Count + 1;
                            revitGroupDef.Name = category.mstrName;
                            revitSFP.Groups.Add(revitGroupDef);

                            cadlibCategory2RevitGroup[category.idParamCategory] = revitGroupDef.Id;

                        }
                        revitparam.Group = cadlibCategory2RevitGroup[category.idParamCategory];
                    }
                }

                revitSFP.Parameters.Add(revitparam);
            }


            revitSFP.Save(revitSFPpath);
        }

        private static RevitSharedParamsIO? mInstance;
    }
}
