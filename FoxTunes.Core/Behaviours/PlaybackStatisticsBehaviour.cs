using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaybackStatisticsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
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
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            }
        }

        public void Disable()
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.IncrementPlayCount(this.PlaybackManager.CurrentStream));
#else
            var task = Task.Run(() => this.IncrementPlayCount(this.PlaybackManager.CurrentStream));
#endif
        }

        protected virtual Task IncrementPlayCount(IOutputStream currentStream)
        {
            if (currentStream == null || currentStream.PlaylistItem == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.IncrementPlayCount(currentStream.PlaylistItem);
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
