using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    [ComponentPreference(ReleaseType.Minimal)]
    public class SystemTheme : ThemeBase
    {
        public const string ID = "392B2FEE-B1E5-4776-AF3A-C28260EE8E81";

        public SystemTheme()
            : base(ID, Strings.SystemTheme_Name, Strings.SystemTheme_Description, GetColorPalettes())
        {

        }

        public override int CornerRadius
        {
            get
            {
                return 0;
            }
        }

        public override ResourceDictionary GetResourceDictionary()
        {
            return new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
            };
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(SystemTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
        }

        public static IEnumerable<IColorPalette> GetColorPalettes()
        {
            return new[]
            {
                new ColorPalette(
                    ID + "_AAAA",
                    ColorPaletteRole.Visualization,
                    Strings.SystemTheme_ColorPalette_Default_Name,
                    Strings.SystemTheme_ColorPalette_Default_Description,
                    Strings.SystemTheme_ColorPalette_Default_Value
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.SystemTheme_ColorPalette_Gradient1_Name,
                    Strings.SystemTheme_ColorPalette_Gradient1_Description,
                    Strings.SystemTheme_ColorPalette_Gradient1_Value
                ),
            };
        }
    }
}
