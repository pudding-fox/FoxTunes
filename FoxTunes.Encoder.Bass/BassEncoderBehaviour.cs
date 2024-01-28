using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoderBehaviour : StandardBehaviour, IBackgroundTaskSource, IInvocableComponent, IConfigurableComponent
    {
        public const string ENCODE = "GGGG";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Destination { get; private set; }

        public TextConfigurationElement Location { get; private set; }

        public BooleanConfigurationElement CopyTags { get; private set; }

        public IntegerConfigurationElement Threads { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.Configuration = core.Components.Configuration;
            this.Destination = this.Configuration.GetElement<SelectionConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_ELEMENT
            );
            this.Location = this.Configuration.GetElement<TextConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_LOCATION_ELEMENT
            );
            this.CopyTags = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.COPY_TAGS
            );
            this.Threads = this.Configuration.GetElement<IntegerConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.THREADS_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, ENCODE, "Convert");
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case ENCODE:
                    return this.Encode();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassEncoderBehaviourConfiguration.GetConfigurationSections();
        }

        public Task Encode()
        {
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Encode(playlistItems);
        }

        public async Task Encode(PlaylistItem[] playlistItems)
        {
            using (var task = new EncodePlaylistItemsTask(this, playlistItems))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        protected virtual void OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
                return;
            }
            this.BackgroundTask(this, new BackgroundTaskEventArgs(backgroundTask));
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        private class EncodePlaylistItemsTask : PlaylistTaskBase
        {
            public static readonly IBassEncoderFactory EncoderFactory = ComponentRegistry.Instance.GetComponent<IBassEncoderFactory>();

            private EncodePlaylistItemsTask()
            {
                this.CancellationToken = new CancellationToken();
            }

            public EncodePlaylistItemsTask(BassEncoderBehaviour behaviour, PlaylistItem[] playlistItems) : this()
            {
                this.Behaviour = behaviour;
                this.PlaylistItems = playlistItems;
                this.EncoderItems = playlistItems
                    .Select(playlistItem => EncoderItem.FromPlaylistItem(playlistItem))
                    .ToArray();
            }

            public override bool Visible
            {
                get
                {
                    return true;
                }
            }

            public override bool Cancellable
            {
                get
                {
                    return true;
                }
            }

            public CancellationToken CancellationToken { get; private set; }

            public BassEncoderBehaviour Behaviour { get; private set; }

            public PlaylistItem[] PlaylistItems { get; private set; }

            public EncoderItem[] EncoderItems { get; private set; }

            protected override async Task OnRun()
            {
                await this.Encode();
                if (this.Behaviour.CopyTags.Value)
                {
                    await this.CopyTags();
                }
            }

            protected virtual async Task Encode()
            {
                Logger.Write(this, LogLevel.Debug, "Creating encoder.");
                var encoder = EncoderFactory.CreateEncoder();
                try
                {
                    Logger.Write(this, LogLevel.Debug, "Starting encoder.");
                    using (var monitor = new BassEncoderMonitor(encoder, this.Visible, this.CancellationToken))
                    {
                        await this.WithSubTask(monitor,
                            async () => await monitor.Encode(this.EncoderItems)
                        );
                    }
                }
                finally
                {
                    this.Unload(encoder.Domain);
                }
                Logger.Write(this, LogLevel.Debug, "Encoder completed successfully.");
            }

            protected virtual async Task CopyTags()
            {
                Logger.Write(this, LogLevel.Debug, "Copying tags.");
                foreach (var playlistItem in this.PlaylistItems)
                {
                    var encoderItem = this.GetEncoderItem(playlistItem);
                    if (encoderItem.Status != EncoderItemStatus.Complete)
                    {
                        Logger.Write(this, LogLevel.Warn, "Not tagging file \"{0}\" status does not indicate success.", encoderItem.OutputFileName);
                        continue;
                    }
                    await this.CopyTags(playlistItem, encoderItem);
                }
                Logger.Write(this, LogLevel.Debug, "Successfully copied tags.");
            }

            protected virtual async Task CopyTags(PlaylistItem playlistItem, EncoderItem encoderItem)
            {
                Logger.Write(this, LogLevel.Debug, "Copying tags from \"{0}\" to \"{1}\".", encoderItem.InputFileName, encoderItem.OutputFileName);
                using (var task = new WriteFileMetaDataTask(encoderItem.OutputFileName, playlistItem.MetaDatas))
                {
                    task.InitializeComponent(this.Core);
                    await task.Run();
                }
            }

            protected virtual EncoderItem GetEncoderItem(PlaylistItem playlistItem)
            {
                return this.EncoderItems.FirstOrDefault(encoderItem => string.Equals(encoderItem.InputFileName, playlistItem.FileName, StringComparison.OrdinalIgnoreCase));
            }

            protected virtual void Unload(AppDomain domain)
            {
                const int ATTEMPTS = 5;
                var name = domain.FriendlyName;
                for (int a = 1; a <= ATTEMPTS; a++)
                {
                    try
                    {
                        Logger.Write(this, LogLevel.Debug, "Attempting to unload app domain \"{0}\" attempt {1} of {2}. ", name, a, ATTEMPTS);
                        AppDomain.Unload(domain);
                        Logger.Write(this, LogLevel.Debug, "Successfully unloaded app domain \"{0}\".", name);
                        return;
                    }
                    catch (CannotUnloadAppDomainException e)
                    {
                        Logger.Write(this, LogLevel.Warn, "Failed to unloaded app domain \"{0}\": {1}", name, e.Message);
                    }
                }
                Logger.Write(this, LogLevel.Error, "Failed to unloaded app domain \"{0}\".", name);
            }

            protected override void OnCancellationRequested()
            {
                this.CancellationToken.Cancel();
                base.OnCancellationRequested();
            }
        }
    }
}
