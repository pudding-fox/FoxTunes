using FoxDb;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace FoxTunes
{
    [Component("C0B2450C-DEDA-4D8B-8A32-5EA733F1FD45", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaybackStateBehaviour : StandardBehaviour, IUIPlaylistColumnProvider, IDatabaseInitializer
    {
        public string Id
        {
            get
            {
                return typeof(PlaybackStateBehaviour).AssemblyQualifiedName;
            }
        }

        public string Name
        {
            get
            {
                return "Playback State";
            }
        }

        public string Description
        {
            get
            {
                return null;
            }
        }

        public DataTemplate CellTemplate
        {
            get
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.PlaybackState)))
                {
                    return (DataTemplate)XamlReader.Load(stream);
                }
            }
        }

        public IEnumerable<string> MetaData
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            if (!type.HasFlag(DatabaseInitializeType.Playlist))
            {
                return;
            }
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                var set = database.Set<PlaylistColumn>(transaction);
                set.Add(new PlaylistColumn()
                {
                    Name = "Playing",
                    Type = PlaylistColumnType.Plugin,
                    Sequence = 0,
                    Plugin = this.Id,
                    Enabled = true
                });
                transaction.Commit();
            }
        }
    }
}
