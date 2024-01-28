using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FoxTunes
{
    public static partial class WindowExtensions
    {
        private static readonly ConditionalWeakTable<Window, AllowsTransparencyBehaviour> AllowsTransparencyBehaviours = new ConditionalWeakTable<Window, AllowsTransparencyBehaviour>();

        public static readonly DependencyProperty AllowsTransparencyProperty = DependencyProperty.RegisterAttached(
            "AllowsTransparency",
            typeof(bool),
            typeof(WindowExtensions),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnAllowsTransparencyPropertyChanged))
        );

        public static bool GetAllowsTransparency(Window source)
        {
            return (bool)source.GetValue(AllowsTransparencyProperty);
        }

        public static void SetAllowsTransparency(Window source, bool value)
        {
            source.SetValue(AllowsTransparencyProperty, value);
        }

        private static void OnAllowsTransparencyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var window = sender as Window;
            if (window == null)
            {
                return;
            }
            if (GetAllowsTransparency(window))
            {
                var behaviour = default(AllowsTransparencyBehaviour);
                if (!AllowsTransparencyBehaviours.TryGetValue(window, out behaviour))
                {
                    AllowsTransparencyBehaviours.Add(window, new AllowsTransparencyBehaviour(window));
                }
            }
            else
            {
                AllowsTransparencyBehaviours.Remove(window);
            }
        }

        private class AllowsTransparencyBehaviour : UIBehaviour
        {
            private AllowsTransparencyBehaviour()
            {
                this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
                this.UserInterface = ComponentRegistry.Instance.GetComponent<IUserInterface>();
            }

            public AllowsTransparencyBehaviour(Window window) : this()
            {
                this.Window = window;
                this.Window.Loaded += this.OnLoaded;
                if (this.ThemeLoader != null)
                {
                    this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
                    this.Refresh();
                }
            }

            public ThemeLoader ThemeLoader { get; private set; }

            public IUserInterface UserInterface { get; private set; }

            public Window Window { get; private set; }

            protected virtual void OnLoaded(object sender, RoutedEventArgs e)
            {
                if (!this.Window.AllowsTransparency)
                {
                    return;
                }
                this.EnableBlur();
            }

            protected virtual void OnThemeChanged(object sender, EventArgs e)
            {
                this.Refresh();
            }

            public void Refresh()
            {
                var allowsTransparency = this.ThemeLoader.Theme.Flags.HasFlag(ThemeFlags.RequiresTransparency);
                if (this.Window.AllowsTransparency != allowsTransparency)
                {
                    if (new WindowInteropHelper(this.Window).Handle != IntPtr.Zero)
                    {
                        this.UserInterface.Warn(Strings.WindowExtensions_TransparencyWarning);
                        return;
                    }
                    this.Window.AllowsTransparency = allowsTransparency;
                }
            }

            protected virtual void EnableBlur()
            {
                var windowHelper = new WindowInteropHelper(this.Window);
                var accent = new AccentPolicy()
                {
                    AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
                };
                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                try
                {
                    Marshal.StructureToPtr(accent, accentPtr, false);
                    var data = new WindowCompositionAttributeData();
                    data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                    data.SizeOfData = accentStructSize;
                    data.Data = accentPtr;
                    SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                }
                finally
                {
                    Marshal.FreeHGlobal(accentPtr);
                }
            }

            protected override void OnDisposing()
            {
                if (this.Window != null)
                {
                    this.Window.Loaded -= this.OnLoaded;
                }
                if (this.ThemeLoader != null)
                {
                    this.ThemeLoader.ThemeChanged -= this.OnThemeChanged;
                }
                base.OnDisposing();
            }

            [DllImport("user32.dll")]
            public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

            public enum AccentState
            {
                ACCENT_DISABLED = 1,
                ACCENT_ENABLE_GRADIENT = 0,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_INVALID_STATE = 4
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct AccentPolicy
            {
                public AccentState AccentState;
                public int AccentFlags;
                public int GradientColor;
                public int AnimationId;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct WindowCompositionAttributeData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }

            public enum WindowCompositionAttribute
            {
                WCA_ACCENT_POLICY = 19
            }
        }
    }
}
