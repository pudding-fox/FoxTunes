using System;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID)]
    [WindowsUserInterfaceDependency]
    public class TransparentTheme : ThemeBase
    {
        public const string ID = "191C2E5B-4732-4CC7-BC31-4CC040DCFEC9";

        public TransparentTheme()
            : base(ID, Strings.TransparentTheme_Name, Strings.TransparentTheme_Description, global::FoxTunes.ColorPalettes.Transparent)
        {

        }

        public override int CornerRadius
        {
            get
            {
                return 0;
            }
        }

        public override ThemeFlags Flags
        {
            get
            {
                return ThemeFlags.RequiresTransparency;
            }
        }

        public override ResourceDictionary GetResourceDictionary()
        {
            return new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Transparent.xaml", UriKind.Relative)
            };
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(AdamantineTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.Transparent_Artwork.png");
        }
    }
}
