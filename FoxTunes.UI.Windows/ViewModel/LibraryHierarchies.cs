using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibraryHierarchies : ViewModelBase
    {
        public IHierarchyManager HierarchyManager { get; private set; }

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
                    () => this.HierarchyManager.AddHierarchy(new LibraryHierarchy()),
                    () => this.HierarchyManager != null
                );
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.HierarchyManager.DeleteHierarchy(this.SelectedHierarchy),
                    () => this.HierarchyManager != null && this.SelectedHierarchy != null
                );
            }
        }

        public ICommand RebuildCommand
        {
            get
            {
                return new AsyncCommand(
                    () => this.HierarchyManager.BuildHierarchies(),
                    () => this.HierarchyManager != null
                );
            }
        }

        protected override void OnCoreChanged()
        {
            this.HierarchyManager = this.Core.Managers.Hierarchy;
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibraryHierarchies();
        }
    }
}
