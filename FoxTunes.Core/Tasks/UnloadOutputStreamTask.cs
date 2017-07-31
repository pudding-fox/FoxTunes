using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class UnloadOutputStreamTask : BackgroundTask
    {
        public const string ID = "E6D893E8-EC80-4823-A66C-286DB69A48D1";

        public UnloadOutputStreamTask()
            : base(ID, false)
        {

        }

        public IOutput Output { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.Output.Unload(this.PlaybackManager.CurrentStream);
            this.ForegroundTaskRunner.Run(() => this.PlaybackManager.CurrentStream = null);
        }
    }
}
