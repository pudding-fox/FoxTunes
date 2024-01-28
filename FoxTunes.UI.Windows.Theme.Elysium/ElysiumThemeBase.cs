using FoxTunes.Theme;
using System;
using System.Windows;

namespace FoxTunes
{
    public abstract class ElysiumThemeBase : ThemeBase
    {
        public override void Apply(Application application)
        {
            application.Resources.MergedDictionaries.Add(
                new ResourceDictionary()
                {
                    Source = new Uri("/Elysium;component/Themes/Generic.xaml", UriKind.Relative)
                }
            );
            this.Configure(application);
        }

        protected abstract void Configure(Application application);
    }
}
