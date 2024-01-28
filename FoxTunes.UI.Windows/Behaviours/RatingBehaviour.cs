using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("2681C239-1291-4018-ACED-4933CC395FF6", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    [UIPlaylistColumnProvider("2681C239-1291-4018-ACED-4933CC395FF6", "Rating Stars")]
    public class RatingBehaviour : StandardBehaviour, IUIPlaylistColumnProvider, IDatabaseInitializer, IDisposable
    {
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

        public IPlaylistManager PlaylistManager { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public void InitializeDatabase(IDatabaseComponent database)
        {
            using (var transaction = database.BeginTransaction())
            {
                var set = database.Set<PlaylistColumn>(transaction);
                set.Add(new PlaylistColumn()
                {
                    Name = "Rating",
                    Type = PlaylistColumnType.Plugin,
                    Sequence = 100,
                    Plugin = typeof(RatingBehaviour).AssemblyQualifiedName,
                    Enabled = false
                });
                transaction.Commit();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            Rating.RatingChanged += this.OnRatingChanged;
            this.PlaylistManager = core.Managers.Playlist;
            this.MetaDataManager = core.Managers.MetaData;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void OnRatingChanged(object sender, RatingChangedEventArgs e)
        {
            var playlistItems = this.PlaylistManager.SelectedItems;
            if (playlistItems == null || !playlistItems.Contains(e.PlaylistItem))
            {
                this.SetRating(new[] { e.PlaylistItem }, e.Value);
            }
            else
            {
                this.SetRating(playlistItems.ToArray(), e.Value);
            }
        }

        protected virtual void SetRating(IEnumerable<PlaylistItem> playlistItems, byte rating)
        {
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.PlaylistManager.SetRating(playlistItems, rating)
            );
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            Rating.RatingChanged -= this.OnRatingChanged;
        }

        ~RatingBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
