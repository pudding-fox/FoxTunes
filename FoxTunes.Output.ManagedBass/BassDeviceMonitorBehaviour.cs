using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BassDeviceMonitorBehaviour : StandardBehaviour
    {
        public IBassOutput Output { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public NotificationClient NotificationClient { get; private set; }

        new public bool IsInitialized { get; private set; }

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                Logger.Write(this, LogLevel.Debug, "Enabled = {0}", this.Enabled);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                return;
            }
            this.NotificationClient = new NotificationClient();
            this.NotificationClient.DeviceAdded += this.OnDeviceAdded;
            this.NotificationClient.DeviceRemoved += this.OnDeviceRemoved;
            this.NotificationClient.DeviceStateChanged += this.OnDeviceStateChanged;
            this.NotificationClient.DefaultDeviceChanged += this.OnDefaultDeviceChanged;
            this.NotificationClient.PropertyValueChanged += this.OnPropertyValueChanged;
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS Device Monitor Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            this.NotificationClient.Dispose();
            this.NotificationClient = null;
            this.IsInitialized = false;
        }

        protected virtual void OnDeviceAdded(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDeviceRemoved(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDeviceStateChanged(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDefaultDeviceChanged(object sender, NotificationClientEventArgs e)
        {
            if (!this.RestartRequired(e.Flow, e.Role))
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "The default playback device was changed: {0} => {1} => {2}", e.Flow.Value, e.Role.Value, e.Device);
            Logger.Write(this, LogLevel.Debug, "Restarting the output.");
            //TODO: Bad awaited Task.
            this.BackgroundTaskRunner.Run(() => this.Restart());
        }

        protected virtual void OnPropertyValueChanged(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!this.Output.IsStarted)
            {
                return false;
            }
            if (flow == null || flow.Value != DataFlow.Render)
            {
                return false;
            }
            if (role == null || role.Value != Role.Multimedia)
            {
                return false;
            }
            return true;
        }

        public async Task Restart()
        {
            if (!this.Output.IsStarted)
            {
                return;
            }
            var position = default(long);
            var playlistItem = default(PlaylistItem);
            if (this.PlaybackManager.CurrentStream != null)
            {
                position = this.PlaybackManager.CurrentStream.Position;
            }
            if (this.PlaylistManager.CurrentItem != null)
            {
                playlistItem = this.PlaylistManager.CurrentItem;
            }
            try
            {
                await this.Output.Start();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to start output: {0}", e.Message);
            }
            if (playlistItem != null)
            {
                await this.ForegroundTaskRunner.Run(async () =>
                {
                    await this.PlaylistManager.Play(playlistItem);
                    if (this.PlaybackManager.CurrentStream != null && position > 0)
                    {
                        this.PlaybackManager.CurrentStream.Position = position;
                    }
                });
            }
        }
    }
}
