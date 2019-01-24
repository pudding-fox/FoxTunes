using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class GlassLabel : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        public static readonly DependencyProperty SectionProperty = DependencyProperty.Register(
            "Section",
            typeof(string),
            typeof(GlassLabel),
            new PropertyMetadata(new PropertyChangedCallback(OnSectionChanged))
        );

        public static string GetSection(GlassLabel source)
        {
            return (string)source.GetValue(SectionProperty);
        }

        public static void SetSection(GlassLabel source, string value)
        {
            source.SetValue(SectionProperty, value);
        }

        public static void OnSectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnSectionChanged();
        }

        public string Section
        {
            get
            {
                return this.GetValue(SectionProperty) as string;
            }
            set
            {
                this.SetValue(SectionProperty, value);
            }
        }

        protected virtual void OnSectionChanged()
        {
            this.Refresh();
            if (this.SectionChanged != null)
            {
                this.SectionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Section");
        }

        public event EventHandler SectionChanged = delegate { };

        public static readonly DependencyProperty ElementProperty = DependencyProperty.Register(
            "Element",
            typeof(string),
            typeof(GlassLabel),
            new PropertyMetadata(new PropertyChangedCallback(OnElementChanged))
        );

        public static string GetElement(GlassLabel source)
        {
            return (string)source.GetValue(ElementProperty);
        }

        public static void SetElement(GlassLabel source, string value)
        {
            source.SetValue(ElementProperty, value);
        }

        public static void OnElementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var glassLabel = sender as GlassLabel;
            if (glassLabel == null)
            {
                return;
            }
            glassLabel.OnElementChanged();
        }

        public string Element
        {
            get
            {
                return this.GetValue(ElementProperty) as string;
            }
            set
            {
                this.SetValue(ElementProperty, value);
            }
        }

        protected virtual void OnElementChanged()
        {
            this.Refresh();
            if (this.ElementChanged != null)
            {
                this.ElementChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Element");
        }

        public event EventHandler ElementChanged = delegate { };

        private bool _IsGlassEnabled { get; set; }

        public bool IsGlassEnabled
        {
            get
            {
                return this._IsGlassEnabled;
            }
            set
            {
                this._IsGlassEnabled = value;
                this.OnIsGlassEnabledChanged();
            }
        }

        protected virtual void OnIsGlassEnabledChanged()
        {
            if (this.IsGlassEnabledChanged != null)
            {
                this.IsGlassEnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsGlassEnabled");
        }

        public event EventHandler IsGlassEnabledChanged = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void Refresh()
        {
            if (this.Configuration == null)
            {
                return;
            }
            var element = this.Configuration.GetElement<BooleanConfigurationElement>(this.Section, this.Element);
            if (element != null)
            {
                element.ConnectValue<bool>(value => Windows.Invoke(() => this.IsGlassEnabled = value));
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GlassLabel();
        }
    }
}
