using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NervanaNcMgd.Functions;
using HostMgd.ApplicationServices;

using Platform = HostMgd;
using NativePlatform = Teigha;
using HostMgd.EditorInput;

namespace NervanaNcMgd.UI.Windows
{
    /// <summary>
    /// Interaction logic for ExplorerSpace.xaml
    /// </summary>
    public partial class Nervana_ExplorerSpace : Window
    {
        private MgdExplorerReflection_Handler _handler;
        private int p_ShowedTag = -1;
        private Style p_ListViewStyle_Bold;
        private Style p_ListViewStyle_Common;
        private Style p_ListViewStyle_Category;

        public Nervana_ExplorerSpace(object? data, bool asPalatte = false)
        {
            InitializeComponent();

            if (asPalatte)
            {
                DocumentCollection dm = Platform.ApplicationServices.Application.DocumentManager;
                dm.MdiActiveDocument.ImpliedSelectionChanged -= new EventHandler(callback_SelectionChanged);
                dm.MdiActiveDocument.ImpliedSelectionChanged += new EventHandler(callback_SelectionChanged);
            }
            

            //Init styles
            p_ListViewStyle_Common = new Style();
            p_ListViewStyle_Common.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Black });
            p_ListViewStyle_Common.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 12.0 });
            p_ListViewStyle_Common.Setters.Add(new Setter { Property = Control.FontWeightProperty, Value = FontWeights.Normal });

            p_ListViewStyle_Bold = new Style();
            p_ListViewStyle_Bold.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Black });
            p_ListViewStyle_Bold.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 12.0 });
            p_ListViewStyle_Bold.Setters.Add(new Setter { Property = Control.FontWeightProperty, Value = FontWeights.Bold });

            p_ListViewStyle_Category = new Style();
            p_ListViewStyle_Category.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Blue });
            p_ListViewStyle_Category.Setters.Add(new Setter { Property = Control.FontSizeProperty, Value = 12.0 });
            p_ListViewStyle_Category.Setters.Add(new Setter { Property = Control.FontWeightProperty, Value = FontWeights.Normal });

            this.ListView_Info.SelectionMode = System.Windows.Controls.SelectionMode.Single;

            onUpdate(data);
        }

        private void onUpdate(object? data)
        {
            _handler = new MgdExplorerReflection_Handler(data);
            this.TreeView_SourceData.Items.Clear();
            for (int name_counter = 0; name_counter < _handler.Items.Count; name_counter++)
            {
                ETreeItem pseudoTreeItemDef = _handler.Items[name_counter];

                TreeViewItem item = new TreeViewItem();
                item.Header = pseudoTreeItemDef.Name;
                item.Tag = pseudoTreeItemDef.Tag;
                //TODO: Set style

                this.TreeView_SourceData.Items.Add(item);
                setChildElementsFrom(pseudoTreeItemDef, item);
                item.IsSelected = true;
                item.IsExpanded = true;
            }
        }

        private void callback_SelectionChanged(object? sender, EventArgs e)
        {
            object? data = null;
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
            onUpdate(data);
        }

        private void setChildElementsFrom(ETreeItem TreeDef, TreeViewItem elmNode)
        {

            foreach (ETreeItem chldElm in TreeDef.Items)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = chldElm.Name;
                item.Tag = chldElm.Tag;
                elmNode.Items.Add(item);

                setChildElementsFrom(chldElm, item);
            }
        }

        private void setObjectToView(EParametersGroup[]? data)
        {
            this.ListView_Info.Items.Clear();
            int i_counter = 0;
            if (data == null) return;
            foreach (var group_Definition in data)
            {
                this.ListView_Info.Items.Add(new EParameter($"---{group_Definition.GroupName}---", null) {IsCategory = true});
                i_counter++;

                //OwnerTypes.Sort((t1, t2) => t1.IsSubclassOf(t2).CompareTo(t2.IsSubclassOf(t1)));
                var group_Parameters = group_Definition.Parameters.OrderBy(p => p.Caption);//  .Sort((p1, p2) => p1.Caption.CompareTo(p2.Caption));
                foreach (var groupItem in group_Parameters)
                {
                    //TODO: Set style and delete column CanDeep
                    //groupItem.Style = p_ListViewStyle_Common;
                    //if (groupItem.PType == EParameter_Type.CanExplore) groupItem.Style = p_ListViewStyle_Bold;
                    //if (groupItem.IsCategory) groupItem.Style = p_ListViewStyle_Category;
                    this.ListView_Info.Items.Add(groupItem);
                    i_counter++;  
                }
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
                int iTag = (int)tvItem.Tag;
                p_ShowedTag = iTag;
                var data = _handler.GetData(iTag);
                setObjectToView(data);
            }
            else
            {
                //TODO: log (not select or tag not exists)
            }


        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView? senderListView = sender as ListView;
            if (senderListView == null || senderListView.SelectedItem == null) return;

            this._handler.ExploreValue(this.ListView_Info.SelectedItem);
            
        }
        private void Button_CopySingle_Click(object sender, RoutedEventArgs e)
        {
            if (this.ListView_Info.SelectedItem == null) return;

            this._handler.CopySingleValue(this.ListView_Info.SelectedItem);
        }

        private void Button_CopyAll_Click(object sender, RoutedEventArgs e)
        {
            this._handler.CopyAllValues(this.p_ShowedTag);
        }
    }
}
