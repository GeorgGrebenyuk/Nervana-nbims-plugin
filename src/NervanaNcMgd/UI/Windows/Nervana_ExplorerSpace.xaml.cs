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
using NervanaNcMgd.UI.Controls;

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

        private Nervana_MgdExplorer4Entity _listview;

        public Nervana_ExplorerSpace(object? data, bool asPalatte = false)
        {
            InitializeComponent();
            _listview = new Nervana_MgdExplorer4Entity();
            this.GroupBoxForListView.Content = _listview;

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
            _listview.setObjectToView(data);
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

        
    }
}
