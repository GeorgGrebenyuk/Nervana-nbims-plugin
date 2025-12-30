using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Teigha.DatabaseServices;
using Teigha.Runtime;
using Teigha.Geometry;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using HostMgd.Windows;
using BIMStructureMgd.ObjectProperties;

namespace NervanaNcBIMsMgd.UI.Controls
{
    public class ParameterContainer : INotifyPropertyChanged
    {
        public const string NO_CATEGORY = "Общие";
        private int paramId { get; set; }
        private string _name { get; set; }
        private string _caption { get; set; }
        private string _value { get; set; }

        internal string Category { get; set; } = NO_CATEGORY;

        private bool isCategory { get; set; } = false;
        private bool isReadOnly { get; set; } = false;
        private bool isCalculated { get; set; } = false;


        //public ParameterContainer(Parameter parameter, string? category)
        //{
        //    this._name = parameter.Name;
        //    this._value = parameter.Value;
        //    this._caption = string.IsNullOrEmpty(parameter.Comment) ? "" : parameter.Comment;
        //    this.Category = category ?? ParameterContainer.NO_CATEGORY;
        //    this.isCalculated = parameter.Calculated;
        //}

        //public ParameterContainer() { }


        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Caption
        {
            get => _caption;
            set { _caption = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); }
        }

        public bool IsCategory
        {
            get => isCategory;
            set { isCategory = value; OnPropertyChanged(); }
        }

        public bool IsReadOnly
        {
            get => isReadOnly;
            set { isReadOnly = value; OnPropertyChanged(); }
        }

        public bool IsCalculated
        {
            get => isCalculated;
            set { isCalculated = value; OnPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ObservableCollection<ParameterContainer> GetParametersFromElementData(ElementData elemData)
        {
            var parameters = elemData.Parameters.Select(p => (Parameter: p, Def: ParameterDefCollection.Global.GetParameterDef(p))).ToList();
            var categories = parameters.SelectMany(p => p.Def?.Categories ?? Array.Empty<ParameterCategory>())
                                       .GroupBy(c => c.Name)
                                       .Select(gr => gr.First())
                                       .OrderBy(c => c.CategoryOrder)
                                       .ToList();

            
            //var _dynamicProperties = parameters.Select(p => p.Parameter).ToDictionary(p => p.Name);
            var _orderedParameters = new List<(Parameter, ParameterDef)>();
            Dictionary<string, List<(Parameter, ParameterDef)>> cat2params = new Dictionary<string, List<(Parameter, ParameterDef)>>();
            foreach (var (categoryName, index) in categories.Select((c, index) => (c.Name, index)))
            {

                cat2params.Add(categoryName, parameters.Where(p => p.Def?.HasCategory(categoryName) ?? false)
                                                           .OrderBy(p => p.Def.GetCategory(categoryName).ParameterOrder)
                                                           .ToList());
            }

            var noCategoryParameters = parameters.Where(p => !(p.Def?.Categories.Any() ?? false)).ToList();
            cat2params.Add(ParameterContainer.NO_CATEGORY, noCategoryParameters.ToList());


            ObservableCollection<ParameterContainer> result = new ObservableCollection<ParameterContainer>();
            foreach (var orderParamSet in cat2params)
            {
                result.Add(new ParameterContainer() { Caption = orderParamSet.Key, Category = orderParamSet.Key, IsCategory = true });

                foreach (var paramDefExtended in orderParamSet.Value)
                {
                    ParameterContainer pCont = new ParameterContainer()
                    {
                        Caption = string.IsNullOrEmpty(paramDefExtended.Item1.Comment) ? null : "",
                        Name = paramDefExtended.Item1.Name,
                        Category = orderParamSet.Key,
                        Value = paramDefExtended.Item1.Value,
                        IsCalculated = paramDefExtended.Item1.Calculated,
                        IsCategory = false,
                        IsReadOnly = paramDefExtended.Item2.IsParameterReadOnly()
                    };
                    result.Add(pCont);
                }  
            }
            return result;
        }
    }

    public class ViewModel
    {
        public ObservableCollection<ParameterContainer> Parameters { get; set; }
        
    }



    /// <summary>
    /// Interaction logic for Nervana_ParametersEditor.xaml
    /// </summary>
    public partial class Nervana_ParametersEditor : UserControl
    {
        private ObjectId idViewObject;
        public Nervana_ParametersEditor()
        {
            InitializeComponent();

            DocumentCollection dm = HostMgd.ApplicationServices.Application.DocumentManager;
            dm.MdiActiveDocument.ImpliedSelectionChanged += new EventHandler(callback_SelectionChanged);
            dm.DocumentCreated += new DocumentCollectionEventHandler(callback_DocumentCreated);
            dm.DocumentToBeDestroyed += new DocumentCollectionEventHandler(callback_DocumentToBeDestroyed);

            foreach (Document doc in dm)
            {
                doc.Database.ObjectErased += new ObjectErasedEventHandler(callback_ObjectErased);
                doc.Database.ObjectModified += new ObjectEventHandler(callback_ObjectModified);
            }

            Parameters = new ObservableCollection<ParameterContainer>();
            this.DataGrid_ParametersInfo.ItemsSource = Parameters;
        }

        private void callback_DocumentCreated(object sender, DocumentCollectionEventArgs e)
        {
            e.Document.Database.ObjectErased += new ObjectErasedEventHandler(callback_ObjectErased);
            e.Document.Database.ObjectModified += new ObjectEventHandler(callback_ObjectModified);
        }

        private void callback_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            e.Document.Database.ObjectErased -= new ObjectErasedEventHandler(callback_ObjectErased);
            e.Document.Database.ObjectModified -= new ObjectEventHandler(callback_ObjectModified);
        }

        private void callback_ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            if (e.DBObject.Id != idViewObject)
                return;


            // если объект был стерт
            if (e.Erased)
                clearObjectData("Объект не выбран.");
        }
        private void callback_ObjectModified(object sender, ObjectEventArgs e)
        {
            if (e.DBObject.Id != idViewObject)
                return;

            setObjectToView(e.DBObject);
        }

