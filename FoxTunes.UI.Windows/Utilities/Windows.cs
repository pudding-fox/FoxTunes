using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FoxTunes
{
    public static class Windows
    {
        static Windows()
        {
            Reset();
        }

        public static Dispatcher Dispatcher
        {
            get
            {
                if (IsMainWindowCreated)
                {
                    return MainWindow.Dispatcher;
                }
                if (IsMiniWindowCreated)
                {
                    return MiniWindow.Dispatcher;
                }
                if (Application.Current != null)
                {
                    return Application.Current.Dispatcher;
                }
                return null;
            }
        }

        private static Lazy<Window> _MainWindow { get; set; }

        public static bool IsMainWindowCreated
        {
            get
            {
                return _MainWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window MainWindow
        {
            get
            {
                var raiseEvent = !IsMainWindowCreated;
                try
                {
                    return _MainWindow.Value;
                }
                finally
                {
                    if (IsMainWindowCreated && raiseEvent)
                    {
                        OnMainWindowCreated();
                    }
                }
            }
        }

        private static void OnMainWindowCreated()
        {
            if (MainWindowCreated == null)
            {
                return;
            }
            MainWindowCreated(MainWindow, EventArgs.Empty);
        }

        public static event EventHandler MainWindowCreated = delegate { };

        private static Lazy<Window> _MiniWindow { get; set; }

        public static bool IsMiniWindowCreated
        {
            get
            {
                return _MiniWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window MiniWindow
        {
            get
            {
                var raiseEvent = !IsMiniWindowCreated;
                try
                {
                    return _MiniWindow.Value;
                }
                finally
                {
                    if (IsMiniWindowCreated && raiseEvent)
                    {
                        OnMiniWindowCreated();
                    }
                }
            }
        }

        private static void OnMiniWindowCreated()
        {
            if (MiniWindowCreated == null)
            {
                return;
            }
            MiniWindowCreated(MiniWindow, EventArgs.Empty);
        }

        public static event EventHandler MiniWindowCreated = delegate { };

        private static Window _ActiveWindow { get; set; }

        public static Window ActiveWindow
        {
            get
            {
                if (_ActiveWindow != null)
                {
                    return _ActiveWindow;
                }
                if (IsMiniWindowCreated && MiniWindow.IsVisible)
                {
                    return MiniWindow;
                }
                if (IsMainWindowCreated)
                {
                    return MainWindow;
                }
                return null;
            }
            set
            {
                OnActiveWindowChanging();
                _ActiveWindow = value;
                OnActiveWindowChanged();
            }
        }

        private static void OnActiveWindowChanging()
        {
            if (ActiveWindowChanging == null)
            {
                return;
            }
            ActiveWindowChanging(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveWindowChanging = delegate { };

        private static void OnActiveWindowChanged()
        {
            if (ActiveWindowChanged == null)
            {
                return;
            }
            ActiveWindowChanged(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveWindowChanged = delegate { };

        public static void Shutdown()
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(
                    DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        if (IsMiniWindowCreated)
                        {
                            MiniWindow.Close();
                        }
                        if (IsMainWindowCreated)
                        {
                            MainWindow.Close();
                        }
                        Reset();
                    })
                );
            }
        }

        private static void Reset()
        {
            _MainWindow = new Lazy<Window>(() => new MainWindow());
            _MiniWindow = new Lazy<Window>(() => new MiniWindow());
        }

        public static Task Invoke(Action action)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    return dispatcher.BeginInvoke(action).Task;
                }
            }
            return Task.CompletedTask;
        }

        public static Task Invoke(Func<Task> func)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.CheckAccess())
                {
                    return func();
                }
                else
                {
                    return dispatcher.BeginInvoke(func).Task;
                }
            }
            return Task.CompletedTask;
        }
    }
}
