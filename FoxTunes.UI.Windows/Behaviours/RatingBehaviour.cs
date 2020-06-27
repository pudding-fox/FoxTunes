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
    [Component("2681C239-1291-4018-ACED-4933CC395FF6", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class RatingBehaviour : StandardBehaviour, IUIPlaylistColumnProvider, IDatabaseInitializer
    {
        public string Id
        {
            get
            {
                return typeof(RatingBehaviour).AssemblyQualifiedName;
            }
        }

        public string Name
        {
            get
            {
                return "Rating Stars";
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
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.Rating)))
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
                    Name = "Rating",
                    Type = PlaylistColumnType.Plugin,
                    Sequence = 13,
                    Plugin = this.Id,
                    Enabled = false
                });
                transaction.Commit();
            }
        }
    }
}
