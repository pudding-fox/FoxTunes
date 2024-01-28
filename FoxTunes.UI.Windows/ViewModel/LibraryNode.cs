using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibraryNode : ViewModelBase
    {
        public LibraryNode()
        {

        }

        public LibraryNode(string header, IEnumerable<LibraryNode> items)
        {
            this.Header = header;
            this.Items = new ObservableCollection<LibraryNode>(items);
        }

        private string _Header { get; set; }

        public string Header
        {
            get
            {
                return this._Header;
            }
            set
            {
                this._Header = value;
                this.OnHeaderChanged();
            }
        }

        protected virtual void OnHeaderChanged()
        {
            if (this.HeaderChanged != null)
            {
                this.HeaderChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Header");
        }

        public event EventHandler HeaderChanged = delegate { };

        public ObservableCollection<LibraryNode> Items { get; private set; }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryNode();
        }
    }
}
