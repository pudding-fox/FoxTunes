using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaybackStatisticsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int TIMEOUT = 1;

        const int DELAY = 10000;

        public PlaybackStatisticsBehaviour()
        {
            this.Queue = new Queue<PlaylistItem>();
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public Queue<PlaylistItem> Queue { get; private set; }

        public SemaphoreSlim Semaphore { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutput Output { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.Output = core.Components.Output;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaybackStatisticsBehaviourConfiguration.SECTION,
                PlaybackStatisticsBehaviourConfiguration.ENABLED
            );
            this.Enabled.ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            if (this.Output != null)
            {
                this.Output.IsStartedChanged += this.OnIsStartedChanged;
            }
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            }
        }

        public void Disable()
        {
            if (this.Output != null)
            {
                this.Output.IsStartedChanged -= this.OnIsStartedChanged;
            }
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
        }

        protected virtual async void OnIsStartedChanged(object sender, AsyncEventArgs e)
        {
            if (this.Output.IsStarted)
            {
                return;
            }
            //When the output is stopped we can empty the queue.
            if (Core.IsShuttingDown)
            {
                //If we're shutting down we need to block while we write our data.
                using (e.Defer())
                {
                    await this.Dequeue(false).ConfigureAwait(false);
                    return;
                }
            }
#if NET40
            var task = TaskEx.Run(() => this.Dequeue(true));
#else
            var task = Task.Run(() => this.Dequeue(true));
#endif
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.Enqueue(this.PlaybackManager.CurrentStream));
#else
            var task = Task.Run(() => this.Enqueue(this.PlaybackManager.CurrentStream));
#endif
        }

        protected virtual Task Enqueue(IOutputStream currentStream)
        {
            if (currentStream != null && currentStream.PlaylistItem != null)
            {
                if (!MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value).HasFlag(WriteBehaviour.Statistics))
                {
                    //If we're not writing tags to files we can do this the easy way.
                    return this.IncrementPlayCount(currentStream.PlaylistItem);
                }
                //Else we need to wait for the file to become available.
                this.Queue.Enqueue(currentStream.PlaylistItem);
                if (this.Queue.Count > 1)
                {
                    //Only try to write if there's more than one item in the queue.
                    //Otherwise nothing can be written (as the track is being played).
                    return this.Dequeue(true);
                }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual async Task Dequeue(bool delay)
        {
#if NET40
            if (!this.Semaphore.Wait(TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(TIMEOUT).ConfigureAwait(false))
#endif
            {
                //TODO: Warn.
                return;
            }
            try
            {
                if (delay)
                {
                    //Wait a moment for the playback queue to settle.
#if NET40
                    await TaskEx.Delay(DELAY).ConfigureAwait(false);
#else
                    await Task.Delay(DELAY).ConfigureAwait(false);
#endif
                }
                var count = this.Queue.Count;
                while (count-- > 0)
                {
                    var playlistItem = this.Queue.Dequeue();
                    if (this.Output.IsStarted)
                    {
                        if (this.OutputStreamQueue.IsQueued(playlistItem))
                        {
                            //File is likely in use, enqueued. Push it to the back.
                            this.Queue.Enqueue(playlistItem);
                            continue;
                        }
                        if (this.PlaybackManager.CurrentStream != null && string.Equals(this.PlaybackManager.CurrentStream.FileName, playlistItem.FileName, StringComparison.OrdinalIgnoreCase))
                        {
                            //File is likely in use, being played. Push it to the back.
                            this.Queue.Enqueue(playlistItem);
                            continue;
                        }
                    }
                    await this.IncrementPlayCount(playlistItem).ConfigureAwait(false);
                }
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task IncrementPlayCount(PlaylistItem playlistItem)
        {
            try
            {
                await this.PlaylistManager.IncrementPlayCount(playlistItem).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Failed to update play count for file \"{0}\": {1}", playlistItem.FileName, e.Message);
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaybackStatisticsBehaviourConfiguration.GetConfigurationSections();
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
            this.Disable();
        }

        ~PlaybackStatisticsBehaviour()
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
