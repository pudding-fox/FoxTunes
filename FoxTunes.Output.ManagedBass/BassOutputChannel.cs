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

        public virtual bool CanPlayPCM
        {
            get
            {
                return true;
            }
        }

        public virtual bool CanPlayDSD
        {
            get
            {
                return false;
            }
        }

        public virtual bool CheckFormat(int rate, int channels)
        {
            return true;
        }

        public int PCMRate { get; private set; }

        public int DSDRate { get; private set; }

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
            this.PCMRate = BassUtils.GetChannelPcmRate(outputStream.ChannelHandle);
            this.DSDRate = BassUtils.GetChannelDsdRate(outputStream.ChannelHandle);
            this.Channels = BassUtils.GetChannelCount(outputStream.ChannelHandle);
            Logger.Write(this, LogLevel.Debug, "Initializing BASS GAPLESS.");
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
            Logger.Write(this, LogLevel.Debug, "Creating BASS GAPLESS stream with rate {0} and {1} channels.", this.PCMRate, this.Channels);
            this.ChannelHandle = BassGapless.StreamCreate(this.PCMRate, this.Channels, this.Flags);
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
            Logger.Write(this, LogLevel.Debug, "Releasing BASS GAPLESS.");
            BassGapless.Free();
            this.OnChannelStopped();
        }

        protected virtual void FreeChannel()
        {
            if (this.ChannelHandle != 0)
            {
                this.Output.FreeStream(this.ChannelHandle);
                this.ChannelHandle = 0;
            }
        }

        protected virtual void OnChannelStopped()
        {
            this.IsStarted = false;
        }

        public IEnumerable<int> Queue
        {
            get
            {
                if (!this.IsStarted)
                {
                    return Enumerable.Empty<int>();
                }
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
            Logger.Write(this, LogLevel.Debug, "Adding stream to the queue: {0}", outputStream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelEnqueue(outputStream.ChannelHandle));
        }

        public void Remove(BassOutputStream outputStream)
        {
            Logger.Write(this, LogLevel.Debug, "Removing stream from the queue: {0}", outputStream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelRemove(outputStream.ChannelHandle));
        }

        public void Clear()
        {
            Logger.Write(this, LogLevel.Debug, "Clearing the queue.");
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
            return outputStream.PCMRate == this.PCMRate && outputStream.Channels == this.Channels;
        }

        public virtual void Play(BassOutputStream outputStream, bool reconfigure)
        {
            Logger.Write(this, LogLevel.Debug, "Playing stream: {0}", outputStream.ChannelHandle);
            if (!this.IsStarted)
            {
                this.StartChannel(outputStream);
            }
            else if (!this.CanPlay(outputStream))
            {
                if (reconfigure)
                {
                    Logger.Write(this, LogLevel.Debug, "Cannot play stream {0} with current configuration, restarting.", outputStream.ChannelHandle);
                    this.StopChannel();
                    this.StartChannel(outputStream);
                }
                else
                {
                    throw new InvalidOperationException("Cannot play with current configuration, set the reconfigure argument to true.");
                }
            }
            else if (this.Position(outputStream) == 0)
            {
                //Nothing to do, already playing.
                return;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Resetting the queue after reconfiguration.");
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
