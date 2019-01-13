using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    public partial class Artwork : UserControl
    {
        public static readonly DependencyProperty ShowPlaceholderProperty = DependencyProperty.Register(
            "ShowPlaceholder",
            typeof(bool),
            typeof(Artwork),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnShowPlaceholderChanged))
        );

        public static bool GetShowPlaceholder(Artwork source)
        {
            return (bool)source.GetValue(ShowPlaceholderProperty);
        }

        public static void SetShowPlaceholder(Artwork source, bool value)
        {
            source.SetValue(ShowPlaceholderProperty, value);
        }

        private static void OnShowPlaceholderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var marquee = sender as Artwork;
            if (marquee == null)
            {
                return;
            }
            marquee.OnShowPlaceholderChanged();
        }

        public Artwork()
        {
            this.InitializeComponent();
        }

        public bool ShowPlaceholder
        {
            get
            {
                return GetShowPlaceholder(this);
            }
            set
            {
                SetShowPlaceholder(this, value);
            }
        }

        protected virtual void OnShowPlaceholderChanged()
        {
            //Nothing to do.
        }
    }
}
