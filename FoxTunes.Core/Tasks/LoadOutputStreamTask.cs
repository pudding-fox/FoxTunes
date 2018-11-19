using FoxTunes.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class LoadOutputStreamTask : BackgroundTask
    {
        public const string ID = "E3E23677-DE0A-4291-8416-BC4A91856037";

        public LoadOutputStreamTask(PlaylistItem playlistItem, bool immediate)
            : base(ID)
        {
            this.PlaylistItem = playlistItem;
            this.Immediate = immediate;
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public bool Immediate { get; private set; }

        public IOutput Output { get; private set; }

        public IOutputStreamQueue OutputStreamQueue { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override bool Visible
        {
            get
            {
                if (this.Output != null && this.Output.ShowBuffering && this.Immediate)
                {
                    return true;
                }
                return base.Visible;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output;
            this.OutputStreamQueue = core.Components.OutputStreamQueue;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected override Task OnStarted()
        {
            this.Name = "Buffering";
            this.Description = this.PlaylistItem.FileName.GetName();
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            Logger.Write(this, LogLevel.Debug, "Loading play list item into output stream: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
            if (this.OutputStreamQueue.IsQueued(this.PlaylistItem))
            {
                Logger.Write(this, LogLevel.Debug, "Play list item already exists in the queue:  {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
                if (this.Immediate)
                {
                    Logger.Write(this, LogLevel.Debug, "Immediate load was requested, de-queuing: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
                    await this.OutputStreamQueue.Dequeue(this.PlaylistItem);
                }
                return;
            }
            var outputStream = await this.Output.Load(this.PlaylistItem, this.Immediate);
            if (outputStream == null)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load play list item into output stream: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Play list item loaded into output stream: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
            await this.OutputStreamQueue.Enqueue(outputStream, this.Immediate);
            if (this.Immediate)
            {
                Logger.Write(this, LogLevel.Debug, "Immediate load was requested, output stream was automatically de-queued: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Output stream added to the queue: {0} => {1}", this.PlaylistItem.Id, this.PlaylistItem.FileName);
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.StreamLoaded));
        }
    }
}
