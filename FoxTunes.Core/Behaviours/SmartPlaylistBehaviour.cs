using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class SmartPlaylistBehaviour : DynamicPlaylistBehaviour
    {
        public override Func<Playlist, bool> Predicate
        {
            get
            {
                return playlist => playlist.Type == PlaylistType.Smart && playlist.Enabled;
            }
        }

        protected virtual string GetFilter(Playlist playlist)
        {
            var minRating = 4;
            var maxLastPlayed = DateTimeHelper.ToString(DateTime.Now.AddDays(-30).Date);
            return string.Format("rating>:{0} lastplayed<\"{1}\"", minRating, maxLastPlayed);
        }

        protected override async Task Refresh(IEnumerable<string> names)
        {
            foreach (var playlist in this.GetPlaylists())
            {
                if (names != null && names.Any())
                {
                    if (!this.FilterParser.AppliesTo(this.GetFilter(playlist), names))
                    {
                        return;
                    }
                }
                await this.Refresh(playlist).ConfigureAwait(false);
            }
        }

        public override Task Refresh(Playlist playlist)
        {
            return this.Refresh(playlist, this.GetFilter(playlist));
        }
    }
}
