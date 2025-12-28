using System;
using System.Collections.Generic;
using System.Linq;
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

using HostMgd.ApplicationServices;

using Platform = HostMgd;
using NativePlatform = Teigha;
using HostMgd.EditorInput;
using Teigha.DatabaseServices;

using mdsUnitsLib;

namespace NervanaNcComMgd.UI.Controls
{
    class mdsElemDef
    {
        public string Name { get; set; }
        public List<mdsElemParameterDef> Parameters { get; set; }
        public List<mdsElemDef> Childs { get; set; }

        public mdsElemDef()
        {
            Childs = new List<mdsElemDef>();
            Parameters = new List<mdsElemParameterDef>();
        }
    }
    class mdsElemParameterDef
    {
        public string Name { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }

        public parameter NativeData { get; set; }
    }
    /// <summary>
    /// Interaction logic for Nervana_ParametersExplorerSpace.xaml
    /// </summary>
    public partial class Nervana_ParametersExplorerSpace : UserControl
    {
        private ObjectIdCollection mLastEntity;
        private mdsElemDef?[]? mLastEntityWrap;
        public Nervana_ParametersExplorerSpace()
        {
            InitializeComponent();

            DocumentCollection dm = Platform.ApplicationServices.Application.DocumentManager;
            dm.MdiActiveDocument.ImpliedSelectionChanged -= new EventHandler(callback_SelectionChanged);
            dm.MdiActiveDocument.ImpliedSelectionChanged += new EventHandler(callback_SelectionChanged);

            dm.DocumentToBeDestroyed += new DocumentCollectionEventHandler(callback_DocumentToBeDestroyed);
            dm.DocumentBecameCurrent += new DocumentCollectionEventHandler(callback_DocumentBecameCurrent);

        }


        private void callback_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            mLastEntityWrap = null;
            setData();
        }

        private void callback_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            mLastEntityWrap = null;
            setData();
        }

        private void callback_SelectionChanged(object? sender, EventArgs e)
        {
            ObjectIdCollection? data = new ObjectIdCollection();
            Editor ed = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

            SelectionSet SelSet = ed.SelectImplied().Value;
            if (SelSet.Count > 0)
            {
                data = new Teigha.DatabaseServices.ObjectIdCollection(SelSet.GetObjectIds());
            }
            else
            {
                PromptSelectionResult res = ed.GetSelection();
                if (res.Status == PromptStatus.OK && res.Value.Count > 0)
                {
                    data = new Teigha.DatabaseServices.ObjectIdCollection(res.Value.GetObjectIds());
                }
            }

            if (data.Count < 1) return;
            // Get the current document and database
            Document acDoc = HostMgd.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable? acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                                OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return;

                // Open the Block table record Model space for write
                BlockTableRecord? acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForRead) as BlockTableRecord;

                if (acBlkTblRec == null) return;

                mLastEntityWrap = new mdsElemDef[data.Count];
                for (int objectIdIndex = 0; objectIdIndex < data.Count; objectIdIndex++)
                {
                    ObjectId oneObjectId = data[objectIdIndex];

                    Entity? ent = acTrans.GetObject(oneObjectId, OpenMode.ForRead) as Entity;
                    if (ent == null) continue;

                    IElement? msObject = null;
                    dynamic entParametric = ent.AcadObject;
                    try
                    {
                        msObject = entParametric.Element;
                    }
                    catch { }

                    if (msObject != null)
                    {
                        mLastEntityWrap[objectIdIndex] = setParametersFrom(msObject);

                    }
                    else mLastEntityWrap[objectIdIndex] = null;
                }
                foreach (ObjectId oneObjectId in data)
                {
                }

                acTrans.Abort();
            }
            setData();
        }

        private mdsElemDef? setParametersFrom(IElement? mstParametricEntity)
        {
            if (mstParametricEntity == null) return null;

            mdsElemDef elemdDef = new mdsElemDef();
            elemdDef.Name = mstParametricEntity.Name;

            IParameters parameters = mstParametricEntity.Parameters;
            foreach (parameter parameter in parameters)
            {
                string caption = parameter.Comment;
                if (caption == "") caption = parameter.Name;
                elemdDef.Parameters.Add(new mdsElemParameterDef() { Caption = caption, Value = parameter.Value, Name = parameter.Name, NativeData = parameter });

                System.Diagnostics.Trace.WriteLine($"Add: " + caption);
            }

            var childs = mstParametricEntity.SubElements;
            if (childs != null && childs.Count > 0)
            {
                elemdDef.Childs = new List<mdsElemDef>();
                foreach (var child in childs)
                {
                    IElement? childElement = child as IElement;
                    mdsElemDef? subMstParametricEntity = setParametersFrom(childElement);
                    if (subMstParametricEntity != null) elemdDef.Childs.Add(subMstParametricEntity);
                }
            }
            return elemdDef;
        }

        private void setData()
        {
            this.TreeView_ElementHierarchy.Items.Clear();
            this.ListView_Info.Items.Clear();
            if (this.mLastEntityWrap == null) return;

            foreach (mdsElemDef? elemdDef in this.mLastEntityWrap)
            {
                if (elemdDef == null) continue;

                TreeViewItem item = new TreeViewItem();
                item.Header = elemdDef.Name;
                item.Tag = elemdDef;

                this.TreeView_ElementHierarchy.Items.Add(item);
                setChildElementsFrom(elemdDef, item);
                item.IsSelected = true;
                item.IsExpanded = true;
            }
        }

        private void setChildElementsFrom(mdsElemDef TreeDef, TreeViewItem elmNode)
        {
            foreach (mdsElemDef chldElm in TreeDef.Childs)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = chldElm.Name;
                item.Tag = chldElm;
                elmNode.Items.Add(item);

                setChildElementsFrom(chldElm, item);
            }
        }

        private void TreeView_AfterSelect(object sender, RoutedEventArgs e)
        {
            TreeView? senderTreeView = sender as TreeView;
            if (senderTreeView == null || senderTreeView.SelectedItem == null)
            {
                return;
            }

            TreeViewItem? tvItem = senderTreeView.SelectedItem as TreeViewItem;
            if (tvItem != null && tvItem.Tag != null)
            {
                mdsElemDef? selectedTagData = tvItem.Tag as mdsElemDef;
                if (selectedTagData != null)
                {
                    showOnListBox(selectedTagData.Parameters);
                }
            }
            else
            {

            }
        }

        private void showOnListBox(List<mdsElemParameterDef>? parameters)
        {
            this.ListView_Info.Items.Clear();
            if (parameters == null) return;

            foreach (var param in parameters)
            {
                this.ListView_Info.Items.Add(param);
            }
        }

        private void Button_CopySingle_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListView_Info.SelectedItem == null) return;

            Clipboard.SetText(this.ListView_Info.SelectedItem.ToString());
        }

        private void Button_CopyAll_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder props = new StringBuilder();
            foreach (mdsElemParameterDef? lvItem in this.ListView_Info.Items)
            {
                if (lvItem == null) continue;
                props.AppendLine($"{lvItem.Name}\t{lvItem.Value}");

            }
            Clipboard.SetText(props.ToString());
        }
    }
}
