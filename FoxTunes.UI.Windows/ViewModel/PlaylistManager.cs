using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistManager : ViewModelBase
    {
        public PlaylistManager()
        {
            this.WindowState = new WindowState(PlaylistManagerWindow.ID);
        }

        public WindowState WindowState { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IErrorEmitter ErrorEmitter { get; private set; }

        private CollectionManager<Playlist> _Playlists { get; set; }

        public CollectionManager<Playlist> Playlists
        {
            get
            {
                return this._Playlists;
            }
            set
            {
                this._Playlists = value;
                this.OnPlaylistsChanged();
            }
        }

        protected virtual void OnPlaylistsChanged()
        {
            this.OnPropertyChanged("Playlists");
        }

        protected override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.DatabaseFactory = core.Factories.Database;
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.ErrorEmitter = core.Components.ErrorEmitter;
            this.Playlists = new CollectionManager<Playlist>()
            {
                ItemFactory = () => new Playlist()
                {
                    Name = Playlist.GetName(this.Playlists.ItemsSource),
                    Enabled = true
                },
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual async void OnActiveChanged(object sender, EventArgs e)
        {
            await Windows.Invoke(() => this.OnIsSavingChanged()).ConfigureAwait(false);
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null && playlists.Any())
                    {

                    }
                    else
                    {
                        return this.Refresh();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Refresh()
        {
            var playlists = new List<Playlist>();
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    playlists.AddRange(database.Set<Playlist>(transaction));
                }
            }
            //Use the "live" playlist instances where possible.
            var cached = this.PlaylistBrowser.GetPlaylists().ToDictionary(
                playlist => playlist.Id
            );
            for (var a = 0; a < playlists.Count; a++)
            {
                var playlist = default(Playlist);
                if (cached.TryGetValue(playlists[a].Id, out playlist))
                {
                    playlists[a] = playlist;
                }
            }
            return Windows.Invoke(() => this.Playlists.ItemsSource = new ObservableCollection<Playlist>(playlists));
        }

        public bool PlaylistManagerVisible
        {
            get
            {
                return Windows.Registrations.IsVisible(PlaylistManagerWindow.ID);
            }
            set
            {
                if (value)
                {
                    Windows.Registrations.Show(PlaylistManagerWindow.ID);
                }
                else
                {
                    Windows.Registrations.Hide(PlaylistManagerWindow.ID);
                }
            }
        }

        protected virtual void OnPlaylistManagerVisibleChanged()
        {
            if (this.PlaylistManagerVisibleChanged != null)
            {
                this.PlaylistManagerVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("PlaylistManagerVisible");
        }

        public event EventHandler PlaylistManagerVisibleChanged;

        public bool IsSaving
        {
            get
            {
                return global::FoxTunes.BackgroundTask.Active
                    .OfType<PlaylistTaskBase>()
                    .Any();
            }
        }

        protected virtual void OnIsSavingChanged()
        {
            if (this.IsSavingChanged != null)
            {
                this.IsSavingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSaving");
        }

        public event EventHandler IsSavingChanged;

        public ICommand SaveCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Save);
            }
        }

        public async Task Save()
        {
            var exception = default(Exception);
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                        {
                            var playlists = database.Set<Playlist>(transaction);
                            foreach (var playlist in this.Playlists.Removed)
                            {
                                await PlaylistTaskBase.RemovePlaylistItems(database, playlist.Id, PlaylistItemStatus.None, transaction).ConfigureAwait(false);
                                await playlists.RemoveAsync(playlist).ConfigureAwait(false);
                            }
                            playlists.AddOrUpdate(this.Playlists.ItemsSource);
                            transaction.Commit();
                        }
                    }))
                    {
                        await task.Run().ConfigureAwait(false);
                    }
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }
            await this.ErrorEmitter.Send("Save", exception).ConfigureAwait(false);
            throw exception;
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel);
            }
        }

        public void Cancel()
        {
            this.Dispatch(this.Refresh);
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
            return new PlaylistManager();
        }
    }
}
