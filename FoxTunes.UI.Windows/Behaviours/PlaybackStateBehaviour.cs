using FoxDb;
using FoxTunes.Interfaces;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace FoxTunes
{
    [Component("C0B2450C-DEDA-4D8B-8A32-5EA733F1FD45", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    [UIPlaylistColumnProvider("C0B2450C-DEDA-4D8B-8A32-5EA733F1FD45", "Playback State")]
    public class PlaybackStateBehaviour : StandardBehaviour, IUIPlaylistColumnProvider, IDatabaseInitializer
    {
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

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            if (!type.HasFlag(DatabaseInitializeType.Playlist))
            {
                return;
            }
            using (var transaction = database.BeginTransaction())
            {
                var set = database.Set<PlaylistColumn>(transaction);
                set.Add(new PlaylistColumn()
                {
                    Name = "Playing",
                    Type = PlaylistColumnType.Plugin,
                    Sequence = 0,
                    Plugin = typeof(PlaybackStateBehaviour).AssemblyQualifiedName,
                    Enabled = true
                });
                transaction.Commit();
            }
        }
    }
}
