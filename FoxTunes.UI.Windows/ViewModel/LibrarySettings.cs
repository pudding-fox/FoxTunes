using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibrarySettings : ViewModelBase
    {
        public IDatabaseComponent Database { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<LibraryHierarchy> _LibraryHierarchies { get; set; }

        public CollectionManager<LibraryHierarchy> LibraryHierarchies
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
            this.OnPropertyChanged("LibraryHierarchies");
        }

        private CollectionManager<LibraryHierarchyLevel> _LibraryHierarchyLevels { get; set; }

        public CollectionManager<LibraryHierarchyLevel> LibraryHierarchyLevels
        {
            get
            {
                return this._LibraryHierarchyLevels;
            }
            set
            {
                this._LibraryHierarchyLevels = value;
                this.OnLibraryHierarchyLevelsChanged();
            }
        }

        protected virtual void OnLibraryHierarchyLevelsChanged()
        {
            this.OnPropertyChanged("LibraryHierarchyLevels");
        }

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
                return new Command(this.Save)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Save()
        {
            try
            {
                using (var transaction = this.Database.BeginTransaction())
                {
                    var libraryHierarchies = this.Database.Set<LibraryHierarchy>(transaction);
                    libraryHierarchies.Remove(libraryHierarchies.Except(this.LibraryHierarchies.ItemsSource));
                    libraryHierarchies.AddOrUpdate(this.LibraryHierarchies.ItemsSource);
                    transaction.Commit();
                }
                this.SignalEmitter.Send(new Signal(this, CommonSignals.LibraryUpdated));
                this.SignalEmitter.Send(new Signal(this, CommonSignals.HierarchiesUpdated));
            }
            catch (Exception e)
            {
                this.OnError("Save", e);
                throw;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Cancel()
        {
            this.Refresh();
        }

        protected override void OnCoreChanged()
        {
            this.Database = this.Core.Components.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.LibraryHierarchyLevels = new CollectionManager<LibraryHierarchyLevel>()
            {
                ItemFactory = () => this.Database.Sets.LibraryHierarchyLevel.Create().With(libraryHierarchyLevel =>
                {
                    libraryHierarchyLevel.Name = "New";
                    libraryHierarchyLevel.DisplayScript = "'New'";
                }),
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.LibraryHierarchies = new CollectionManager<LibraryHierarchy>()
            {
                ItemFactory = () => this.Database.Sets.LibraryHierarchy.Create().With(libraryHierarchy =>
                {
                    libraryHierarchy.Name = "New";
                }),
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.LibraryHierarchies.SelectedValueChanged += (sender, e) =>
            {
                if (this.LibraryHierarchies.SelectedValue != null)
                {
                    this.LibraryHierarchyLevels.ItemsSource = this.LibraryHierarchies.SelectedValue.Levels;
                }
                else
                {
                    this.LibraryHierarchyLevels.ItemsSource = null;
                }
            };
            this.Refresh();
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
        {
            this.LibraryHierarchies.ItemsSource = new ObservableCollection<LibraryHierarchy>(this.Database.Sets.LibraryHierarchy);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibrarySettings();
        }
    }
}
