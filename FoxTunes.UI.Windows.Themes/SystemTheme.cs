using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public class SystemTheme : ThemeBase
    {
        public SystemTheme()
            : base("D4EBB53F-BF59-4D61-99E1-D6D52926AE3F", "System")
        {

        }

        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Clear();
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/FoxTunes.UI.Windows.Themes;component/Themes/System.xaml", UriKind.Relative)
                }
            );
        }
    }
}
