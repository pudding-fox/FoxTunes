using FoxTunes.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace FoxTunes
{
    public class AeroGlassBehaviour : StandardBehaviour
    {
        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement ExtendGlass { get; private set; }

        public bool IsCompositionEnabled
        {
            get
            {
                return Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.ExtendGlass = this.Configuration.GetElement<BooleanConfigurationElement>(
               MiniPlayerBehaviourConfiguration.SECTION,
               MiniPlayerBehaviourConfiguration.EXTEND_GLASS_ELEMENT
           );
            this.ExtendGlass.ConnectValue<bool>(value =>
            {
                if (Windows.IsMiniWindowCreated)
                {
                    if (value)
                    {
                        this.EnableGlass();
                    }
                    else
                    {
                        this.DisableGlass();
                    }
                }
            });
            Windows.MiniWindowCreated += this.OnMiniWindowCreated;
            base.InitializeComponent(core);
        }

        protected virtual void OnMiniWindowCreated(object sender, EventArgs e)
        {
            ((Window)sender).Loaded += this.OnLoaded;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.ExtendGlass.Value)
            {
                this.EnableGlass();
            }
        }

        protected virtual void EnableGlass()
        {
            var margins = new MARGINS();
            margins.Left = -1;
            this.DwmExtendFrameIntoClientArea(margins);
        }

        protected virtual void DisableGlass()
        {
            var margins = new MARGINS();
            this.DwmExtendFrameIntoClientArea(margins);
        }

        protected virtual void DwmExtendFrameIntoClientArea(MARGINS margins)
        {
            if (!this.IsCompositionEnabled)
            {
                return;
            }
            var handle = new WindowInteropHelper(Windows.MiniWindow).Handle;
            var source = HwndSource.FromHwnd(handle);
            source.CompositionTarget.BackgroundColor = Colors.Transparent;
            try
            {
                var result = DwmExtendFrameIntoClientArea(handle, ref margins);
                if (result < 0)
                {
                    //Something went wrong.
                }
                else
                {

                }
            }
            catch (DllNotFoundException)
            {
                //This shouldn't happen, we're running .NET 4.5 after all.
            }
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        };

        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
    }
}
