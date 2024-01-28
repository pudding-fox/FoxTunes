using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Timers;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace FoxTunes
{
    public class SystemMediaTransportControlsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        public SystemMediaTransportControls TransportControls { get; private set; }

        public Timer Timer { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool Enabled
        {
            get
            {
                return this.TransportControls != null;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            if (SystemMediaTransportControlsBehaviourConfiguration.IsPlatformSupported)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaybackManager = core.Managers.Playback;
                this.ArtworkProvider = core.Components.ArtworkProvider;
                this.Configuration = core.Components.Configuration;
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    SystemMediaTransportControlsBehaviourConfiguration.SECTION,
                    SystemMediaTransportControlsBehaviourConfiguration.ENABLED_ELEMENT
                ).ConnectValue(value =>
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
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Platform is not supported.");
            }
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            if (this.Enabled)
            {
                return;
            }
#pragma warning disable CS0618
            this.TransportControls = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
#pragma warning restore CS0618
            this.TransportControls.IsEnabled = true;
            this.TransportControls.IsPlayEnabled = true;
            this.TransportControls.IsPauseEnabled = true;
            this.TransportControls.IsStopEnabled = true;
            //this.TransportControls.ShuffleEnabled = true;
            this.TransportControls.IsNextEnabled = true;
            this.TransportControls.IsPreviousEnabled = true;
            //this.TransportControls.IsFastForwardEnabled = false;
            //this.TransportControls.IsRewindEnabled = false;
            this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            this.TransportControls.ButtonPressed += this.OnButtonPressed;
            //this.TransportControls.AutoRepeatModeChangeRequested += this.OnAutoRepeatModeChangeRequested;
            //this.TransportControls.PlaybackPositionChangeRequested += this.OnPlaybackPositionChangeRequested;
            //this.TransportControls.PlaybackRateChangeRequested += this.OnPlaybackRateChangeRequested;
            //this.TransportControls.ShuffleEnabledChangeRequested += this.OnShuffleEnabledChangeRequested;
            //this.TransportControls.PropertyChanged += this.OnPropertyChanged;
            this.Timer = new Timer();
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.Elapsed += this.OnElapsed;
            this.Timer.Start();
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.Refresh();
            Logger.Write(this, LogLevel.Debug, "SystemMediaTransportControls enabled.");
        }

        public void Disable()
        {
            if (!this.Enabled)
            {
                return;
            }
            this.Timer.Stop();
            this.Timer.Elapsed -= this.OnElapsed;
            this.Timer.Dispose();
            this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            this.Timer = null;
            this.TransportControls.IsEnabled = false;
            this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            this.TransportControls.ButtonPressed -= this.OnButtonPressed;
            //this.TransportControls.AutoRepeatModeChangeRequested -= this.OnAutoRepeatModeChangeRequested;
            //this.TransportControls.PlaybackPositionChangeRequested -= this.OnPlaybackPositionChangeRequested;
            //this.TransportControls.PlaybackRateChangeRequested -= this.OnPlaybackRateChangeRequested;
            //this.TransportControls.ShuffleEnabledChangeRequested -= this.OnShuffleEnabledChangeRequested;
            //this.TransportControls.PropertyChanged -= this.OnPropertyChanged;
            this.TransportControls = null;
            Logger.Write(this, LogLevel.Debug, "SystemMediaTransportControls disabled.");
        }

        public void Refresh()
        {
            this.RefreshState();
            this.RefreshStream();
            this.RefreshPosition();
        }

        protected virtual void RefreshState()
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
            else
            {
                if (outputStream.IsPlaying)
                {
                    this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                }
                else if (outputStream.IsPaused)
                {
                    this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                }
                else if (outputStream.IsStopped)
                {
                    this.TransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                }
            }
        }

        protected virtual void RefreshStream()
        {
            {
                var task = this.UpdateMusicProperties();
            }
            {
                var task = this.UpdateThumbnail();
            }
        }

        protected virtual void RefreshPosition()
        {
            //var properties = new SystemMediaTransportControlsTimelineProperties();
            //this.UpdateTimeline(properties);
            //this.TransportControls.UpdateTimelineProperties(properties);
        }

        protected virtual Task UpdateMusicProperties()
        {
            var updater = this.TransportControls.DisplayUpdater;
            var playlistItem = this.PlaylistManager.CurrentItem;
            updater.ClearAll();
            updater.Type = MediaPlaybackType.Music;
            if (playlistItem != null)
            {
                var metaData = playlistItem.MetaDatas.ToDictionary(metaDataItem => metaDataItem.Name, metaDataItem => metaDataItem.Value, StringComparer.OrdinalIgnoreCase);
                updater.MusicProperties.Title = metaData.GetValueOrDefault(CommonMetaData.Title) ?? string.Empty;
                updater.MusicProperties.Artist = metaData.GetValueOrDefault(CommonMetaData.Performer) ?? string.Empty;
                updater.MusicProperties.AlbumArtist = metaData.GetValueOrDefault(CommonMetaData.Artist) ?? string.Empty;
                updater.MusicProperties.AlbumTitle = metaData.GetValueOrDefault(CommonMetaData.Album) ?? string.Empty;
                updater.MusicProperties.TrackNumber = Convert.ToUInt32(metaData.GetValueOrDefault(CommonMetaData.Track));
                updater.MusicProperties.AlbumTrackCount = Convert.ToUInt32(metaData.GetValueOrDefault(CommonMetaData.TrackCount));
                var genre = metaData.GetValueOrDefault(CommonMetaData.Genre);
                if (!string.IsNullOrEmpty(genre))
                {
                    updater.MusicProperties.Genres.Add(genre);
                }
            }
            updater.Update();
            return Task.CompletedTask;
        }

        protected virtual async Task UpdateThumbnail()
        {
            var updater = this.TransportControls.DisplayUpdater;
            var playlistItem = this.PlaylistManager.CurrentItem;
            if (updater.Thumbnail != null)
            {
                var disposable = updater.Thumbnail.OpenReadAsync() as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            if (playlistItem != null)
            {
                //TODO: Bad .Result
                var metaDataItem = await this.ArtworkProvider.Find(playlistItem, ArtworkType.FrontCover).ConfigureAwait(false);
                if (metaDataItem != null && File.Exists(metaDataItem.Value))
                {
                    var stream = await this.GetThumbnail(metaDataItem.Value).ConfigureAwait(false);
                    updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(stream);
                }
            }
            updater.Update();
        }

        protected virtual async Task<IRandomAccessStream> GetThumbnail(string fileName)
        {
            //TODO: For some reason we can't just return FileRandomAccessStream.OpenAsync(metaDataItem.Value, FileAccessMode.Read);
            using (var fileStream = File.OpenRead(fileName))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                    var result = new InMemoryRandomAccessStream();
#pragma warning disable ConfigureAwaitEnforcer
                    await result.WriteAsync(memoryStream.ToArray().AsBuffer());
#pragma warning restore ConfigureAwaitEnforcer
                    return result;
                }
            }
        }

        //protected virtual void UpdateTimeline(SystemMediaTransportControlsTimelineProperties properties)
        //{
        //    var outputStream = this.PlaybackManager.CurrentStream;
        //    if (outputStream == null)
        //    {
        //        properties.StartTime = TimeSpan.FromMilliseconds(0);
        //        properties.EndTime = TimeSpan.FromMilliseconds(0);
        //        properties.MinSeekTime = TimeSpan.FromSeconds(0);
        //        properties.MaxSeekTime = TimeSpan.FromSeconds(0);
        //        properties.Position = TimeSpan.FromMilliseconds(0);

        //        this.TransportControls.IsFastForwardEnabled = false;
        //        this.TransportControls.IsRewindEnabled = false;
        //    }
        //    else
        //    {
        //        properties.StartTime = TimeSpan.FromMilliseconds(0);
        //        properties.EndTime = TimeSpan.FromMilliseconds(outputStream.Length);
        //        properties.MinSeekTime = TimeSpan.FromMilliseconds(0);
        //        properties.MaxSeekTime = TimeSpan.FromMilliseconds(outputStream.Length);
        //        properties.Position = TimeSpan.FromMilliseconds(outputStream.Position);

        //        this.TransportControls.IsFastForwardEnabled = true;
        //        this.TransportControls.IsRewindEnabled = true;
        //    }
        //}

        protected virtual void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            var task = default(Task);
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    task = this.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    task = this.Pause();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    task = this.Next();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    task = this.Previous();
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    task = this.Stop();
                    break;
            }
            this.RefreshState();
            Logger.Write(this, LogLevel.Debug, "Handled button press: {0}", Enum.GetName(typeof(SystemMediaTransportControlsButton), args.Button));
        }

        //protected virtual void OnAutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        //{
        //    //Nothing to do.
        //}

        //protected virtual void OnPlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        //{
        //    //Nothing to do.
        //}

        //protected virtual void OnPlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        //{
        //    //Nothing to do.
        //}

        //protected virtual void OnShuffleEnabledChangeRequested(SystemMediaTransportControls sender, ShuffleEnabledChangeRequestedEventArgs args)
        //{
        //    //Nothing to do.
        //}

        //protected virtual void OnPropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        //{
        //    //Nothing to do.
        //}

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            this.Refresh();
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            this.RefreshState();
            this.RefreshPosition();
        }

        protected virtual Task Play()
        {
            if (this.PlaybackManager.CurrentStream == null)
            {
                return this.PlaylistManager.Next();
            }
            else if (this.PlaybackManager.CurrentStream.IsPaused)
            {
                return this.PlaybackManager.CurrentStream.Resume();
            }
            else if (this.PlaybackManager.CurrentStream.IsStopped)
            {
                return this.PlaybackManager.CurrentStream.Play();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Pause()
        {
            if (this.PlaybackManager.CurrentStream != null)
            {
                if (this.PlaybackManager.CurrentStream.IsPaused)
                {
                    return this.PlaybackManager.CurrentStream.Resume();
                }
                else if (this.PlaybackManager.CurrentStream.IsPlaying)
                {
                    return this.PlaybackManager.CurrentStream.Pause();
                }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Next()
        {
            return this.PlaylistManager.Next();
        }

        protected virtual Task Previous()
        {
            return this.PlaylistManager.Previous();
        }

        protected virtual Task Stop()
        {
            return this.PlaybackManager.Stop();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return SystemMediaTransportControlsBehaviourConfiguration.GetConfigurationSections();
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

        ~SystemMediaTransportControlsBehaviour()
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