        private void clearObjectData(String msgNode)
        {
            this.TreeView_ElementHierarchy.Items.Clear();
            this.TreeView_ElementHierarchy.Items.Add(msgNode);

            //this.DataGrid_ParametersInfo.SelectedItem = null;
            this.Parameters = new ObservableCollection<ParameterContainer>();

            idViewObject = ObjectId.Null;
        }

        private void callback_SelectionChanged(object? sender, EventArgs e)
        {
            // Type evType = e.GetType();

            //             DocumentCollection dm = Platform.ApplicationServices.Application.DocumentManager;
            //             SelectionSet SelSet = dm.MdiActiveDocument.Editor.SelectImplied().Value;
            Document acDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = acDoc.Editor;

            SelectionSet SelSet = ed.SelectImplied().Value;
            if (SelSet.Count == 0)
            {
                clearObjectData("Объект не выбран.");
                return;
            }

            if (SelSet.Count > 1)
            {
                clearObjectData("Выбрано несколько объектов.");
                return;
            }

            Database db = acDoc.Database;

            // Заполним список 
            ObjectId idObj = SelSet.GetObjectIds().ElementAt(0);

            using Transaction tr = db.TransactionManager.StartTransaction();

            DBObject selObj = idObj.GetObject(OpenMode.ForRead);
            //DBObject selObj = idObj.Open(OpenMode.ForRead);
            var dbParametricObject = selObj as BIMStructureMgd.ObjectProperties.IParametricObject;
            if (dbParametricObject == null)
            {
                clearObjectData("Неподдерживаемый тип объекта.");
                return;
            }

            setObjectToView(selObj);

            tr.Commit();
        }

        private void setObjectToView(DBObject cObj)
        {
            IParametricObject? dbPrmEnt = cObj as IParametricObject;
            ElementData? elmData = dbPrmEnt?.GetElementData();

            if (elmData == null)
            {
                clearObjectData("Объект не выбран.");
                return;
            }

            setRootElement(elmData);

            setParametersFrom(elmData);

            idViewObject = cObj.Id;
        }

        private void setRootElement(ElementData elmData)
        {
            this.TreeView_ElementHierarchy.Items.Clear();
            TreeViewItem nodeRoot = new TreeViewItem() { Header = elmData.Name, Tag = elmData.Id.ToString() };

            setChildElementsFrom(elmData, nodeRoot);
            this.TreeView_ElementHierarchy.Items.Add(nodeRoot);
            nodeRoot.IsExpanded = true;
        }

        private void setChildElementsFrom(ElementData elmData, TreeViewItem elmNode)
        {
            foreach (var chldElm in elmData.Children.Where(c => c != null))
            {
                TreeViewItem chldNode = new TreeViewItem() { Header = chldElm.Name, Tag = chldElm.Id.ToString() };
                chldNode.Tag = chldElm.Id.ToString();
                setChildElementsFrom(chldElm, chldNode);
                elmNode.Items.Add(chldNode);
            }
        }

        private void setParametersFrom(ElementData elmData)
        {
            //this.DataGrid_ParametersInfo.SelectedItem = null;
            this.Parameters = new ObservableCollection<ParameterContainer>();
            this.Parameters = ParameterContainer.GetParametersFromElementData(elmData);
        }

        private void TreeView_AfterSelect(object sender, RoutedEventArgs e)
        {
            if (idViewObject.IsNull)
                return;

            
            string? stag = ((TreeViewItem?)this.TreeView_ElementHierarchy.SelectedItem)?.Tag as string;
            int idElm = string.IsNullOrEmpty(stag) ? int.MinValue : Int32.Parse(stag);

            Document acDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = acDoc.Database;

            using Transaction tr = db.TransactionManager.StartTransaction();

            DBObject cObj = idViewObject.GetObject(OpenMode.ForRead);
            IParametricObject? dbPrmEnt = cObj as IParametricObject;

            if (dbPrmEnt == null)
                return;

            ElementData? elmData = dbPrmEnt?.GetElementData();
            ElementData chldElm = new ElementData();
            if (!elmData.GetElementById(idElm, ref chldElm))
            {
                System.Windows.MessageBox.Show("Incorrect data structure!", "Error message");
                return;
            }
            setParametersFrom(chldElm);

            tr.Commit();
        }

        private ObservableCollection<ParameterContainer> Parameters { get; set; }
    }
}
