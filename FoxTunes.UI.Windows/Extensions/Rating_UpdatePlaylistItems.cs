using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    public static partial class RatingExtensions
    {
        public static readonly IPlaylistManager PlaylistManager = ComponentRegistry.Instance.GetComponent<IPlaylistManager>();

        private static readonly ConditionalWeakTable<Rating, UpdatePlaylistItemsBehaviour> UpdatePlaylistItemsBehaviours = new ConditionalWeakTable<Rating, UpdatePlaylistItemsBehaviour>();

        public static readonly DependencyProperty UpdatePlaylistItemsProperty = DependencyProperty.RegisterAttached(
            "UpdatePlaylistItems",
            typeof(bool),
            typeof(RatingExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnUpdatePlaylistItemsPropertyChanged))
        );

        public static bool GetUpdatePlaylistItems(Rating source)
        {
            return (bool)source.GetValue(UpdatePlaylistItemsProperty);
        }

        public static void SetUpdatePlaylistItems(Rating source, bool value)
        {
            source.SetValue(UpdatePlaylistItemsProperty, value);
        }

        private static void OnUpdatePlaylistItemsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rating = sender as Rating;
            if (rating == null)
            {
                return;
            }
            if (GetUpdatePlaylistItems(rating))
            {
                var behaviour = default(UpdatePlaylistItemsBehaviour);
                if (!UpdatePlaylistItemsBehaviours.TryGetValue(rating, out behaviour))
                {
                    UpdatePlaylistItemsBehaviours.Add(rating, new UpdatePlaylistItemsBehaviour(rating));
                }
            }
            else
            {
                var behaviour = default(UpdatePlaylistItemsBehaviour);
                if (UpdatePlaylistItemsBehaviours.TryGetValue(rating, out behaviour))
                {
                    UpdatePlaylistItemsBehaviours.Remove(rating);
                    behaviour.Dispose();
                }
            }
        }

        private class UpdatePlaylistItemsBehaviour : UIBehaviour
        {
            public UpdatePlaylistItemsBehaviour(Rating rating)
            {
                this.Rating = rating;
                this.Rating.ValueChanged += this.OnValueChanged;
            }

            public Rating Rating { get; private set; }

            protected virtual void OnValueChanged(object sender, RatingEventArgs e)
            {
                var playlistItem = e.FileData as PlaylistItem;
                if (playlistItem == null)
                {
                    return;
                }
                var playlistItems = PlaylistManager.SelectedItems;
                if (playlistItems == null || !playlistItems.Contains(playlistItem))
                {
                    this.SetRating(new[] { playlistItem }, e.Value);
                }
                else
                {
                    this.SetRating(playlistItems.ToArray(), e.Value);
                }
            }

            protected virtual void SetRating(IEnumerable<PlaylistItem> playlistItems, byte rating)
            {
                this.Dispatch(() => PlaylistManager.SetRating(playlistItems, rating));
            }
        }
    }
}
