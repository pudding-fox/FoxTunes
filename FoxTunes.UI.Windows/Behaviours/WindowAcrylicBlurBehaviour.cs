using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
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
                this.Refresh();
            });
        }

        protected override void OnRefresh()
        {
            var windows = new HashSet<IntPtr>();
            foreach (var window in WindowBase.Active)
            {
                windows.Add(window.Handle);
                WindowExtensions.EnableAcrylicBlur(window.Handle, this.AccentColor);
            }
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return WindowAcrylicBlurBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
