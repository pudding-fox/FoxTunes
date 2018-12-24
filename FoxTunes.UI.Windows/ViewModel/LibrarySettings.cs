using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class LibrarySettings : ViewModelBase
    {
        public ILibraryManager LibraryManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

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
                return new AsyncCommand(this.BackgroundTaskRunner, this.Save)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public async Task Save()
        {
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var libraryHierarchies = database.Set<LibraryHierarchy>(transaction);
                        await libraryHierarchies.RemoveAsync(libraryHierarchies.Except(this.LibraryHierarchies.ItemsSource));
                        await libraryHierarchies.AddOrUpdateAsync(this.LibraryHierarchies.ItemsSource);
                        transaction.Commit();
                    }
                }
                await this.HierarchyManager.Clear();
                await this.HierarchyManager.Build();
            }
            catch (Exception e)
            {
                await this.OnError("Save", e);
                throw;
            }
        }

        public ICommand RescanCommand
        {
            get
            {
                return new AsyncCommand(this.BackgroundTaskRunner, this.Rescan)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public Task Rescan()
        {
            return this.LibraryManager.Rescan();
        }

        public ICommand ClearCommand
        {
            get
            {
                return new AsyncCommand(this.BackgroundTaskRunner, this.Clear)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public async Task Clear()
        {
            await this.HierarchyManager.Clear();
            await this.LibraryManager.Clear();
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
            this.LibraryManager = this.Core.Managers.Library;
            this.HierarchyManager = this.Core.Managers.Hierarchy;
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.LibraryHierarchyLevels = new CollectionManager<LibraryHierarchyLevel>()
            {
                ItemFactory = () =>
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        return database.Set<LibraryHierarchyLevel>().Create().With(libraryHierarchyLevel =>
                        {
                            libraryHierarchyLevel.Name = "New";
                            libraryHierarchyLevel.DisplayScript = "'New'";
                            libraryHierarchyLevel.Sequence = this.LibraryHierarchyLevels.ItemsSource.Count();
                        });
                    }
                },
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.LibraryHierarchies = new CollectionManager<LibraryHierarchy>()
            {
                ItemFactory = () =>
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        return database.Set<LibraryHierarchy>().Create().With(libraryHierarchy =>
                        {
                            libraryHierarchy.Name = "New";
                            libraryHierarchy.Sequence = this.LibraryHierarchies.ItemsSource.Count();
                        });
                    }
                },
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
            this.OnCommandsChanged();
            base.OnCoreChanged();
        }

        protected virtual void OnCommandsChanged()
        {
            this.OnPropertyChanged("RescanCommand");
            this.OnPropertyChanged("ClearCommand");
        }

        protected virtual void Refresh()
        {
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    this.LibraryHierarchies.ItemsSource = new ObservableCollection<LibraryHierarchy>(
                        database.Set<LibraryHierarchy>(transaction)
                    );
                }
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibrarySettings();
        }
    }
}
