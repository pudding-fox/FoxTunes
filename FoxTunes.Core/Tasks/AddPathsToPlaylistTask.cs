using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddPathsToPlaylistTask : PlaylistTaskBase
    {
        public const string ID = "7B564369-A6A0-4BAF-8C99-08AF27908591";

        public AddPathsToPlaylistTask(int sequence, IEnumerable<string> paths, bool clear)
            : base(ID, sequence)
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
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                if (this.Clear)
                {
                    await this.RemoveItems(PlaylistItemStatus.None, transaction);
                }
                await this.AddPaths(this.Paths, transaction);
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
