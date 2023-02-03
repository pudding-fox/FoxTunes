using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class AeroTheme : ThemeBase
    {
        public const string ID = "99941BF7-D348-493B-9E4E-3CAC71C562B6";

        public AeroTheme() : base(ID, Strings.AeroTheme_Name, Strings.AeroTheme_Description, GetColorPalettes())
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
                Source = new Uri("/PresentationFramework.Aero,Version=4.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35,processorArchitecture=MSIL;component/themes/Aero.NormalColor.xaml", UriKind.Relative)
            });
            resourceDictionary.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Aero.xaml", UriKind.Relative)
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
                    Strings.AeroTheme_ColorPalette_Default_Name,
                    Strings.AeroTheme_ColorPalette_Default_Description,
                    Resources.Blue
                ),
                new ColorPalette(
                    ID + "_BBBB",
                    ColorPaletteRole.Visualization,
                    Strings.AeroTheme_ColorPalette_Gradient1_Name,
                    Strings.AeroTheme_ColorPalette_Gradient1_Description,
                    Resources.Transparent_Blue
                ),
            };
        }
    }
}
