using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Gapless;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassOutputChannel : BaseComponent, IDisposable
    {
        public BassOutputChannel(BassOutput output)
        {
            this.Output = output;
        }

        public BassOutput Output { get; private set; }

        public int Rate { get; private set; }

        public int Channels { get; private set; }

        public int ChannelHandle { get; private set; }

        public BassFlags Flags
        {
            get
            {
                var flags = BassFlags.Default;
                if (this.Output.Float)
                {
                    flags |= BassFlags.Float;
                }
                return flags;
            }
        }

        public virtual bool IsStarted { get; private set; }

        public virtual bool IsPlaying
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Playing;
            }
        }

        public virtual bool IsPaused
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Paused;
            }
        }

        public virtual bool IsStopped
        {
            get
            {
                return Bass.ChannelIsActive(this.ChannelHandle) == PlaybackState.Stopped;
            }
        }

        protected virtual void StartChannel(BassOutputStream outputStream)
        {
            this.Rate = outputStream.SampleRate;
            this.Channels = outputStream.Channels;
            BassUtils.OK(BassGapless.Init());
            try
            {
                this.CreateChannel();
                this.OnChannelStarted();
            }
            catch
            {
                this.StopChannel();
                throw;
            }
        }

        protected virtual void CreateChannel()
        {
            this.ChannelHandle = BassGapless.StreamCreate(this.Rate, this.Channels, this.Flags);
            if (this.ChannelHandle == 0)
            {
                BassUtils.Throw();
            }
        }

        protected virtual void OnChannelStarted()
        {
            this.IsStarted = true;
        }

        protected virtual void StopChannel()
        {
            this.FreeChannel();
            //Ignore errors.
            BassGapless.Free();
            this.OnChannelStopped();
        }

        protected virtual void FreeChannel()
        {
            //Ignore errors.
            Bass.StreamFree(this.ChannelHandle);
        }

        protected virtual void OnChannelStopped()
        {
            this.IsStarted = false;
        }

        public IEnumerable<int> Queue
        {
            get
            {
                var count = default(int);
                return BassGapless.GetChannels(out count);
            }
        }

        public bool Contains(BassOutputStream outputStream)
        {
            return this.Queue.Contains(outputStream.ChannelHandle);
        }

        public int Position(BassOutputStream outputStream)
        {
            var count = default(int);
            var channelHandles = BassGapless.GetChannels(out count);
            return channelHandles.IndexOf(outputStream.ChannelHandle);
        }

        public void Enqueue(BassOutputStream outputStream)
        {
            BassUtils.OK(BassGapless.ChannelEnqueue(outputStream.ChannelHandle));
        }

        public void Remove(BassOutputStream outputStream)
        {
            BassUtils.OK(BassGapless.ChannelRemove(outputStream.ChannelHandle));
        }

        public void Clear()
        {
            foreach (var channelHandle in this.Queue)
            {
                BassUtils.OK(BassGapless.ChannelRemove(channelHandle));
            }
        }

        public virtual void ClearBuffer(BassOutputStream outputStream)
        {
            //Nothing to do.
        }

        public virtual bool CanPlay(BassOutputStream outputStream)
        {
            return outputStream.SampleRate == this.Rate && outputStream.Channels == this.Channels;
        }

        public virtual void Play(BassOutputStream outputStream, bool reconfigure)
        {
            if (!this.IsStarted)
            {
                this.StartChannel(outputStream);
            }
            else if (!this.CanPlay(outputStream))
            {
                if (reconfigure)
                {
                    this.StopChannel();
                    this.StartChannel(outputStream);
                }
                else
                {
                    throw new ApplicationException("Cannot play with current configuration.");
                }
            }
            else if (this.Position(outputStream) == 0)
            {
                //Nothing to do, already playing.
                return;
            }
            else
            {
                this.Stop();
                this.Clear();
            }
            this.Enqueue(outputStream);
            this.Play();
        }

        public virtual void Play()
        {
            Logger.Write(this, LogLevel.Debug, "Starting output stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Pause()
        {
            Logger.Write(this, LogLevel.Debug, "Pausing output stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPause(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Resume()
        {
            Logger.Write(this, LogLevel.Debug, "Resuming output stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelPlay(this.ChannelHandle));
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
        }

        public virtual void Stop()
        {
            Logger.Write(this, LogLevel.Debug, "Stopping output stream: {0}", this.ChannelHandle);
            try
            {
                BassUtils.OK(Bass.ChannelStop(this.ChannelHandle));
            }
            catch (Exception e)
            {

                this.OnError(e);
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

        protected virtual void OnDisposing()
        {
            this.StopChannel();
        }

        ~BassOutputChannel()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
