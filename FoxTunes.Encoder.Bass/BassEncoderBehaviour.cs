using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    public class BassEncoderBehaviour : StandardBehaviour, IBackgroundTaskSource, IInvocableComponent
    {
        public const string ENCODE = "GGGG";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
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

        public async Task Encode(IEnumerable<PlaylistItem> playlistItems)
        {
            var settings = this.GetSettings();
            using (var task = new EncodePlaylistItemsTask(playlistItems, settings))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run();
            }
        }

        protected virtual IBassEncoderSettings GetSettings()
        {
            return new FlacEncoderSettings();
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

            public EncodePlaylistItemsTask(IEnumerable<PlaylistItem> playlistItems, IBassEncoderSettings settings)
            {
                this.PlaylistItems = playlistItems;
                this.Settings = settings;
            }

            public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

            public IBassEncoderSettings Settings { get; private set; }

            protected override async Task OnRun()
            {
                var fileNames = this.PlaylistItems.Select(
                    playlistItem => playlistItem.FileName
                ).ToArray();
                var encoder = EncoderFactory.CreateEncoder(1);
                try
                {
                    var monitor = new BassEncoderMonitor(encoder, this.Visible);
                    await this.WithSubTask(monitor,
                        async () => await monitor.Encode(fileNames, this.Settings)
                    );
                }
                finally
                {
                    AppDomain.Unload(encoder.Domain);
                }
            }
        }
    }
}
