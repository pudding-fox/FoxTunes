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
    public class WindowAcrylicBlurBehaviour : WindowBlurProvider
    {
        public const string ID = "BBBBC45C-11A4-4A2A-83F2-4FFED3C72C3E";

        public override string Id
        {
            get
            {
                return ID;
            }
        }

        public WindowAcrylicBlurBehaviour()
        {
            this.AccentColors = new Dictionary<IntPtr, Color>();
        }

        public IDictionary<IntPtr, Color> AccentColors { get; private set; }

        public Color AccentColor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Configuration.GetElement<TextConfigurationElement>(
                WindowAcrylicBlurBehaviourConfiguration.SECTION,
                WindowAcrylicBlurBehaviourConfiguration.ACCENT_COLOR
            ).ConnectValue(value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    AccentColor = WindowExtensions.DefaultAccentColor;
                }
                else
                {
                    AccentColor = value.ToColor();
                }
                if (this.AccentColors.Any())
                {
                    this.Refresh();
                }
            });
        }

        protected override void OnRefresh()
        {
            var windows = new HashSet<IntPtr>();
            foreach (var window in WindowBase.Active)
            {
                windows.Add(window.Handle);
                var color = default(Color);
                if (AccentColors.TryGetValue(window.Handle, out color) && color == this.AccentColor)
                {
                    continue;
                }
                WindowExtensions.EnableAcrylicBlur(window.Handle, this.AccentColor);
                this.AccentColors[window.Handle] = this.AccentColor;
            }
            foreach (var handle in AccentColors.Keys.ToArray())
            {
                if (!windows.Contains(handle))
                {
                    AccentColors.Remove(handle);
                }
            }
        }

        protected override void OnDisabled()
        {
            this.AccentColors.Clear();
            base.OnDisabled();
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowAcrylicBlurBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
