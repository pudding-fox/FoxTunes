using FoxTunes.Interfaces;
using System.Linq;
using System.Windows;

namespace FoxTunes.Theme
{
    public class ThemeLoader : BaseComponent, IThemeLoader
    {
        public ITheme Theme { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            var types = ComponentScanner.Instance.GetComponents(typeof(ITheme));
            var type = types.FirstOrDefault(); //Using the first theme for now.
            this.Theme = ComponentActivator.Instance.Activate<ITheme>(type);
            base.InitializeComponent(core);
        }

        public void Apply(Application application)
        {
            this.Theme.Apply(application);
        }
    }
}
