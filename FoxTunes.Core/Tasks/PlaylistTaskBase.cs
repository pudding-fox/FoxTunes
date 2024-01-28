using FoxTunes.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class PlaylistTaskBase : BackgroundTask
    {
        protected PlaylistTaskBase(string id) : base(id)
        {
        }

        public IDataManager DataManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.DataManager = core.Managers.Data;
            base.InitializeComponent(core);
        }

        protected virtual void ShiftItems(IDatabaseContext context, int sequence, int offset)
        {
            Logger.Write(this, LogLevel.Debug, "Shifting playlist items from {0}", sequence);
            var query =
                from playlistItem in context.Queries.PlaylistItem
                where playlistItem.Sequence >= sequence
                orderby playlistItem.Sequence
                select playlistItem;
            foreach (var playlistItem in query)
            {
                Logger.Write(this, LogLevel.Debug, "Shifting playlist item: {0} => {1} => {2} => {3}", playlistItem.Id, playlistItem.FileName, playlistItem.Sequence, playlistItem.Sequence + offset);
                playlistItem.Sequence = playlistItem.Sequence + offset;
                context.Sets.PlaylistItem.Update(playlistItem);
                this.ForegroundTaskRunner.Run(() =>
                {
                    this.DataManager.ReadContext.Sets.PlaylistItem.SetCurrentValues(playlistItem, playlistItem.Id);
                });
            }
        }

        protected virtual Task SaveChanges(IDatabaseContext context)
        {
            this.Name = "Saving changes";
            this.IsIndeterminate = true;
            Logger.Write(this, LogLevel.Debug, "Saving changes to playlist.");
            return context.SaveChangesAsync();
        }
    }
}
