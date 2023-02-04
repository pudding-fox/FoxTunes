using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID)]
    [WindowsUserInterfaceDependency]
    public class RoyaleTheme : ThemeBase
    {
        public const string ID = "B337A1E3-CA33-4769-BB13-E9F8DC70FE9D";

        public RoyaleTheme() : base(ID, Strings.RoyaleTheme_Name, Strings.RoyaleTheme_Description, GetColorPalettes())
        {

        }

        public override int CornerRadius
        {
            get
            {
                return 5;
            }
        }

        public override ResourceDictionary GetResourceDictionary()
        {
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/PresentationFramework.Royale,Version=4.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35,processorArchitecture=MSIL;component/themes/Royale.NormalColor.xaml", UriKind.Relative)
            });
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Royale.xaml", UriKind.Relative)
            });
            return resourceDictionary;
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(RoyaleTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.System_Artwork.png");
        }

        public static IEnumerable<IColorPalette> GetColorPalettes()
        {
            return new[]
            {
                new ColorPalette(
                    ID + "_AAAA",
                    ColorPaletteRole.Visualization,
                    Strings.RoyaleTheme_ColorPalette_Default_Name,
                    Strings.RoyaleTheme_ColorPalette_Default_Description,
                    Resources.Blue
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.RoyaleTheme_ColorPalette_Gradient1_Name,
                    Strings.RoyaleTheme_ColorPalette_Gradient1_Description,
                    Resources.Transparent_Blue
                ),
            };
        }
    }
}
