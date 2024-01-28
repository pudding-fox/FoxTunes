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

        public LibraryNode(string header, IEnumerable<LibraryItem> libraryItems, IEnumerable<LibraryNode> libraryNodes)
        {
            this.Header = header;
            this.LibraryItems = new ObservableCollection<LibraryItem>(libraryItems);
            this.LibraryNodes = new ObservableCollection<LibraryNode>(libraryNodes);
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

        public ObservableCollection<LibraryItem> LibraryItems { get; private set; }

        public ObservableCollection<LibraryNode> LibraryNodes { get; private set; }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryNode();
        }
    }
}
