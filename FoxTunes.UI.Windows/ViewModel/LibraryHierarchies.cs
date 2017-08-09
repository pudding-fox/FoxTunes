using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchies : ViewModelBase
    {
        public ILibrary Library { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

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

        public ICommand SaveCommand
        {
            get
            {
                return null;
            }
        }

        public ICommand NewCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.LibraryManager.AddHierarchy(new LibraryHierarchy()),
                    () => this.LibraryManager != null
                );
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.LibraryManager.DeleteHierarchy(this.SelectedHierarchy),
                    () => this.LibraryManager != null && this.SelectedHierarchy != null
                );
            }
        }

        public ICommand RebuildCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.LibraryManager.BuildHierarchies(),
                    () => this.LibraryManager != null
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.Library = this.Core.Components.Library;
            this.LibraryManager = this.Core.Managers.Library;
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchies();
        }
    }
}
