using FoxTunes.Interfaces;
using System.IO;

namespace FoxTunes
{
    public class LoadOutputStreamTask : BackgroundTask
    {
        public const string ID = "E3E23677-DE0A-4291-8416-BC4A91856037";

        public LoadOutputStreamTask(string fileName)
            : base(ID)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }

        public IOutput Output { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IOutputStream OutputStream { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override void OnRun()
        {
            this.Name = "Buffering";
            this.Description = new FileInfo(this.FileName).Name;
            this.OutputStream = this.Output.Load(this.FileName);
            this.ForegroundTaskRunner.Run(() => this.PlaybackManager.CurrentStream = this.OutputStream);
        }
    }
}
