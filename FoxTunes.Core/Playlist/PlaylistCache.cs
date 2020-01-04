using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class PlaylistCache : StandardComponent, IPlaylistCache, IDisposable
    {
        public Lazy<IList<PlaylistItem>> Items { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public PlaylistCache()
        {
            this.Reset();
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    if (!object.Equals(signal.State, CommonSignalFlags.SOFT))
                    {
                        Logger.Write(this, LogLevel.Debug, "Playlist was updated, resetting cache.");
                        this.Reset();
                    }
                    else
                    {
                        Logger.Write(this, LogLevel.Debug, "Playlist was updated but soft flag was specified, ignoring.");
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<PlaylistItem> GetItems(Func<IEnumerable<PlaylistItem>> factory)
        {
            if (this.Items == null)
            {
                this.Items = new Lazy<IList<PlaylistItem>>(() => new List<PlaylistItem>(factory()));
            }
            return this.Items.Value;
        }

        public void Reset()
        {
            this.Items = null;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
        }

        ~PlaylistCache()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
