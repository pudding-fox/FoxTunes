using FoxTunes.Interfaces;
using MD.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MinidiscBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IBackgroundTaskSource
    {
        public const string VIEW_DISC = "AAAA";

        public const string ERASE_DISC = "BBBB";

        public const string SEND_TO_DISC = "CCCC";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IReportEmitter ReportEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.ReportEmitter = core.Components.ReportEmitter;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MinidiscBehaviourConfiguration.SECTION,
                MinidiscBehaviourConfiguration.ENABLED
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, VIEW_DISC, Strings.MinidiscBehaviour_ViewDisc, path: Strings.MinidiscBehaviour_Path);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, ERASE_DISC, Strings.MinidiscBehaviour_EraseDisc, path: Strings.MinidiscBehaviour_Path);
                    if (this.PlaylistManager.SelectedItems != null && this.PlaylistManager.SelectedItems.Any())
                    {
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SEND_TO_DISC, Strings.MinidiscBehaviour_SendToDisc, path: Strings.MinidiscBehaviour_Path);
                    }
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case VIEW_DISC:
                    return this.ViewDisc();
                case ERASE_DISC:
                    return this.EraseDisc();
                case SEND_TO_DISC:
                    return this.SendToDisc();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public async Task ViewDisc()
        {
            var task = await this.OpenDisc().ConfigureAwait(false);
            if (!task.Success)
            {
                return;
            }
            this.OnReport(task.Device, task.Disc);
        }

        public Task EraseDisc()
        {
            throw new NotImplementedException();
        }

        public async Task SendToDisc()
        {
            if (this.PlaylistManager.SelectedItems == null)
            {
                return;
            }
            var playlistItems = this.PlaylistManager.SelectedItems.ToArray();
            if (!playlistItems.Any())
            {
                return;
            }
            var task = await this.OpenDisc().ConfigureAwait(false);
            if (!task.Success)
            {
                return;
            }
            this.OnReport(task.Device, task.Disc);
        }

        protected virtual async Task<OpenMinidiscTask> OpenDisc()
        {
            using (var task = new OpenMinidiscTask())
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                return task;
            }
        }

        protected virtual void OnReport(IDevice device, IDisc disc)
        {
            var report = new MinidiscReport(device, disc);
            report.InitializeComponent(this.Core);
            this.ReportEmitter.Send(report);
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

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return MinidiscBehaviourConfiguration.GetConfigurationSections();
        }

        public class OpenMinidiscTask : BackgroundTask
        {
            public const string ID = "7AE3FD56-9ADA-42F8-8A47-FE8BF5CDA54A";

            public OpenMinidiscTask() : base(ID)
            {

            }

            public override bool Visible
            {
                get
                {
                    return true;
                }
            }

            public IDevice Device { get; private set; }

            public IDisc Disc { get; private set; }

            public bool Success { get; private set; }

            protected override Task OnStarted()
            {
                this.Name = Strings.OpenMinidiscTask_Name;
                return base.OnStarted();
            }

            protected override Task OnRun()
            {
                var toolManager = new ToolManager();
                var deviceManager = new DeviceManager(toolManager);
                var devices = deviceManager.GetDevices();
                this.Device = devices.FirstOrDefault();
                if (this.Device != null)
                {
                    var discManager = new DiscManager(toolManager);
                    this.Disc = discManager.GetDisc(this.Device);
                    if (this.Disc != null)
                    {
                        this.Success = true;
                    }
                }
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
        }
    }
}
