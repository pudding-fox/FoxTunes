using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class OutputStream : BaseComponent, IOutputStream
    {
        protected OutputStream(PlaylistItem playlistItem)
        {
            this.PlaylistItem = playlistItem;
        }

        public int Id
        {
            get
            {
                return this.PlaylistItem.Id;
            }
        }

        public string FileName
        {
            get
            {
                return this.PlaylistItem.FileName;
            }
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public abstract long Position { get; set; }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged = delegate { };

        public abstract long Length { get; }

        public abstract int PCMRate { get; }

        public abstract int DSDRate { get; }

        public abstract int Channels { get; }

        public abstract bool IsPlaying { get; }

        protected virtual void OnIsPlayingChanged()
        {
            if (this.IsPlayingChanged != null)
            {
                this.IsPlayingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsPlaying");
        }

        public event EventHandler IsPlayingChanged = delegate { };

        public abstract bool IsPaused { get; }

        protected virtual void OnIsPausedChanged()
        {
            if (this.IsPausedChanged != null)
            {
                this.IsPausedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsPaused");
        }

        public event EventHandler IsPausedChanged = delegate { };

        public abstract bool IsStopped { get; }

        protected virtual void OnIsStoppedChanged()
        {
            if (this.IsStoppedChanged != null)
            {
                this.IsStoppedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsStopped");
        }

        public event EventHandler IsStoppedChanged = delegate { };

        public abstract void Play();

        protected virtual void OnPlayed(bool manual)
        {
            if (this.Played == null)
            {
                return;
            }
            this.Played(this, new PlayedEventArgs(manual));
        }

        public event PlayedEventHandler Played = delegate { };

        public abstract void Pause();

        protected virtual void OnPaused()
        {
            if (this.Paused == null)
            {
                return;
            }
            this.Paused(this, EventArgs.Empty);
        }

        public event EventHandler Paused = delegate { };

        public abstract void Resume();

        protected virtual void OnResumed()
        {
            if (this.Resumed == null)
            {
                return;
            }
            this.Resumed(this, EventArgs.Empty);
        }

        public event EventHandler Resumed = delegate { };

        public abstract void Stop();

        protected virtual void OnStopping()
        {
            if (this.Stopping == null)
            {
                return;
            }
            this.Stopping(this, EventArgs.Empty);
        }

        public event EventHandler Stopping = delegate { };

        protected virtual void OnStopped(bool manual)
        {
            if (this.Stopped == null)
            {
                return;
            }
            this.Stopped(this, new StoppedEventArgs(manual));
        }

        public event StoppedEventHandler Stopped = delegate { };

        protected virtual void EmitState()
        {
            this.OnIsPlayingChanged();
            this.OnIsPausedChanged();
            this.OnIsStoppedChanged();
        }

        public string Description
        {
            get
            {
                return string.Format("Length = {0},  Rate (PCM) = {1}, Rate (DSD) = {2}, Channels = {3}", this.Length, this.PCMRate, this.DSDRate, this.Channels);
            }
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

        protected abstract void OnDisposing();

        ~OutputStream()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
