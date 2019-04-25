using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    public class SystemTheme : ThemeBase
    {
        public SystemTheme()
            : base("D4EBB53F-BF59-4D61-99E1-D6D52926AE3F", "System")
        {

        }

        public ResourceDictionary ResourceDictionary { get; private set; }

        public override Stream ArtworkPlaceholder
        {
            get
            {
                return typeof(ExpressionDarkTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
            }
        }

        public override void Enable()
        {
            Application.Current.Resources.MergedDictionaries.Add(this.ResourceDictionary);
        }

        public override void Disable()
        {
            Application.Current.Resources.MergedDictionaries.Remove(this.ResourceDictionary);
        }

        public override void InitializeComponent(ICore core)
        {
            if (!(core.Flags.HasFlag(CoreFlags.Headless)))
            {
                this.ResourceDictionary = new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
                };
            }
            base.InitializeComponent(core);
        }
    }
}
