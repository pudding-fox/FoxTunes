using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for GlassLabel.xaml
    /// </summary>
    public partial class GlassLabel : Label
    {
        public static readonly DependencyProperty IsGlassEnabledProperty = DependencyProperty.RegisterAttached(
            "IsGlassEnabled",
            typeof(bool),
            typeof(GlassLabel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits)
        );

        public static bool GetIsGlassEnabled(DependencyObject source)
        {
            return (bool)source.GetValue(IsGlassEnabledProperty);
        }

        public static void SetIsGlassEnabled(DependencyObject source, bool value)
        {
            source.SetValue(IsGlassEnabledProperty, value);
        }

        public GlassLabel()
        {
            this.InitializeComponent();
        }

        public bool IsGlassEnabled
        {
            get
            {
                return GetIsGlassEnabled(this);
            }
            set
            {
                SetIsGlassEnabled(this, value);
            }
        }
    }
}
