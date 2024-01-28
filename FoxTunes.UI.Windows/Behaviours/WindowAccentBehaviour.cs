using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    //Requires Windows 11 22H2.
    [PlatformDependency(Major = 6, Minor = 2, Build = 22621)]
    public class WindowAccentBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        public WindowAccentBehaviour()
        {
            this.AccentColors = new Dictionary<IntPtr, Color>();
        }

        public IDictionary<IntPtr, Color> AccentColors { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Transparency { get; private set; }

        public TextConfigurationElement AccentColor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            WindowBase.ActiveChanged += this.OnActiveChanged;
            this.Configuration = core.Components.Configuration;
            this.Transparency = this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.TRANSPARENCY
            );
            this.AccentColor = this.Configuration.GetElement<TextConfigurationElement>(
                WindowAccentBehaviourConfiguration.SECTION,
                WindowAccentBehaviourConfiguration.ACCENT_COLOR
            );
            this.AccentColor.ValueChanged += this.OnValueChanged;
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected virtual void Refresh()
        {
            if (!this.Transparency.Value)
            {
                return;
            }
            var color = default(Color);
            if (!string.IsNullOrEmpty(this.AccentColor.Value))
            {
                color = this.AccentColor.Value.ToColor();
            }
            else
            {
                color = WindowExtensions.DefaultAccentColor;
            }
            this.Refresh(color);
        }

        protected virtual void Refresh(Color color)
        {
            var windows = new HashSet<IntPtr>();
            foreach (var window in WindowBase.Active)
            {
                windows.Add(window.Handle);
                var currentColor = default(Color);
                if (AccentColors.TryGetValue(window.Handle, out currentColor) && currentColor == color)
                {
                    continue;
                }
                this.Refresh(window, color);
                this.AccentColors[window.Handle] = color;
            }
            foreach (var handle in AccentColors.Keys.ToArray())
            {
                if (!windows.Contains(handle))
                {
                    AccentColors.Remove(handle);
                }
            }
        }

        protected virtual void Refresh(WindowBase window, Color color)
        {
            WindowExtensions.EnableAcrylicBlur(window.Handle, color);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowAccentBehaviourConfiguration.GetConfigurationSections();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            WindowBase.ActiveChanged -= this.OnActiveChanged;
        }

        ~WindowAccentBehaviour()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
