using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    //Setting PRIORITY_HIGH so the the cache is cleared before being re-queried.
    [Component("E678301C-B04B-46A9-A275-E1A0AC09F171", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class LibraryCache : StandardComponent, ILibraryCache, IDisposable
    {
        public LibraryCache()
        {
            this.Reset();
        }

        public ConcurrentDictionary<int, Lazy<LibraryItem>> Items { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

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
                case CommonSignals.LibraryUpdated:
                    if (!object.Equals(signal.State, CommonSignalFlags.SOFT))
                    {
                        this.Reset();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public bool TryGetItem(int id, out LibraryItem playlistItem)
        {
            var value = default(Lazy<LibraryItem>);
            if (this.Items.TryGetValue(id, out value))
            {
                playlistItem = value.Value;
                return true;
            }
            playlistItem = null;
            return false;
        }

        public LibraryItem GetItem(int id, Func<LibraryItem> factory)
        {
            return this.Items.GetOrAdd(id, _id => new Lazy<LibraryItem>(factory)).Value;
        }

        public void Reset()
        {
            this.Items = new ConcurrentDictionary<int, Lazy<LibraryItem>>();
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

        ~LibraryCache()
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
