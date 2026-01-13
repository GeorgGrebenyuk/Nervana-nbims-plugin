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

        public void Import(bool useWay2)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выбор файла Revit ФОП";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Revit ФОП (*.txt, *.TXT) | *.txt;*.TXT";

            string revitSFPpath = "";
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    revitSFPpath = openFileDialog.FileName;
            //}
            //revitSFPpath = @"E:\DataTest\Разное\Alla\ГП_Площадка ЗИФ.ifc.sharedparameters.txt";

#if DEBUG
            
#else

#endif

            if (!File.Exists(revitSFPpath)) return;

            RevitSharedParametersFile? revitSFP = RevitSharedParametersFile.LoadFrom(revitSFPpath);
            if (revitSFP == null)
            {
                MessageBox.Show("Файл Revit ФОП не был успешно обработан!. Функция будет прервана");
                return;
            }

            string revitSFP2 = Path.GetTempFileName();
            //Из-за бага в API нельзя применить "CADLibData.CADLIB_Library.CreateParamDef(cadlibParamDef);"
            // Поэтму придется создавать определение CSoftParametersFile и подсовывать его команде Library.ImportParameters()

            var existedCategories = CADLibData.CADLIB_Library.GetCategoriesList();
            Dictionary<int, int> revitGroup2CADLibCats = new Dictionary<int, int>();

            CSoftParametersFile csParamsFile = new CSoftParametersFile();

            foreach (RevitSharedParametersFile.ParamDefinition revitparamDef in revitSFP.Parameters)
            {
                var revitGroups = revitSFP.Groups.Where(group => group.Id == revitparamDef.Group);

                if (!useWay2)
                {
                    ParamDef cadlibParamDef = new ParamDef();
                    cadlibParamDef.name = revitparamDef.Name;
                    cadlibParamDef.caption = revitparamDef.Name;
                    cadlibParamDef.type = (int)revitparamDef.GetCSoftPropType();
                    cadlibParamDef.valueType = (int)revitparamDef.GetCSoftPropType();
                    cadlibParamDef.Categories = new List<ParamDefCategory>();

                    if (revitGroups.Any())
                    {
                        cadlibParamDef.Categories.Add(new ParamDefCategory() { name = revitGroups.First().Name });
                    }
                    cadlibParamDef.isReadonly = !revitparamDef.UserModifiable;

                    CLibParamDefInfo cadlibParamDef2 = new CLibParamDefInfo(cadlibParamDef);
                    CADLibData.CADLIB_Library.CreateParamDef(cadlibParamDef2);
                }
                else
                {
                    CSoftParametersFile.ParameterDefinition csParamDef = CSoftParametersFile.ParameterDefinition.CreateDefault();
                    csParamDef.Name = revitparamDef.Name;
                    csParamDef.Caption = revitparamDef.Name;
                    csParamDef.ParamType = revitparamDef.GetCSoftPropType();
                    csParamDef.ValueType = csParamDef.ParamType;
                    csParamDef.IsReadOnly = !revitparamDef.UserModifiable;

                    if (revitGroups.Any())
                    {
                        RevitSharedParametersFile.GroupDefinition revitParamGroup = revitGroups.First();
                        if (!revitGroup2CADLibCats.ContainsKey(revitParamGroup.Id))
                        {
                            var coNamesCats = existedCategories.Where(cat => cat.Value.mSysName == revitparamDef.Name);
                            if (!coNamesCats.Any())
                            {
                                int catId = CADLibData.CADLIB_Library.CreateCategory(revitparamDef.Name, revitparamDef.Name);
                                revitGroup2CADLibCats[revitParamGroup.Id] = catId;
                                //existedCategories.Add(revitparamDef.Name, CADLibData.CADLIB_Library.GetCategoryInfo(catId));
                            }
                            revitGroup2CADLibCats[revitParamGroup.Id] = coNamesCats.First().Value.idCategory;
                        }

                        //CLibCategoryInfo catInfo = existedCategories[revitparamDef.Name];
                        //var catInfo2 = CADLibData.CADLIB_Library.GetCategoryInfo(catInfo.idCategory);

                        CSoftParametersFile.CategoryDefinition csCatDef = new CSoftParametersFile.CategoryDefinition();
                        csCatDef.CategoryName = revitparamDef.Name;
                        csParamDef.Categories.CategoriesList.Add(csCatDef);
                    }
                    csParamsFile.Parameters.Add(csParamDef);
                }                
            }

            if (useWay2)
            {
                csParamsFile.Save(revitSFP2);
                CADLibData.CADLIB_Library.ImportParameters(revitSFP2, false);
            }   
        }

        public void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Сохранение файла конфигурации";
            saveFileDialog.Filter = "Конфиграционный файл (*.XML, *.xml) | *.XML;*.xml";
            saveFileDialog.AddExtension = true;

            string revitSFPpath = "";
            //if (saveFileDialog.ShowDialog() == true)
            //{
            //    revitSFPpath = saveFileDialog.FileName;
            //}

            revitSFPpath = @"E:\Temp\0001.txt";

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
