using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class ClassicTheme : ThemeBase
    {
        public const string ID = "B55D8409-A389-4FF0-88FC-D1B0D88D81C7";

        public ClassicTheme() : base(ID, Strings.ClassicTheme_Name, Strings.ClassicTheme_Description, GetColorPalettes())
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
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/PresentationFramework.Classic,Version=4.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35,processorArchitecture=MSIL;component/themes/Classic.xaml", UriKind.Relative)
            });
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Classic.xaml", UriKind.Relative)
            });
            return resourceDictionary;
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
                    Strings.ClassicTheme_ColorPalette_Default_Name,
                    Strings.ClassicTheme_ColorPalette_Default_Description,
                    Resources.Blue
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.ClassicTheme_ColorPalette_Gradient1_Name,
                    Strings.ClassicTheme_ColorPalette_Gradient1_Description,
                    Resources.Transparent_Blue
                ),
            };
        }
    }
}
