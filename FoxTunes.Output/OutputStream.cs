using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

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

        public abstract long Length { get; }

        public abstract int Rate { get; }

        public abstract int Channels { get; }

        public abstract bool IsPlaying { get; }

        public abstract bool IsPaused { get; }

        public abstract bool IsStopped { get; }

        public abstract Task Play();

        public abstract Task Pause();

        public abstract Task Resume();

        public abstract Task Stop();

        protected virtual Task OnEnding()
        {
            if (this.Ending == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Ending(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Ending;

        protected virtual Task OnEnded()
        {
            if (this.Ended == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new AsyncEventArgs();
            this.Ended(this, e);
            return e.Complete();
        }

        public event AsyncEventHandler Ended;

        public virtual Task BeginSeek()
        {
            if (!this.IsPlaying)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Stop();
        }

        public virtual Task EndSeek()
        {
            if (!this.IsStopped)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.Play();
        }

        public string Description
        {
            get
            {
                return string.Format("Length = {0},  Rate {1}, Channels = {2}", this.Length, this.Rate, this.Channels);
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
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
