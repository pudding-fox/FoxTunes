using FoxTunes.Interfaces;
using MD.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MinidiscBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent, IBackgroundTaskSource
    {
        public const string VIEW_DISC = "AAAA";

        public const string ERASE_DISC = "BBBB";

        public const string SEND_TO_DISC_SP = "CCCC";

        public const string SEND_TO_DISC_LP2 = "DDDD";

        public const string SEND_TO_DISC_LP4 = "EEEE";

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IReportEmitter ReportEmitter { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.ReportEmitter = core.Components.ReportEmitter;
            this.UserInterface = core.Components.UserInterface;
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
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SEND_TO_DISC_SP, string.Format(Strings.MinidiscBehaviour_SendToDisc, "SP"), path: Strings.MinidiscBehaviour_Path);
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SEND_TO_DISC_LP2, string.Format(Strings.MinidiscBehaviour_SendToDisc, "LP2"), path: Strings.MinidiscBehaviour_Path);
                        yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYLIST, SEND_TO_DISC_LP4, string.Format(Strings.MinidiscBehaviour_SendToDisc, "LP4"), path: Strings.MinidiscBehaviour_Path);
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
                case SEND_TO_DISC_SP:
                    return this.SendToDisc(Compression.None);
                case SEND_TO_DISC_LP2:
                    return this.SendToDisc(Compression.LP2);
                case SEND_TO_DISC_LP4:
                    return this.SendToDisc(Compression.LP4);
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
            if (task.Disc == null)
            {
                return;
            }
            var actions = new Actions(task.Device, task.Disc, task.Disc, Actions.None);
            this.ConfirmActions(task.Device, actions);
        }

        public async Task<bool> EraseDisc()
        {
            if (!this.UserInterface.Confirm(Strings.MinidiscBehaviour_ConfirmEraseDisc))
            {
                return false;
            }
            using (var task = new EraseMinidiscTask())
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                return task.Result != null && task.Result.Status == ResultStatus.Success;
            }
        }

        public async Task<bool> WriteDisc(IDevice device, IActions actions)
        {
            if (!this.UserInterface.Confirm(Strings.MinidiscBehaviour_ConfirmWriteDisc))
            {
                return false;
            }
            using (var task = new WriteMinidiscTask(device, actions))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                return task.Result != null && task.Result.Status == ResultStatus.Success;
            }
        }

        public async Task SendToDisc(Compression compression)
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
            var fileNames = await this.GetWaveFiles(playlistItems).ConfigureAwait(false);
            if (fileNames == null || fileNames.Count == 0)
            {
                return;
            }
            var task = await this.OpenDisc().ConfigureAwait(false);
            if (task.Disc == null)
            {
                return;
            }
            var device = task.Device;
            var currentDisc = task.Disc;
            var updatedDisc = currentDisc.Clone();
            if (this.IsUntitled(updatedDisc))
            {
                updatedDisc.Title = this.GetTitle(playlistItems);
            }
            foreach (var pair in fileNames)
            {
                var track = updatedDisc.Tracks.Add(pair.Value, compression);
                track.Name = this.GetName(pair.Key);
            }
            var toolManager = new ToolManager();
            var formatManager = new FormatManager(toolManager);
            var actionBuilder = new ActionBuilder(formatManager);
            var actions = actionBuilder.GetActions(device, currentDisc, updatedDisc);
            this.ConfirmActions(task.Device, actions);
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

        protected virtual void ConfirmActions(IDevice device, IActions actions)
        {
            var report = new MinidiscReport(this, device, actions);
            report.InitializeComponent(this.Core);
            this.ReportEmitter.Send(report);
        }

        public async Task<IDictionary<IFileData, string>> GetWaveFiles(IFileData[] fileDatas)
        {
            var fileNames = new Dictionary<IFileData, string>();
            var behaviour = ComponentRegistry.Instance.GetComponent<BassEncoderBehaviour>();
            var encoderItems = await behaviour.Encode(
                fileDatas,
                new BassEncoderOutputPath.Fixed(Path.Combine(Path.GetTempPath(), string.Format("MD-{0}", DateTime.UtcNow.ToFileTimeUtc()))),
                Wav_16_44100_Settings.NAME,
                false
            ).ConfigureAwait(false);
            foreach (var encoderItem in encoderItems)
            {
                if (encoderItem.Status != EncoderItemStatus.Complete)
                {
                    //If something went wrong then present the conversion log.
                    var report = new BassEncoderReport(encoderItems);
                    report.InitializeComponent(this.Core);
                    await this.ReportEmitter.Send(report).ConfigureAwait(false);
                    return null;
                }
                var fileData = fileDatas.FirstOrDefault(_fileData => string.Equals(_fileData.FileName, encoderItem.InputFileName, StringComparison.OrdinalIgnoreCase));
                if (fileData == null)
                {
                    //TODO: Warn.
                    continue;
                }
                fileNames.Add(fileData, encoderItem.OutputFileName);
            }
            return fileNames;
        }

        public bool IsUntitled(IDisc disc)
        {
            const string UNTITLED = "<Untitled>";
            return string.IsNullOrEmpty(disc.Title) || string.Equals(disc.Title, UNTITLED, StringComparison.OrdinalIgnoreCase)
        }

        public string GetTitle(IEnumerable<IFileData> fileDatas)
        {

        }

        public string GetName(IFileData fileData)
        {

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
    }
}
