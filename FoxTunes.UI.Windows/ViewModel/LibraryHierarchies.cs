using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchies : ViewModelBase
    {
        public ISignalEmitter SignalEmitter { get; private set; }

        private IDatabaseContext _DatabaseContext { get; set; }

        public IDatabaseContext DatabaseContext
        {
            get
            {
                return this._DatabaseContext;
            }
            set
            {
                this._DatabaseContext = value;
                this.OnDatabaseContextChanged();
            }
        }

        protected virtual void OnDatabaseContextChanged()
        {
            if (this.DatabaseContextChanged != null)
            {
                this.DatabaseContextChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DatabaseContext");
        }

        public event EventHandler DatabaseContextChanged = delegate { };

        private LibraryHierarchy _SelectedHierarchy { get; set; }

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                return this._SelectedHierarchy;
            }
            set
            {
                this._SelectedHierarchy = value;
                this.OnSelectedHierarchyChanged();
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
        }

        public event EventHandler SelectedHierarchyChanged = delegate { };

        private LibraryHierarchyLevel _SelectedHierarchyLevel { get; set; }

        public LibraryHierarchyLevel SelectedHierarchyLevel
        {
            get
            {
                return this._SelectedHierarchyLevel;
            }
            set
            {
                this._SelectedHierarchyLevel = value;
                this.OnSelectedHierarchyLevelChanged();
            }
        }

        protected virtual void OnSelectedHierarchyLevelChanged()
        {
            if (this.SelectedHierarchyLevelChanged != null)
            {
                this.SelectedHierarchyLevelChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchyLevel");
        }

        public event EventHandler SelectedHierarchyLevelChanged = delegate { };

        private bool _SettingsVisible { get; set; }

        public bool SettingsVisible
        {
            get
            {
                return this._SettingsVisible;
            }
            set
            {
                this._SettingsVisible = value;
                this.OnSettingsVisibleChanged();
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged = delegate { };

        public ICommand NewCommand
        {
            get
            {
                return new Command(
                    () => this.DatabaseContext.Sets.LibraryHierarchy.Add(new LibraryHierarchy()),
                    () => this.DatabaseContext != null
                );
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new Command(
                    () => this.DatabaseContext.Sets.LibraryHierarchy.Remove(this.SelectedHierarchy),
                    () => this.DatabaseContext != null && this.SelectedHierarchy != null
                );
            }
        }

        public ICommand UpdateCommand
        {
            get
            {
                return new Command(this.Update);
            }
        }

        public void Update()
        {
            if (this.SelectedHierarchy.Id == 0 && this.SelectedHierarchyLevel != null)
            {
                return;
            }
            this.DatabaseContext.GetSet<LibraryHierarchyLevel>().Update(this.SelectedHierarchyLevel);
        }

        public ICommand SaveCommand
        {
            get
            {
                return new Command(this.Save);
            }
        }

        public void Save()
        {
            this.DatabaseContext.SaveChanges();
            this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
        }

        public void Reload()
        {
            if (this.DatabaseContext != null)
            {
                this.DatabaseContext.Dispose();
            }

        }

        protected override void OnCoreChanged()
        {
            this.DatabaseContext = this.Core.Managers.Data.CreateWriteContext();
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchies();
        }
    }
}
