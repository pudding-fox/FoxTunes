using System;
using System.Collections.ObjectModel;
using FoxTunes.Interfaces;
using System.Linq;

namespace FoxTunes
{
    public class LibraryHierarchy : PersistableComponent
    {
        public LibraryHierarchy()
        {

        }

        public ICore Core { get; private set; }

        public IDatabase Database { get; private set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged = delegate { };

        private ObservableCollection<LibraryHierarchyLevel> _Levels { get; set; }

        public ObservableCollection<LibraryHierarchyLevel> Levels
        {
            get
            {
                if (this._Levels == null)
                {
                    this.LoadLevels();
                }
                return this._Levels;
            }
            set
            {
                this._Levels = value;
                this.OnLevelsChanged();
            }
        }

        protected virtual void OnLevelsChanged()
        {
            if (this.LevelsChanged != null)
            {
                this.LevelsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Levels");
        }

        public event EventHandler LevelsChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Database = core.Components.Database;
            base.InitializeComponent(core);
        }

        public void LoadLevels()
        {
            if (this.Database == null)
            {
                this.Levels = new ObservableCollection<LibraryHierarchyLevel>();
                return;
            }
            this.Levels = new ObservableCollection<LibraryHierarchyLevel>(LibraryHierarchyInfo.GetLevels(this.Core, this.Database, this));
        }
    }
}
