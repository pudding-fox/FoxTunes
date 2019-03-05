using FoxTunes.Interfaces;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace FoxTunes
{
    public class NotifyIconElement : FrameworkElement, IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(NotifyIconElement),
            new PropertyMetadata(new PropertyChangedCallback(OnImageSourceChanged))
        );

        public static ImageSource GetImageSource(DependencyObject source)
        {
            return (ImageSource)source.GetValue(ImageSourceProperty);
        }

        public static void SetImageSource(DependencyObject source, ImageSource value)
        {
            source.SetValue(ImageSourceProperty, value);
        }

        public static void OnImageSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var uri = new Uri((e.NewValue as ImageSource).ToString());
                SetIcon(sender, new Icon(Application.GetResourceStream(uri).Stream));
            }
            else
            {
                SetIcon(sender, null);
            }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(Icon),
            typeof(NotifyIconElement),
            new PropertyMetadata(new PropertyChangedCallback(OnIconChanged))
        );

        public static Icon GetIcon(DependencyObject source)
        {
            return (Icon)source.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject source, Icon value)
        {
            source.SetValue(IconProperty, value);
        }

        public static void OnIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //Nothing to do.
        }

        public NotifyIconElement()
        {
            this.InitializeComponent();
        }

        public INotifyIcon NotifyIcon { get; private set; }

        protected virtual void InitializeComponent()
        {
            this.Loaded += this.OnLoaded;
            this.IsEnabledChanged += this.OnIsEnabledChanged;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.NotifyIcon = ComponentRegistry.Instance.GetComponent<INotifyIcon>();
            if (this.IsEnabled)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        protected virtual void OnMouseLeftButtonDown(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = this
                });
            });
        }

        protected virtual void OnMouseLeftButtonUp(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseUpEvent,
                    Source = this
                });
            });
        }

        protected virtual void OnMouseRightButtonDown(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
                {
                    RoutedEvent = Mouse.MouseDownEvent,
                    Source = this
                });
            });
        }

        protected virtual void OnMouseRightButtonUp(object sender, EventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                this.ShowContextMenu();
                this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
                {
                    RoutedEvent = Mouse.MouseUpEvent,
                    Source = this
                });
            });
        }

        protected virtual void ShowContextMenu()
        {
            if (this.ContextMenu == null)
            {
                return;
            }

            var window = this.FindAncestor<Window>();
            if (Windows.ActiveWindow != window)
            {
                return;
            }

            var x = default(int);
            var y = default(int);
            MouseHelper.GetPosition(out x, out y);
            DpiHelper.TransformPosition(ref x, ref y);

            this.ContextMenu.DataContext = this.DataContext;
            this.ContextMenu.Placement = PlacementMode.AbsolutePoint;
            this.ContextMenu.HorizontalOffset = x;
            this.ContextMenu.VerticalOffset = y;
            this.ContextMenu.IsOpen = true;

            var source = PresentationSource.FromVisual(this.ContextMenu) as HwndSource;
            if (source != null && source.Handle != IntPtr.Zero)
            {
                SetForegroundWindow(source.Handle);
            }
        }

        protected virtual void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsEnabled)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }

        public void Show()
        {
            if (this.NotifyIcon == null)
            {
                return;
            }
            var icon = GetIcon(this);
            if (icon != null)
            {
                this.NotifyIcon.Icon = icon.Handle;
            }
            this.NotifyIcon.Show();
            if (this.NotifyIcon.MessageSink != null)
            {
                this.NotifyIcon.MessageSink.MouseLeftButtonDown += this.OnMouseLeftButtonDown;
                this.NotifyIcon.MessageSink.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
                this.NotifyIcon.MessageSink.MouseRightButtonDown += this.OnMouseRightButtonDown;
                this.NotifyIcon.MessageSink.MouseRightButtonUp += this.OnMouseRightButtonUp;
            }
        }

        public void Hide()
        {
            if (this.NotifyIcon == null)
            {
                return;
            }
            if (this.NotifyIcon.MessageSink != null)
            {
                this.NotifyIcon.MessageSink.MouseLeftButtonDown -= this.OnMouseLeftButtonDown;
                this.NotifyIcon.MessageSink.MouseLeftButtonUp -= this.OnMouseLeftButtonUp;
                this.NotifyIcon.MessageSink.MouseRightButtonDown -= this.OnMouseRightButtonDown;
                this.NotifyIcon.MessageSink.MouseRightButtonUp -= this.OnMouseRightButtonUp;
            }
            this.NotifyIcon.Hide();
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
            this.Hide();
        }

        ~NotifyIconElement()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
