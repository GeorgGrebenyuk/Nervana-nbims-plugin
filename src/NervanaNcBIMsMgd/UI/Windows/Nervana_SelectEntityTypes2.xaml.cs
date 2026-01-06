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
using System.Windows.Shapes;

using NervanaNcBIMsMgd.UI.Controls;

namespace NervanaNcBIMsMgd.UI.Windows
{
    /// <summary>
    /// Interaction logic for Nervana_SelectEntityTypes2.xaml
    /// </summary>
    public partial class Nervana_SelectEntityTypes2 : Window
    {
        public Nervana_SelectEntityTypes2(bool isNCBIMsOnly = true)
        {
            InitializeComponent();

            Type[] types;
            if (isNCBIMsOnly) types = getTargetTypes(typeof(BIMStructureMgd.ObjectProperties.IParametricObject));
            else types = getTargetTypes(typeof(Teigha.DatabaseServices.Polyline));

            types = types.OrderBy(t => t.Name).ToArray();
            setTypesToListbox(types);

            this.ListView_Types.SelectionMode = SelectionMode.Multiple;

            this.Button_SaveTypes.Click += Button_SaveTypes_Click;
            SelectedTypes = new Type[] { };
        }

        private void Button_SaveTypes_Click(object sender, RoutedEventArgs e)
        {
            var selItems = this.ListView_Types.SelectedItems;
            if (selItems.Count < 1) return;
            SelectedTypes = new Type[selItems.Count];
            for (int typeIndex = 0; typeIndex < selItems.Count; typeIndex++)
            {
                SelectedTypes[typeIndex] = (Type)selItems[typeIndex];
            }
            this.DialogResult = true;
            this.Close();
        }

        private Type[] getTargetTypes(Type assType)
        {
            return assType.Assembly.GetTypes().Where(type => type.IsAssignableTo(assType)).ToArray();
        }

        private void setTypesToListbox(Type[] types)
        {
            this.ListView_Types.Items.Clear();
            foreach (Type type in types)
            {
                this.ListView_Types.Items.Add(type);

            }
        }

        public Type[] SelectedTypes;
    }
}
