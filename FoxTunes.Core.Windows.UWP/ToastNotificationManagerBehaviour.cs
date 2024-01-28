using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ToastNotificationManagerBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const string GROUP = "87854E64-DD46-4C8C-8E29-B97FBF5265BD";

        const string TAG = "C4A36DDF-A8AC-49B6-AF8F-0A8A9F1541D0";

        public ToastNotifier ToastNotifier { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public bool Enabled
        {
            get
            {
                return this.ToastNotifier != null;
            }
        }

        public bool Popup { get; private set; }

        public bool LargeArtwork { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            if (ToastNotificationManagerBehaviourConfiguration.IsPlatformSupported)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaybackManager = core.Managers.Playback;
                this.ArtworkProvider = core.Components.ArtworkProvider;
                this.Configuration = core.Components.Configuration;
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    ToastNotificationManagerBehaviourConfiguration.SECTION,
                    ToastNotificationManagerBehaviourConfiguration.ENABLED_ELEMENT
                ).ConnectValue(value =>
                {
                    if (value)
                    {
                        this.Enable();
                    }
                    else
                    {
                        this.Disable();
                        ToastNotificationHelper.Uninstall(false);
                    }
                });
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    ToastNotificationManagerBehaviourConfiguration.SECTION,
                    ToastNotificationManagerBehaviourConfiguration.POPUP_ELEMENT
                ).ConnectValue(value => this.Popup = value);
                this.Configuration.GetElement<BooleanConfigurationElement>(
                    ToastNotificationManagerBehaviourConfiguration.SECTION,
                    ToastNotificationManagerBehaviourConfiguration.LARGE_ARTWORK_ELEMENT
                ).ConnectValue(value => this.LargeArtwork = value);
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
            ToastNotificationHelper.Install();
            ToastNotificationHelper.Invoke(new Action(
                () => this.ToastNotifier = ToastNotificationManager.CreateToastNotifier(ToastNotificationHelper.ID)
            ), null);
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
        }

        public void Disable()
        {
            if (!this.Enabled)
            {
                return;
            }
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            ToastNotificationHelper.NotificationActivator.Disable();
            try
            {
                ToastNotificationManager.History.Remove(TAG, GROUP, ToastNotificationHelper.ID);
            }
            catch
            {
                //Nothing can be done.
            }
            this.ToastNotifier = null;
        }

        protected virtual void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream == null)
            {
                return;
            }
            var notification = this.CreateNotification(outputStream);
            ToastNotificationHelper.Invoke(new Action(() => this.ToastNotifier.Show(notification)), null);
        }

        protected virtual ToastNotification CreateNotification(IOutputStream outputStream)
        {
            var document = this.CreateNotificationDocument(outputStream);
            var notification = new ToastNotification(document);
            notification.Group = GROUP;
            notification.Tag = TAG;
            notification.SuppressPopup = !this.Popup;
            return notification;
        }

        protected virtual XmlDocument CreateNotificationDocument(IOutputStream outputStream)
        {
            var metaData = outputStream.PlaylistItem.MetaDatas.ToDictionary(
                metaDataItem => metaDataItem.Name,
                metaDataItem => metaDataItem.Value,
                StringComparer.OrdinalIgnoreCase
            );
            var fileName = ArtworkProvider.Find(
                outputStream.PlaylistItem,
                ArtworkType.FrontCover
            );
            var hasArtwork = !string.IsNullOrEmpty(fileName) && File.Exists(fileName);
            var xml = default(string);
            if (hasArtwork)
            {
                if (this.LargeArtwork)
                {
                    xml = this.CreateNotificationXml_3(metaData, fileName);
                }
                else
                {
                    xml = this.CreateNotificationXml_2(metaData, fileName);
                }
            }
            else
            {
                xml = this.CreateNotificationXml_1(metaData);
            }
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }

        protected virtual string CreateNotificationXml_1(IDictionary<string, string> metaData)
        {
            var title = metaData[CommonMetaData.Title];
            var album = metaData[CommonMetaData.Album];
            var artist = metaData[CommonMetaData.Artist];
            return @"
<toast>
    <visual>
        <binding template=""ToastGeneric"">" +
(!string.IsNullOrEmpty(title) ? "<text>" + title + "</text>" : "<text>No Title</text>") +
(!string.IsNullOrEmpty(album) ? "<text>" + album + "</text>" : string.Empty) +
(!string.IsNullOrEmpty(artist) ? "<text>" + artist + "</text>" : string.Empty) + @"
        </binding>
    </visual>
</toast>
";
        }

        protected virtual string CreateNotificationXml_2(IDictionary<string, string> metaData, string fileName)
        {
            var title = metaData[CommonMetaData.Title];
            var album = metaData[CommonMetaData.Album];
            var artist = metaData[CommonMetaData.Artist];
            return @"
<toast>
    <visual>
        <binding template=""ToastGeneric"">" +
(!string.IsNullOrEmpty(title) ? "<text>" + title + "</text>" : "<text>No Title</text>") +
(!string.IsNullOrEmpty(album) ? "<text>" + album + "</text>" : string.Empty) +
(!string.IsNullOrEmpty(artist) ? "<text>" + artist + "</text>" : string.Empty) + @"
            <image
                placement=""appLogoOverride""
                hint-crop=""circle""
                src = """ + new Uri(fileName).AbsoluteUri + @""" />
        </binding>
    </visual>
</toast>
";
        }

        protected virtual string CreateNotificationXml_3(IDictionary<string, string> metaData, string fileName)
        {
            var title = metaData[CommonMetaData.Title];
            var album = metaData[CommonMetaData.Album];
            var artist = metaData[CommonMetaData.Artist];
            return @"
<toast>
    <visual>
        <binding template=""ToastGeneric"">" +
            (!string.IsNullOrEmpty(title) ? "<text>" + title + "</text>" : "<text>No Title</text>") +
            (!string.IsNullOrEmpty(album) ? "<text>" + album + "</text>" : string.Empty) +
            (!string.IsNullOrEmpty(artist) ? "<text>" + artist + "</text>" : string.Empty) + @"
            <image
                placement=""inline""
                src = """ + new Uri(fileName).AbsoluteUri + @""" />
        </binding>
    </visual>
</toast>
";
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ToastNotificationManagerBehaviourConfiguration.GetConfigurationSections();
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

        ~ToastNotificationManagerBehaviour()
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
