using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace FoxTunes.Theme
{
    public class ThemeRegistry : StandardComponent, IThemeRegistry
    {
        public ThemeRegistry()
        {
            this.Themes = new ObservableCollection<ITheme>(GetThemes());
        }

        public ObservableCollection<ITheme> Themes { get; private set; }

        private static IEnumerable<ITheme> GetThemes()
        {
            var types = ComponentScanner.Instance.GetComponents(typeof(ITheme));
            foreach (var type in types)
            {
                var theme = ComponentActivator.Instance.Activate<ITheme>(type);
                yield return theme;
            }
        }

        public ITheme GetTheme(string id)
        {
            return this.Themes.FirstOrDefault(theme =>
            {
                var attribute = theme.GetType().GetCustomAttribute<ThemeAttribute>();
                return attribute != null && string.Equals(attribute.Id, id, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}
