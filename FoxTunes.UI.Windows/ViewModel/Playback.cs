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

        public Playback(bool monitor)
        {
            if (monitor)
            {
                this.Timer = new DispatcherTimer(DispatcherPriority.Background);
                this.Timer.Interval = UPDATE_INTERVAL;
                this.Timer.Tick += this.OnTick;
                this.Timer.Start();
            }
        }

        public Playback() : this(true)
        {

        }

        public DispatcherTimer Timer { get; private set; }

        public IOutputStream OutputStream { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public bool IsPlaying
        {
            get
            {
                if (this.PlaybackManager == null)
                {
                    return false;
                }
                if (this.PlaybackManager.CurrentStream == null)
                {
                    return false;
                }
                return this.PlaybackManager.CurrentStream.IsPlaying;
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
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
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

        private bool _isPlaying = default(bool);

        protected virtual void OnTick(object sender, EventArgs e)
        {
            if (this.IsPlaying == this._isPlaying)
            {
                return;
            }
            this._isPlaying = this.IsPlaying;
            this.OnIsPlayingChanged();
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected override void OnDisposing()
        {
            if (this.Timer != null)
            {
                this.Timer.Stop();
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Playback();
        }
    }
}
