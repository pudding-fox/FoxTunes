using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FoxTunes
{
    [Component(ID)]
    [WindowsUserInterfaceDependency]
    public class AdamantineTheme : ThemeBase, IConfigurableComponent
    {
        public const string ID = "06464CF4-118F-47EA-9597-303D305EF847";

        public AdamantineTheme()
            : base(ID, Strings.AdamantineTheme_Name, Strings.AdamantineTheme_Description)
        {

        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.ConnectSetting(
                AdamantineThemeConfiguration.SECTION,
                AdamantineThemeConfiguration.LIST_ROW_SHADING,
                () => new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Adamantine_ListRowShading.xaml", UriKind.Relative)
                }
            );
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
                Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/Adamantine.xaml", UriKind.Relative)
            };
        }

        public override Stream GetArtworkPlaceholder()
        {
            return typeof(AdamantineTheme).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Themes.Images.Adamantine_Artwork.png");
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return AdamantineThemeConfiguration.GetConfigurationSections();
        }
    }
}
