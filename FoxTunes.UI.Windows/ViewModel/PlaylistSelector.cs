using FoxDb;
using System.Threading.Tasks;
using System;
using System.Windows.Input;
using FoxTunes.Interfaces;

namespace FoxTunes.ViewModel
{
    public class PlaylistSelector : Playlists
    {
        const int TIMEOUT = 100;

        public PlaylistSelector()
        {
            this.Debouncer = new AsyncDebouncer(TIMEOUT);
        }

        public AsyncDebouncer Debouncer { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public IErrorEmitter ErrorEmitter { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.DatabaseFactory = core.Factories.Database;
            this.ErrorEmitter = core.Components.ErrorEmitter;
            base.InitializeComponent(core);
        }

        public ICommand ExchangeCommand
        {
            get
            {
                return new Command<object[]>(items => this.Exchange(items), items => this.CanExchange(items));
            }
        }

        public bool CanExchange(object[] items)
        {
            if (items == null || items.Length != 2)
            {
                return false;
            }
            if (!(items[0] is Playlist) || !(items[1] is Playlist))
            {
                return false;
            }
            return true;
        }

        public void Exchange(object[] items)
        {
            var playlist1 = (Playlist)items[0];
            var playlist2 = (Playlist)items[1];
            this.Exchange(playlist1, playlist2);
        }

        public void Exchange(Playlist playlist1, Playlist playlist2)
        {
            var temp = playlist1.Sequence;
            playlist1.Sequence = playlist2.Sequence;
            playlist2.Sequence = temp;
            this.Debouncer.Exec(this.Save);
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
                            await playlists.AddOrUpdateAsync(this.Items).ConfigureAwait(false);
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
            await this.ErrorEmitter.Send(this, "Save", exception).ConfigureAwait(false);
            throw exception;
        }

        protected override void OnDisposing()
        {
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
            }
            base.OnDisposing();
        }
    }
}
