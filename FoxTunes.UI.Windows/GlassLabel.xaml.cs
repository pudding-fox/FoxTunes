using FoxTunes.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for GlassLabel.xaml
    /// </summary>
    public partial class GlassLabel : UserControl
    {
        public static readonly DependencyProperty IsGlassEnabledProperty = DependencyProperty.Register(
            "IsGlassEnabled",
            typeof(bool),
            typeof(GlassLabel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsGlassEnabledChanged))
        );

        public static bool GetIsGlassEnabled(GlassLabel source)
        {
            return (bool)source.GetValue(IsGlassEnabledProperty);
        }

        public static void SetIsGlassEnabled(GlassLabel source, bool value)
        {
            source.SetValue(IsGlassEnabledProperty, value);
        }

        private static void OnIsGlassEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnIsGlassEnabledChanged();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(GlassLabel),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnTextChanged))
        );

        public static string GetText(GlassLabel source)
        {
            return (string)source.GetValue(TextProperty);
        }

        public static void SetText(GlassLabel source, string value)
        {
            source.SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnTextChanged();
        }

        public GlassLabel()
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            if (configuration != null)
            {
                var element = configuration.GetElement<BooleanConfigurationElement>(
                    WindowsUserInterfaceConfiguration.SECTION,
                    WindowsUserInterfaceConfiguration.EXTEND_GLASS_ELEMENT
                );
                if (element != null)
                {
                    element.ConnectValue<bool>(value => Windows.Invoke(() => this.IsGlassEnabled = value));
                }
            }
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

        protected virtual void OnIsGlassEnabledChanged()
        {
            //Nothing to do.
        }

        public string Text
        {
            get
            {
                return GetText(this);
            }
            set
            {
                SetText(this, value);
            }
        }

        protected virtual void OnTextChanged()
        {
            //Nothing to do.
        }
    }
}
