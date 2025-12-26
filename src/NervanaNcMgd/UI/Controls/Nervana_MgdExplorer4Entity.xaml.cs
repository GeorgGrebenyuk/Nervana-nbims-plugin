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
using Teigha.DatabaseServices;


namespace NervanaNcMgd.UI.Controls
{
    /// <summary>
    /// Interaction logic for Nervana_MgdExplorer4Entity.xaml
    /// </summary>
    public partial class Nervana_MgdExplorer4Entity : UserControl
    {
        private MgdExplorerReflection_Handler _handler;
        public Nervana_MgdExplorer4Entity(bool asPalette = false)
        {
            InitializeComponent();

            if (asPalette)
            {
                DocumentCollection dm = Platform.ApplicationServices.Application.DocumentManager;
                dm.MdiActiveDocument.ImpliedSelectionChanged -= new EventHandler(callback_SelectionChanged);
                dm.MdiActiveDocument.ImpliedSelectionChanged += new EventHandler(callback_SelectionChanged);

                dm.DocumentToBeDestroyed += new DocumentCollectionEventHandler(callback_DocumentToBeDestroyed);
                dm.DocumentBecameCurrent += new DocumentCollectionEventHandler(callback_DocumentBecameCurrent);

            }
            _handler = new MgdExplorerReflection_Handler(null);
        }

        private void callback_DocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            setObjectToView(null);
        }

        private void callback_DocumentBecameCurrent(object sender, DocumentCollectionEventArgs e)
        {
            setObjectToView(null);
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

            _handler = new MgdExplorerReflection_Handler(data);
            if (_handler.Items.Count > 0) setObjectToView(_handler.GetData(0));
        }

        //public void onUpdate(object? data)
        //{
        //    _handler = new MgdExplorerReflection_Handler(data);
        //    if (_handler.Items.Count > 0) setObjectToView(_handler.GetData(0));
        //}

        internal void setObjectToView(EParametersGroup[]? data)
        {
            this.ListView_Info.Items.Clear();
            int i_counter = 0;
            if (data == null) return;
            foreach (var group_Definition in data)
            {
                this.ListView_Info.Items.Add(new EParameter($"---{group_Definition.GroupName}---", null) { IsCategory = true });
                i_counter++;

                var group_Parameters = group_Definition.Parameters.OrderBy(p => p.Caption);
                foreach (var groupItem in group_Parameters)
                {
                    this.ListView_Info.Items.Add(groupItem);
                    i_counter++;
                }
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
            this._handler.CopyAllValues(0);
        }
    }
}
