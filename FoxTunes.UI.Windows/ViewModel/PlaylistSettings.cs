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
    public class PlaylistSettings : ViewModelBase
    {
        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<PlaylistColumn> _PlaylistColumns { get; set; }

        public CollectionManager<PlaylistColumn> PlaylistColumns
        {
            get
            {
                return this._PlaylistColumns;
            }
            set
            {
                this._PlaylistColumns = value;
                this.OnPlaylistColumnsChanged();
            }
        }

        protected virtual void OnPlaylistColumnsChanged()
        {
            this.OnPropertyChanged("PlaylistColumns");
        }

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
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        var playlistColumns = database.Set<PlaylistColumn>(transaction);
                        playlistColumns.Remove(playlistColumns.Except(this.PlaylistColumns.ItemsSource));
                        playlistColumns.AddOrUpdate(this.PlaylistColumns.ItemsSource);
                        transaction.Commit();
                    }
                }
                this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated));
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

        public override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistColumns = new CollectionManager<PlaylistColumn>()
            {
                ItemFactory = () =>
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        return database.Set<PlaylistColumn>().Create().With(playlistColumn =>
                        {
                            playlistColumn.Name = "New";
                            playlistColumn.DisplayScript = "'New'";
                            playlistColumn.Sequence = this.PlaylistColumns.ItemsSource.Count();
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
            this.Refresh();
            base.InitializeComponent(core);
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.SettingsUpdated:
                case CommonSignals.PlaylistColumnsUpdated:
                    return this.Refresh();
            }
            return Task.CompletedTask;
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                    {
                        this.PlaylistColumns.ItemsSource = new ObservableCollection<PlaylistColumn>(
                            database.Set<PlaylistColumn>(transaction)
                        );
                    }
                }
            });
        }

        protected override void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
