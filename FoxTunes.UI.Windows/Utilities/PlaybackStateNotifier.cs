using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FoxTunes
{
    public static class PlaybackStateNotifier
    {
        private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromMilliseconds(500);

        public static readonly IPlaybackManager PlaybackManager;

        public static readonly DispatcherTimer Timer;

        static PlaybackStateNotifier()
        {
            PlaybackManager = ComponentRegistry.Instance.GetComponent<IPlaybackManager>();
            if (PlaybackManager != null)
            {
                PlaybackManager.CurrentStreamChanged += OnCurrentStreamChanged;
            }
            Timer = new DispatcherTimer(DispatcherPriority.Background);
            Timer.Interval = UPDATE_INTERVAL;
            Timer.Start();
            Timer.Tick += OnTick;
        }

        private static void OnCurrentStreamChanged(object sender, AsyncEventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => Windows.Invoke(() => OnNotify()));
#else
            var task = Task.Run(() => Windows.Invoke(() => OnNotify()));
#endif
        }

        private static void OnTick(object sender, EventArgs e)
        {
            OnNotify();
        }

        private static void OnNotify()
        {
            if (Notify != null)
            {
                Notify(typeof(PlaybackStateNotifier), EventArgs.Empty);
            }
        }

        public static event EventHandler Notify;
    }
}
