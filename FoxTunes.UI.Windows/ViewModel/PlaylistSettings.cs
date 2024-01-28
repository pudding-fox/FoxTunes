using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        protected override void OnCoreChanged()
        {
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
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
            base.OnCoreChanged();
        }

        protected virtual void Refresh()
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
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
