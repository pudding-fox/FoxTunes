using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;
using FoxTunes.Interfaces;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Rating.xaml
    /// </summary>
    public partial class Rating : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(byte),
            typeof(Rating),
            new FrameworkPropertyMetadata(byte.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValueChanged))
        );

        public static byte GetValue(Rating source)
        {
            return (byte)source.GetValue(ValueProperty);
        }

        public static void SetValue(Rating source, byte value)
        {
            source.SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var rating = sender as Rating;
            if (rating == null)
            {
                return;
            }
            rating.OnValueChanged();
        }

        public Rating()
        {
            this.InitializeComponent();
        }

        public byte Value
        {
            get
            {
                return GetValue(this);
            }
            set
            {
                SetValue(this, value);
            }
        }

        protected virtual void OnValueChanged()
        {
            this.Star1.IsChecked = this.Value > 0;
            this.Star2.IsChecked = this.Value > 1;
            this.Star3.IsChecked = this.Value > 2;
            this.Star4.IsChecked = this.Value > 3;
            this.Star5.IsChecked = this.Value > 4;
            if (this.Value != GetValue(this.DataContext as PlaylistItem))
            {
                OnRatingChanged(this);
            }
        }

        private static void OnRatingChanged(Rating rating)
        {
            if (RatingChanged == null)
            {
                return;
            }
            RatingChanged(rating, new RatingChangedEventArgs(rating.DataContext as PlaylistItem, rating.Value));
        }

        public static event RatingChangedEventHandler RatingChanged;

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.Value = GetValue(this.DataContext as PlaylistItem);
        }

        protected virtual void OnClick(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton == null)
            {
                return;
            }
            var value = Convert.ToByte(toggleButton.Tag);
            if (!toggleButton.IsChecked.GetValueOrDefault())
            {
                value--;
            }
            this.Value = value;
        }

        public static byte GetValue(PlaylistItem playlistItem)
        {
            if (playlistItem == null)
            {
                return byte.MinValue;
            }
            var metaDataItem = playlistItem.MetaDatas.FirstOrDefault(
                _metaDataItem => string.Equals(_metaDataItem.Name, CommonMetaData.Rating, StringComparison.OrdinalIgnoreCase)
            );
            if (metaDataItem == null)
            {
                return byte.MinValue;
            }
            var value = default(byte);
            if (string.IsNullOrEmpty(metaDataItem.Value) || !byte.TryParse(metaDataItem.Value, out value))
            {
                return byte.MinValue;
            }
            return value;
        }
    }

    public delegate void RatingChangedEventHandler(object sender, RatingChangedEventArgs e);

    public class RatingChangedEventArgs : EventArgs
    {
        public RatingChangedEventArgs(PlaylistItem playlistItem, byte value)
        {
            this.PlaylistItem = playlistItem;
            this.Value = value;
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public byte Value { get; private set; }
    }
}
