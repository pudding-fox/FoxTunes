using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
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
                if (this.ThemeLoader != null)
                {
                    this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
                    this.Refresh();
                }
            }

            public ThemeLoader ThemeLoader { get; private set; }

            public IUserInterface UserInterface { get; private set; }

            public Window Window { get; private set; }

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

            protected override void OnDisposing()
            {
                if (this.ThemeLoader != null)
                {
                    this.ThemeLoader.ThemeChanged -= this.OnThemeChanged;
                }
                base.OnDisposing();
            }
        }
    }
}
