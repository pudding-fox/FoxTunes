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

        public const string SET_DISC_TITLE = "BBBB";

        public const string ERASE_DISC = "CCCC";

        public const string SEND_TO_DISC_SP = "DDDD";

        public const string SEND_TO_DISC_LP2 = "EEEE";

        public const string SEND_TO_DISC_LP4 = "FFFF";

        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), Publication.Product, typeof(MinidiscBehaviour).Name);

        public ICore Core { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IReportEmitter ReportEmitter { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public TextConfigurationElement DiscTitleScript { get; private set; }

        public TextConfigurationElement TrackNameScript { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.PlaylistManager = core.Managers.Playlist;
            this.ReportEmitter = core.Components.ReportEmitter;
            this.UserInterface = core.Components.UserInterface;
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                MinidiscBehaviourConfiguration.SECTION,
                MinidiscBehaviourConfiguration.ENABLED
            );
            this.DiscTitleScript = this.Configuration.GetElement<TextConfigurationElement>(
                MinidiscBehaviourConfiguration.SECTION,
                MinidiscBehaviourConfiguration.DISC_TITLE_SCRIPT
            );
            this.TrackNameScript = this.Configuration.GetElement<TextConfigurationElement>(
                MinidiscBehaviourConfiguration.SECTION,
                MinidiscBehaviourConfiguration.TRACK_NAME_SCRIPT
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
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, SET_DISC_TITLE, Strings.MinidiscBehaviour_SetDiscTitle, path: Strings.MinidiscBehaviour_Path);
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_SETTINGS, ERASE_DISC, Strings.MinidiscBehaviour_EraseDisc, path: Strings.MinidiscBehaviour_Path, attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
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
                case SET_DISC_TITLE:
                    return this.SetDiscTitle();
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

        public async Task<bool> SetDiscTitle()
        {
            var task = await this.OpenDisc().ConfigureAwait(false);
            if (task.Disc == null)
            {
                return false;
            }
            var title = this.UserInterface.Prompt(Strings.MinidiscBehaviour_SetDiscTitle, task.Disc.Title);
            if (string.IsNullOrEmpty(title))
            {
                return false;
            }
            return await this.SetDiscTitle(title).ConfigureAwait(false);
        }

        public async Task<bool> SetDiscTitle(string title)
        {
            using (var task = new SetDiscTitleTask(title))
            {
                task.InitializeComponent(this.Core);
                this.OnBackgroundTask(task);
                await task.Run().ConfigureAwait(false);
                return task.Result != null && task.Result.Status == ResultStatus.Success;
            }
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
            using (var scriptingContext = this.ScriptingRuntime.CreateContext())
            {
                if (this.IsUntitled(updatedDisc))
                {
                    updatedDisc.Title = this.GetTitle(scriptingContext, playlistItems);
                }
                foreach (var pair in fileNames)
                {
                    var track = updatedDisc.Tracks.Add(pair.Value, compression);
                    track.Name = this.GetName(scriptingContext, pair.Key);
                }
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
                new BassEncoderOutputPath.Fixed(TempPath),
                Wav_16_44100_Settings.NAME,
                false
            ).ConfigureAwait(false);
            foreach (var encoderItem in encoderItems)
            {
                if (encoderItem.Status != EncoderItemStatus.Complete && !EncoderItem.WasSkipped(encoderItem))
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
                    Logger.Write(this, LogLevel.Warn, "Failed to determine input file for encoder item: {0}", encoderItem.InputFileName);
                    continue;
                }
                fileNames.Add(fileData, encoderItem.OutputFileName);
            }
            return fileNames;
        }

        public bool IsUntitled(IDisc disc)
        {
            const string UNTITLED = "<Untitled>";
            return string.IsNullOrEmpty(disc.Title) || string.Equals(disc.Title, UNTITLED, StringComparison.OrdinalIgnoreCase);
        }

        public string GetTitle(IScriptingContext scriptingContext, IEnumerable<IFileData> fileDatas)
        {
            var title = default(string);
            foreach (var playlistItem in fileDatas.OfType<PlaylistItem>())
            {
                var runner = new PlaylistItemScriptRunner(scriptingContext, playlistItem, this.DiscTitleScript.Value);
                runner.Prepare();
                var temp = Convert.ToString(runner.Run());
                if (string.IsNullOrEmpty(title) || string.Equals(temp, title, StringComparison.OrdinalIgnoreCase))
                {
                    title = temp;
                }
                else
                {
                    const string UNTITLED = "<Untitled>";
                    Logger.Write(this, LogLevel.Warn, "Disc title is ambiguous, falling back to : {0}", UNTITLED);
                    return UNTITLED;
                }
            }
            return title;
        }

        public string GetName(IScriptingContext scriptingContext, IFileData fileData)
        {
            if (fileData is PlaylistItem playlistItem)
            {
                var runner = new PlaylistItemScriptRunner(scriptingContext, playlistItem, this.TrackNameScript.Value);
                runner.Prepare();
                return Convert.ToString(runner.Run());
            }
            throw new NotImplementedException();
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

        public static void Cleanup()
        {
            try
            {
                Directory.Delete(TempPath, true);
            }
            catch (Exception e)
            {
                Logger.Write(typeof(MinidiscBehaviour), LogLevel.Warn, "Failed to cleanup temp files: {0}", e.Message);
            }
        }
    }
}
