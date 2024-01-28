using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace FoxTunes.ViewModel
{
    public class Playback : ViewModelBase
    {
        private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromMilliseconds(500);

        public static readonly DispatcherTimer Timer;

        static Playback()
        {
            Timer = new DispatcherTimer(DispatcherPriority.Background);
            Timer.Interval = UPDATE_INTERVAL;
            Timer.Start();
        }

        public Playback(bool monitor)
        {
            if (Timer != null && monitor)
            {
                Timer.Tick += this.OnTick;
            }
        }

        public Playback() : this(true)
        {

        }

        public IOutputStream OutputStream { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        private bool _IsPlaying { get; set; }

        public bool IsPlaying
        {
            get
            {
                return this._IsPlaying;
            }
            set
            {
                this._IsPlaying = value;
                this.OnIsPlayingChanged();
            }
        }

        protected virtual void OnIsPlayingChanged()
        {
            if (this.IsPlayingChanged != null)
            {
                this.IsPlayingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsPlaying");
        }

        public event EventHandler IsPlayingChanged;

        public string Caption
        {
            get
            {
                if (this.IsPlaying)
                {
                    return ";";
                }
                else
                {
                    return "4";
                }
            }
        }

        protected virtual void OnCaptionChanged()
        {
            if (this.CaptionChanged != null)
            {
                this.CaptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Caption");
        }

        public event EventHandler CaptionChanged;

        public ICommand PlayCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream == null)
                        {
                            return this.PlaylistManager.Next();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsPaused)
                        {
                            return this.PlaybackManager.CurrentStream.Resume();
                        }
                        else if (this.PlaybackManager.CurrentStream.IsStopped)
                        {
                            return this.PlaybackManager.CurrentStream.Play();
                        }
                        else
                        {
                            return this.PlaybackManager.CurrentStream.Pause();
                        }
                    }
                );
            }
        }

        public ICommand StopStreamCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        if (this.PlaybackManager.CurrentStream != null)
                        {
                            return this.PlaybackManager.CurrentStream.Stop();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }
                );
            }
        }

        public ICommand StopOutputCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaybackManager.Stop()
                );
            }
        }

        public ICommand PreviousCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Previous()
                );
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () => this.PlaylistManager.Next()
                );
            }
        }

        protected virtual void OnTick(object sender, EventArgs e)
        {
            var isPlaying = default(bool);
            if (this.PlaybackManager != null)
            {
                var currentStream = PlaybackManager.CurrentStream;
                if (currentStream != null)
                {
                    isPlaying = currentStream.IsPlaying;
                }
            }
            var refresh = default(bool);
            if (this.IsPlaying != isPlaying)
            {
                this.IsPlaying = isPlaying;
                refresh = true;
            }
            if (refresh)
            {
                this.OnCaptionChanged();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override void OnDisposing()
        {
            if (Timer != null)
            {
                Timer.Tick -= this.OnTick;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
