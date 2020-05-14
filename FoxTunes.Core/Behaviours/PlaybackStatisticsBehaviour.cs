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

        public BooleanConfigurationElement MetaDataEnabled { get; private set; }

        public BooleanConfigurationElement PlaybackStatisticsEnabled { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.Configuration = core.Components.Configuration;
            this.MetaDataEnabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.ENABLE_ELEMENT
            );
            this.MetaDataEnabled.ValueChanged += this.OnValueChanged;
            this.PlaybackStatisticsEnabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaybackBehaviourConfiguration.SECTION,
                PlaybackStatisticsBehaviourConfiguration.ENABLED
            );
            this.PlaybackStatisticsEnabled.ValueChanged += this.OnValueChanged;
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected virtual void Refresh()
        {
            if (this.MetaDataEnabled.Value && this.PlaybackStatisticsEnabled.Value)
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
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

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(() => this.IncrementPlayCount(this.PlaybackManager.CurrentStream));
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
            if (this.MetaDataEnabled != null)
            {
                this.MetaDataEnabled.ValueChanged -= this.OnValueChanged;
            }
            if (this.PlaybackStatisticsEnabled != null)
            {
                this.PlaybackStatisticsEnabled.ValueChanged -= this.OnValueChanged;
            }
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
