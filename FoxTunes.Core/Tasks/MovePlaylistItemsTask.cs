#pragma warning disable 612, 618
using FoxDb;
using FoxDb.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class MovePlaylistItemsTask : PlaylistTaskBase
    {
        public MovePlaylistItemsTask(int sequence, IEnumerable<PlaylistItem> playlistItems)
            : base(sequence)
        {
            this.PlaylistItems = playlistItems;
            this.Offset = playlistItems.Count();
        }

        public IEnumerable<PlaylistItem> PlaylistItems { get; private set; }

        protected override async Task OnRun()
        {
            using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
            {
                var sequence = 0;
                var fetch = this.Database.QueryFactory.Build().With(query =>
                {
                    query.Output.AddColumn(this.Database.Tables.PlaylistItem.PrimaryKey);
                    query.Source.AddTable(this.Database.Tables.PlaylistItem);
                    query.Sort.AddColumn(this.Database.Tables.PlaylistItem.Column("Sequence"));
                }).Build();
                var update = this.Database.QueryFactory.Build().With(query =>
                {
                    query.Update.SetTable(this.Database.Tables.PlaylistItem);
                    query.Update.AddColumn(this.Database.Tables.PlaylistItem.Column("Sequence"));
                    query.Filter.AddColumn(this.Database.Tables.PlaylistItem.Column("Id"));
                }).Build();
                using (var reader = this.Database.ExecuteReader(fetch, null, transaction))
                {
                    foreach (var record in reader)
                    {
                        var id = record.Get<int>(this.Database.Tables.PlaylistItem.PrimaryKey);
                        if (this.PlaylistItems.Any(playlistItem => playlistItem.Id == id))
                        {
                            await this.Database.ExecuteAsync(update, (parameters, phase) =>
                            {
                                switch (phase)
                                {
                                    case DatabaseParameterPhase.Fetch:
                                        parameters["id"] = id;
                                        parameters["sequence"] = this.Sequence++;
                                        break;
                                }
                            }, transaction);
                        }
                        else
                        {
                            if (sequence == this.Sequence)
                            {
                                sequence += this.Offset;
                            }
                            await this.Database.ExecuteAsync(update, (parameters, phase) =>
                            {
                                switch (phase)
                                {
                                    case DatabaseParameterPhase.Fetch:
                                        parameters["id"] = id;
                                        parameters["sequence"] = sequence++;
                                        break;
                                }
                            }, transaction);
                        }
                    }
                }
                transaction.Commit();
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated));
        }
    }
}
