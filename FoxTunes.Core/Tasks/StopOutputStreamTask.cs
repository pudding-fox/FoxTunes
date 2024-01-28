using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class StopOutputStreamTask : BackgroundTask
    {
        public const string ID = "0115D319-74AE-4A41-A096-30F54E6FBDDB";

        public StopOutputStreamTask()
            : base(ID, false)
        {

        }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.PlaybackManager.CurrentStream.Stop();
        }
    }
}
