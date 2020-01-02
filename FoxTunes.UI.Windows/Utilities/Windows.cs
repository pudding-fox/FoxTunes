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
            MainWindow.Closed += OnMainWindowClosed;
            if (MainWindowCreated == null)
            {
                return;
            }
            MainWindowCreated(MainWindow, EventArgs.Empty);
        }

        public static event EventHandler MainWindowCreated;

        private static void OnMainWindowClosed(object sender, EventArgs e)
        {
            _MainWindow = new Lazy<Window>(() => new MainWindow());
            CheckShutdown();
        }

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
            MiniWindow.Closed += OnMiniWindowClosed;
            if (MiniWindowCreated == null)
            {
                return;
            }
            MiniWindowCreated(MiniWindow, EventArgs.Empty);
        }

        public static event EventHandler MiniWindowCreated;

        private static void OnMiniWindowClosed(object sender, EventArgs e)
        {
            _MiniWindow = new Lazy<Window>(() => new MiniWindow());
            CheckShutdown();
        }

        private static Lazy<Window> _SettingsWindow { get; set; }

        public static bool IsSettingsWindowCreated
        {
            get
            {
                return _SettingsWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window SettingsWindow
        {
            get
            {
                var raiseEvent = !IsSettingsWindowCreated;
                try
                {
                    return _SettingsWindow.Value;
                }
                finally
                {
                    if (IsSettingsWindowCreated && raiseEvent)
                    {
                        OnSettingsWindowCreated();
                    }
                }
            }
        }

        private static void OnSettingsWindowCreated()
        {
            SettingsWindow.Closed += OnSettingsWindowClosed;
            if (SettingsWindowCreated == null)
            {
                return;
            }
            SettingsWindowCreated(SettingsWindow, EventArgs.Empty);
        }

        public static event EventHandler SettingsWindowCreated;

        private static void OnSettingsWindowClosed(object sender, EventArgs e)
        {
            _SettingsWindow = new Lazy<Window>(() => new SettingsWindow());
        }

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

        public static event EventHandler ActiveWindowChanging;

        private static void OnActiveWindowChanged()
        {
            if (ActiveWindowChanged == null)
            {
                return;
            }
            ActiveWindowChanged(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveWindowChanged;

        private static void CheckShutdown()
        {
            if ((IsMainWindowCreated && MainWindow.IsVisible) || (IsMiniWindowCreated && MiniWindow.IsVisible))
            {
                return;
            }
            Shutdown();
        }

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
                        if (IsSettingsWindowCreated)
                        {
                            SettingsWindow.Close();
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
            _SettingsWindow = new Lazy<Window>(() => new SettingsWindow());
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
#if NET40
                    var source = new TaskCompletionSource<bool>();
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            action();
                        }
                        finally
                        {
                            source.SetResult(false);
                        }
                    }));
                    return source.Task;
#else
                    return dispatcher.BeginInvoke(action).Task;
#endif
                }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
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
#if NET40
                    var source = new TaskCompletionSource<bool>();
                    dispatcher.BeginInvoke(new Action(async () =>
                    {
                        try
                        {
                            await func().ConfigureAwait(false);
                        }
                        finally
                        {
                            source.SetResult(false);
                        }
                    }));
                    return source.Task;
#else
                    return dispatcher.BeginInvoke(func).Task;
#endif
                }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
