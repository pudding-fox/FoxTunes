using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    //TODO: Not sure of the exact required platform.
    //TODO: SetWindowCompositionAttribute is undocumented.
    //TODO: Assuming Windows 8.
    [PlatformDependency(Major = 6, Minor = 2)]
    public class WindowBlurBehaviour : StandardBehaviour
    {
        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Transparency { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            WindowBase.ActiveChanged += this.OnActiveChanged;
            this.Configuration = core.Components.Configuration;
            this.Transparency = this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.TRANSPARENCY
            );
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected virtual void Refresh()
        {
            if (!this.Transparency.Value)
            {
                return;
            }
            foreach (var window in WindowBase.Active)
            {
                WindowExtensions.EnableBlur(window.Handle);
            }
        }
    }
}
