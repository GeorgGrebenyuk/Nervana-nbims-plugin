using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;
using Teigha.DatabaseServices;
using System.Windows;

using HostMgd.ApplicationServices;
using HostMgd.EditorInput;

using NervanaCommonMgd.Configs;

using NC_APP = HostMgd.ApplicationServices.Application ;

namespace NervanaNcMgd.Functions
{
    /// <summary>
    /// Use Sustem.Reflection to look into object
    /// </summary>
    internal class MgdExplorerReflection
    {
        private const string p_Type_COM = "__ComObject";
        private const string p_Type_ImpEntity = "ImpEntity";
        private MgdExplorerReflectionConfig? mConfig;
        //
        private MgdExplorerReflection()
        {
            mConfig = (MgdExplorerReflectionConfig?)IConfigBase.LoadFrom<MgdExplorerReflectionConfig>(null);
            if (mConfig == null) mConfig = new MgdExplorerReflectionConfig();
            p_Classes = new List<Type>();

            InitTypes();
        }

        public static MgdExplorerReflection CreateInstance()
        {
            if (mInstance == null) mInstance = new MgdExplorerReflection();
            return mInstance;
        }



        private void InitTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> need_assemblies = new List<Assembly>();
            foreach (var ass in assemblies)
            {
                //Using try\catch because in Linq (assemblies.Where(a => a.CodeBase != null && Loader.Config.Libs.Contains(a.CodeBase))) there will be an error during some libraries "The invoked member is not supported in a dynamic assembly."
                bool can_add = false;
                try
                {
                    string ass_location = ass.Location;
                    string ass_name = Path.GetFileName(ass_location);
                    can_add = mConfig?.MgdLibraries?.Contains(Path.GetFileName(ass_name)) ?? false;
                }
                catch
                { 
                    
                }
                if (can_add) need_assemblies.Add(ass);
            }

            
            foreach (var a in need_assemblies)
            {
                var classes = a.GetTypes().Where(t => (t.IsClass | t.IsValueType) && t.FullName != null && !t.FullName.Contains("::"));
                p_Classes = p_Classes.Concat(classes);
            }

            //TODO: add struct and other types?
        }

