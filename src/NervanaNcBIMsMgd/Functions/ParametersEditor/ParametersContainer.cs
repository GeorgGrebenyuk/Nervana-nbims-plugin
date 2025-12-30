using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NervanaNcBIMsMgd.Functions.ParametersEditor
{
    internal class ParametersContainer : INotifyPropertyChanged
    {
        private int paramId { get; set; }
        private string _name { get; set; }
        private string _caption { get; set; }
        private string _value { get; set; }

        internal string Category { get; set; }

        private bool isCategory { get; set; }


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


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
