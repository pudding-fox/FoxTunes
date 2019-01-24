using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for GlassLabel.xaml
    /// </summary>
    public partial class GlassLabel : UserControl
    {
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

        public static readonly DependencyProperty SectionProperty = DependencyProperty.Register(
            "Section",
            typeof(string),
            typeof(GlassLabel),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnSectionChanged))
        );

        public static string GetSection(GlassLabel source)
        {
            return (string)source.GetValue(SectionProperty);
        }

        public static void SetSection(GlassLabel source, string value)
        {
            source.SetValue(SectionProperty, value);
        }

        private static void OnSectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnSectionChanged();
        }

        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(
            "Element",
            typeof(string),
            typeof(GlassLabel),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnElementChanged))
        );

        public static string GetElement(GlassLabel source)
        {
            return (string)source.GetValue(ElementProperty);
        }

        public static void SetElement(GlassLabel source, string value)
        {
            source.SetValue(ElementProperty, value);
        }

        private static void OnElementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnElementChanged();
        }

        public GlassLabel()
        {
            this.InitializeComponent();
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

        public string Section
        {
            get
            {
                return GetSection(this);
            }
            set
            {
                SetSection(this, value);
            }
        }

        protected virtual void OnSectionChanged()
        {
            //Nothing to do.
        }

        public string Element
        {
            get
            {
                return GetElement(this);
            }
            set
            {
                SetElement(this, value);
            }
        }

        protected virtual void OnElementChanged()
        {
            //Nothing to do.
        }
    }
}
