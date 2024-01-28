using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths, bool clear)
            : base(sequence)
        {
            this.Paths = paths;
            this.Clear = clear;
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

        public IEnumerable<string> Paths { get; private set; }

        public bool Clear { get; private set; }

        protected override Task OnStarted()
        {
            this.Name = "Getting file list";
            this.IsIndeterminate = true;
            return base.OnStarted();
        }

        protected override async Task OnRun()
        {
            if (this.Clear)
            {
                await this.RemoveItems(PlaylistItemStatus.None);
            }
            await this.AddPaths(this.Paths);

        }

        protected override async Task OnCompleted()
        {
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
            await base.OnCompleted();
        }
    }
}
