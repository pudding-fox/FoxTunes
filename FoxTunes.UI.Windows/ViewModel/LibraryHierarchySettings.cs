using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchySettings : ViewModelBase
    {
        public IDatabase Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private LibraryHierarchy _SelectedLibraryHierarchy { get; set; }

        public LibraryHierarchy SelectedLibraryHierarchy
        {
            get
            {
                return this._SelectedLibraryHierarchy;
            }
            set
            {
                this._SelectedLibraryHierarchy = value;
                this.OnSelectedLibraryHierarchyChanged();
            }
        }

        protected virtual void OnSelectedLibraryHierarchyChanged()
        {
            if (this.SelectedLibraryHierarchyChanged != null)
            {
                this.SelectedLibraryHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedLibraryHierarchy");
        }

        public event EventHandler SelectedLibraryHierarchyChanged = delegate { };

        private ObservableCollection<LibraryHierarchy> _LibraryHierarchies { get; set; }

        public ObservableCollection<LibraryHierarchy> LibraryHierarchies
        {
            get
            {
                return this._LibraryHierarchies;
            }
            set
            {
                this._LibraryHierarchies = value;
                this.OnLibraryHierarchiesChanged();
            }
        }

        protected virtual void OnLibraryHierarchiesChanged()
        {
            if (this.LibraryHierarchiesChanged != null)
            {
                this.LibraryHierarchiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LibraryHierarchies");
        }

        public event EventHandler LibraryHierarchiesChanged = delegate { };

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
                    () => this.LibraryHierarchies.Add(new LibraryHierarchy()),
                    () => this.LibraryHierarchies != null
                );
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new Command(
                    () => this.LibraryHierarchies.Remove(this.SelectedLibraryHierarchy),
                    () => this.LibraryHierarchies != null && this.SelectedLibraryHierarchy != null
                );
            }
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
            using (var transaction = this.Database.BeginTransaction())
            {
                var libraryHierarchies = this.Database.GetSet<LibraryHierarchy>(transaction);
                var libraryHierarchyLevels = this.Database.GetSet<LibraryHierarchyLevel>(transaction);
                libraryHierarchies.Delete(libraryHierarchies.Except(this.LibraryHierarchies));
                libraryHierarchies.AddOrUpdate(this.LibraryHierarchies);
                foreach (var libraryHierarchy in this.LibraryHierarchies)
                {
                    libraryHierarchy.Levels.Do(libraryHierarchyLevel => libraryHierarchyLevel.LibraryHierarchyId = libraryHierarchy.Id).Enumerate();
                    libraryHierarchyLevels.Delete(libraryHierarchies.Find(libraryHierarchy.Id).Levels.Except(libraryHierarchy.Levels));
                    libraryHierarchyLevels.AddOrUpdate(libraryHierarchy.Levels);
                }
                transaction.Commit();
            }
            this.Refresh();
            this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
            this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.Refresh();
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
        {
            this.LibraryHierarchies = new ObservableCollection<LibraryHierarchy>(this.Database.Sets.LibraryHierarchy);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchySettings();
        }
    }
}
