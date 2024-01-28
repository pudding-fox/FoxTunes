using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

#if NET40
using Microsoft.Windows.Shell;
#else
using System.Windows.Shell;
#endif


namespace FoxTunes
{
    public abstract class WindowBase : Window
    {
        public WindowBase()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.Template = TemplateFactory.Template;
            this.SetValue(WindowChrome.WindowChromeProperty, new WindowChrome()
            {
                CaptionHeight = 30,
                ResizeBorderThickness = new Thickness(5)
            });
        }

        private static class TemplateFactory
        {
            private static Lazy<ControlTemplate> _Template = new Lazy<ControlTemplate>(GetTemplate);

            public static ControlTemplate Template
            {
                get
                {
                    return _Template.Value;
                }
            }

            private static ControlTemplate GetTemplate()
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(global::FoxTunes.Resources.WindowBase)))
                {
                    return (ControlTemplate)XamlReader.Load(stream);
                }
            }
        }
    }
}