        #region Частные случаи
        private MgdExplorerReflection_ExplorerStructure Process_Application()
        {
            MgdExplorerReflection_ExplorerStructure explStructure = new MgdExplorerReflection_ExplorerStructure();
            p_elemDataCounter = 0;

            ETreeItem item = new ETreeItem("Application", p_elemDataCounter);
            p_elemDataCounter++;

            EParametersGroup app_props = new EParametersGroup("Application properties");
            try
            {
                app_props.Parameters.Add(new EParameter("IsInBackgroundMode")
                { Value = NC_APP.IsInBackgroundMode });
                app_props.Parameters.Add(new EParameter("NonInPlaceMainWindow")
                { Value = NC_APP.NonInPlaceMainWindow, PType = EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("MenuGroups")
                { Value = NC_APP.MenuGroups, PType = EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("MenuBar")
                { Value = NC_APP.MenuBar, PType = EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("LongTransactionManager")
                { Value = NC_APP.LongTransactionManager, PType = EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("IsInPlaceServer")
                { Value = NC_APP.IsInPlaceServer });
                app_props.Parameters.Add(new EParameter("IsInCustomizationMode")
                { Value = NC_APP.IsInCustomizationMode });
                app_props.Parameters.Add(new EParameter("InfoCenter")
                { Value = NC_APP.InfoCenter, PType = EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("DisplayTextScreen")
                { Value = NC_APP.DisplayTextScreen });
                app_props.Parameters.Add(new EParameter("AcadApplication")
                { Value = NC_APP.AcadApplication, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("IsQuiescent")
                { Value = NC_APP.IsQuiescent });
                app_props.Parameters.Add(new EParameter("MainWindow")
                { Value = NC_APP.MainWindow, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("DocumentWindowCollection")
                { Value = NC_APP.DocumentWindowCollection, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("DocumentManager")
                { Value = NC_APP.DocumentManager, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("HostApplication")
                { Value = NC_APP.HostApplication, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("UserConfigurationManager")
                { Value = NC_APP.UserConfigurationManager, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("Version")
                { Value = NC_APP.Version, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("Publisher")
                { Value = NC_APP.Publisher, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("RecentDocuments")
                { Value = NC_APP.RecentDocuments, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("Preferences")
                { Value = NC_APP.Preferences, PType =EParameter_Type.CanExplore });
                app_props.Parameters.Add(new EParameter("StatusBar")
                { Value = NC_APP.StatusBar, PType =EParameter_Type.CanExplore });
            }

            catch { }

            explStructure.TreeStructure.Add(item);
            explStructure.PropertiesStructure.Add(new EParametersGroup[] { app_props });

            return explStructure;
        }
        #endregion

        private MgdExplorerReflection_ExplorerStructure? Process_ObjectIdCollection(ObjectIdCollection collection)
        {
            if (collection == null)
            {
                //получаем объекты выделенные в модели
                Editor ed = NDocument.Editor;

                SelectionSet SelSet = ed.SelectImplied().Value;
                if (SelSet.Count > 0)
                {
                    collection = new ObjectIdCollection(SelSet.GetObjectIds());
                }
                else collection = new ObjectIdCollection();
            }

            MgdExplorerReflection_ExplorerStructure explStructure = new MgdExplorerReflection_ExplorerStructure();

            p_elemDataCounter = 0;
            using (Transaction tr = NDocument.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId OneObjectId in collection)
                {
                    if (OneObjectId == ObjectId.Null) continue;
                    DBObject selObj = OneObjectId.GetObject(OpenMode.ForRead);
                    Type selObjType = selObj.GetType();
                    //Облака точек, проверка
                    if (selObjType.Name == p_Type_ImpEntity)
                    {
                        //var selObj_as_PCI = (HostMgd.PointClouds.PointCloudInsertion)selObj;
                        //if (selObj_as_PCI != null) selObj = selObj_as_PCI;
                    }

                    List<EParametersGroup> props = GetProperties(selObj);

                    //RXClass
                    EParametersGroup obj_RXClass = new EParametersGroup("RXClass");
                    obj_RXClass.Parameters.Add(new EParameter("AppName", selObj.GetRXClass().AppName));
                    obj_RXClass.Parameters.Add(new EParameter("AutoDelete", selObj.GetRXClass().AutoDelete));
                    obj_RXClass.Parameters.Add(new EParameter("ClassVersion", selObj.GetRXClass().ClassVersion));
                    obj_RXClass.Parameters.Add(new EParameter("DxfName", selObj.GetRXClass().DxfName));
                    obj_RXClass.Parameters.Add(new EParameter("IsDisposed", selObj.GetRXClass().IsDisposed));
                    obj_RXClass.Parameters.Add(new EParameter("Name", selObj.GetRXClass().Name));
                    obj_RXClass.Parameters.Add(new EParameter("ProxyFlags", selObj.GetRXClass().ProxyFlags));

                    props.Add(obj_RXClass);
                    if (!props.Where(p => p.GroupName == "RXClass").Any()) props = props.Concat(new EParametersGroup[] { obj_RXClass }).ToList();


                    ETreeItem item = new ETreeItem(selObj.GetType().Name + " " + selObj.Handle.ToString(), p_elemDataCounter);
                    explStructure.TreeStructure.Add(item);
                    explStructure.PropertiesStructure.Add(props.ToArray());
                    p_elemDataCounter++;

                }
            }
            return explStructure;
        }

        public MgdExplorerReflection_ExplorerStructure? Process_Object(object? obj)
        {
            if (obj == null) return null;

            MgdExplorerReflection_ExplorerStructure explStructure = new MgdExplorerReflection_ExplorerStructure();
            if (obj == null) return explStructure;
            Type objType = obj.GetType();

            if (objType.IsEnum)
            {
                MgdMode? enumType = (MgdMode)Enum.ToObject(typeof(MgdMode), obj);
                if (enumType == MgdMode.Application) 
                {
                    return Process_Application();
                }
                else
                {
                    //Резервируем для иных вариантов
                }
            }

            if (objType.Name.Contains(p_Type_COM) | (objType.BaseType != null && objType.BaseType.Name.Contains(p_Type_COM))) return Process_COM(obj);
            else if (objType == typeof(Teigha.DatabaseServices.ObjectIdCollection)) return Process_ObjectIdCollection((ObjectIdCollection)obj);
            else if (objType == typeof(ObjectId)) return Process_ObjectIdCollection(new ObjectIdCollection(new ObjectId[] { (ObjectId)obj }));
            //TODO: add for hostPointClouds ...

            p_elemDataCounter = 0;
            if (p_Classes.Contains(objType))
            {
                var props = GetProperties(obj);
                //var methods = objType.GetMethods();
                ETreeItem item = new ETreeItem(obj.GetType().Name, p_elemDataCounter);
                explStructure.PropertiesStructure.Add(props.ToArray());
                p_elemDataCounter++;

                if (obj is ICollection)
                {
                    foreach (var sub_obj in (IEnumerable)obj)
                    {
                        ETreeItem sub_item = new ETreeItem(sub_obj.GetType().Name, p_elemDataCounter);
                        var sub_props = GetProperties(sub_obj);
                        p_elemDataCounter++;

                        item.Items.Add(sub_item);
                        explStructure.PropertiesStructure.Add(sub_props.ToArray());
                    }
                }

                explStructure.TreeStructure.Add(item);
                
            }
            return explStructure;
        }


        private MgdExplorerReflection_ExplorerStructure Process_COM(object obj)
        {
            MgdExplorerReflection_ExplorerStructure explStructure = new MgdExplorerReflection_ExplorerStructure();
            if (obj == null) return explStructure;
#if NET_FR
            Type objType = obj.GetType();
            bool can_process1 = false;
            if (objType.Name.Contains(p_Type_COM)) can_process1 = true;
            if (objType.BaseType.Name.Contains(p_Type_COM)) can_process1 = true;
            if (!can_process1) return explStructure;
            p_elemDataCounter = 0;
            //ProcessTypes(objType);

            if (DispatchUtility.ImplementsIDispatch(obj))
            {
                Type dispatchType = DispatchUtility.GetType(obj, false);
                if (dispatchType != null)
                {
                    dynamic obj2 = obj;
                    ETreeItem item = new ETreeItem(dispatchType.Name, p_elemDataCounter);
                    var com_props = GetProperties_COM(dispatchType, obj2);

                    p_elemDataCounter++;
                    explStructure.TreeStructure.Add(item);
                    explStructure.PropertiesStructure.Add(new EParametersGroup[] { com_props });

                    bool is_IEnumerable = false;
                    try
                    {
                        is_IEnumerable = (IEnumerable)obj2 != null;
                    }
                    catch { }

                    if (is_IEnumerable)
                    {
                        foreach (var sub_obj in (IEnumerable)obj2)
                        {
                            Type sub_dispatchType = DispatchUtility.GetType(sub_obj, false);
                            ETreeItem sub_item = new ETreeItem(sub_dispatchType.Name, p_elemDataCounter);
                            var sub_com_props = GetProperties_COM(sub_dispatchType, sub_obj);
                            p_elemDataCounter++;

                            item.Items.Add(sub_item);
                            //explStructure.TreeStructure.Add(sub_item);
                            explStructure.PropertiesStructure.Add(new EParametersGroup[] { sub_com_props });
                        }
                    }
                }
            }
#endif
            return explStructure;
        }

        private List<EParametersGroup> GetProperties (object obj)
        {
            Type objType = obj.GetType();

            //Получаем неупорядоченную цепочку наследования типов
            List<Type> OwnerTypes = new List<Type>();
            foreach (Type classType in p_Classes)
            {
                //var classType_instance = Activator.CreateInstance(classType);
                if (objType.IsSubclassOf(classType) | objType.Equals(classType)) OwnerTypes.Add(classType);
                //else if (classType.IsSubclassOf(objType) && (classType)obj != null) OwnerTypes.Add(classType);
            }
            //Так не учитываются классы, наследники от DBObject, например Circle, то есть нужно ввести пряму. проверку всех типов

            //Теперь нужно установить последовательность наследования
            OwnerTypes.Sort((t1, t2) => t1.IsSubclassOf(t2).CompareTo(t2.IsSubclassOf(t1)));

            //Проходимся по каждому типу и собираем во временный список названия свойств (чтобы явно установить наследованные свойства)
            List<string[]> prop_names = new List<string[]>();

            string[] previous_names = new string[] { };
            foreach (Type classType in OwnerTypes)
            {
                var typeProps = classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

                if (typeProps.Length > 0)
                {
                    string[] names = typeProps.Select(t => t.Name).ToArray();
                    if (!previous_names.Any()) previous_names = names;
                    else
                    {
                        names = names.Except(previous_names).ToArray();
                    }
                    prop_names.Add(names);
                }
                else prop_names.Add(new string[] { });
            }

            //Теперь начисто отбираем только нужные свойста
            List<EParametersGroup> props = new List<EParametersGroup>();
            for (int type_counter = 0; type_counter < OwnerTypes.Count; type_counter++)
            {
                Type classType = OwnerTypes[type_counter];
                string[] classType_Props = prop_names[type_counter];
                var typeProps = classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

                if (classType_Props.Length > 0)
                {
                    EParametersGroup propType = new EParametersGroup(classType.Name);
                    foreach (var prop in classType_Props)
                    {
                        PropertyInfo? prop_info = typeProps.Where(p => p.Name == prop).FirstOrDefault();
                        EParameter prop_Def = new EParameter(prop, null);
                        object? value = null;

                        
                        try
                        {
                            if (prop_info != null) value = prop_info.GetValue(obj, new object[] { });
                        }
                        catch (System.Exception ex)
                        {
                            Type exType = ex.GetType();
                            string exStr = ex.ToString();
                            if (exStr.Contains("NotImplementedException"))
                            {
                                prop_Def.VType =EValue_Type.NotImplemented;
                                value = "NotImplementedException";
                            }
                            else if (exStr.Contains("eNotApplicable")) 
                            {
                                prop_Def.VType =EValue_Type.NotApplicable;
                                value = "eNotApplicable";
                            }
                            else value = ex;
                        }

                        if (value != null)
                        {
                            prop_Def.PType = GetParameterType(value.GetType());

                        }
                        prop_Def.Value = value;
                        propType.Parameters.Add(prop_Def);
                    }
                    props.Add(propType);
                }
            }

            return props;
        }

        private EParametersGroup GetProperties_COM(Type dispatchType, object obj)
        {
            MemberInfo[] members = dispatchType.GetMembers();
            EParametersGroup group = new EParametersGroup("COM данные");
           

            foreach (MemberInfo member in members)
            {
                EParameter memberDef = new EParameter("");

                bool can_process = false;
                object? member_data = null;

                if (member.Name.StartsWith("get_"))
                {
                    string memberName = member.Name.Replace("get_", "");
                    memberDef.Caption = memberName;
                    can_process = true;
                }


                if (can_process)
                {
                    try
                    {
                        MethodInfo? method = dispatchType.GetMethod(member.Name);
                        member_data = method?.Invoke(obj, new object[] { }) ?? null;

                    }
                    catch (System.Exception ex)
                    {
                        member_data = ex.Message;
                    }
                    
                    if (member_data != null)
                    {
                        Type member_data_Type = member_data.GetType();
                        if ((member_data_Type.BaseType != null && member_data_Type.BaseType.Name.Contains(p_Type_COM)) | member_data_Type.Name.Contains(p_Type_COM)) memberDef.PType =EParameter_Type.CanExplore;
                        else
                        {
                            //member_data = ConvertTypes(member_data);
                        }
                    }
                    

                    memberDef.Value = member_data;
                    group.Parameters.Add(memberDef);
                }
            }
            return group;
        }

        private EParameter_Type GetParameterType(Type vType)
        {
            if (p_Classes.Contains(vType)) return EParameter_Type.CanExplore;
            else if (vType.Name.Contains(p_Type_COM)) return EParameter_Type.CanExplore;
            else if (vType == typeof(ObjectId) | vType == typeof(ObjectIdCollection)) return EParameter_Type.CanExplore;
            else
            {
                //value = ConvertTypes(value);
            }

            return EParameter_Type.Common;
        }
        


        private IEnumerable<Type> p_Classes;

        private int p_elemDataCounter = 0;
        private Document NDocument => HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

        private static MgdExplorerReflection? mInstance = null;
    }
}
