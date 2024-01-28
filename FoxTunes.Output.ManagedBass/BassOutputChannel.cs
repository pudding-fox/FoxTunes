using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Gapless;
using ManagedBass.Sox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassOutputChannel : BaseComponent, IDisposable
    {
        private readonly Metric BufferLengthMetric = new Metric(3);

        public BassOutputChannel(BassOutput output)
        {
            this.Output = output;
            this.EnforceRate = output.EnforceRate;
            this.SoxResampler = output.SoxResampler;
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

        public bool EnforceRate { get; private set; }

        public bool SoxResampler { get; private set; }

        public int PCMRate { get; private set; }

        public int DSDRate { get; private set; }

        public int Channels { get; private set; }

        public virtual bool ShouldResample
        {
            get
            {
                return this.SoxResampler && this.ShouldEnforceRate;
            }
        }

        public bool IsResampling { get; protected set; }

        public virtual bool ShouldEnforceRate
        {
            get
            {
                return (this.EnforceRate && this.Output.Rate != this.PCMRate) || !this.CheckRate(this.PCMRate);
            }
        }

        public bool IsEnforcingRate { get; protected set; }

        protected virtual bool CheckRate(int rate)
        {
            return true;
        }

        public int GaplessChannelHandle { get; private set; }

        public int ResamplerChannelHandle { get; private set; }

        public int ChannelHandle
        {
            get
            {
                if (this.ResamplerChannelHandle != 0)
                {
                    return this.ResamplerChannelHandle;
                }
                return this.GaplessChannelHandle;
            }
        }

        /// <remarks>
        /// If we define this property as <see cref="BassFlags"/> we will
        /// get a type load exception when starting up. I have no idea
        /// why. Good stuff.
        /// </remarks>
        public virtual Enum InputFlags { get; private set; }

        public virtual BassFlags OutputFlags
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

        public virtual long BufferLength
        {
            get
            {
                if (this.IsResampling)
                {
                    var length = default(int);
                    BassUtils.OK(BassSox.StreamBufferLength(this.ResamplerChannelHandle, out length));
                    return this.BufferLengthMetric.Average(length);
                }
                return 0;
            }
        }

        protected virtual void StartChannel(BassOutputStream outputStream)
        {
            this.PCMRate = BassUtils.GetChannelPcmRate(outputStream.ChannelHandle);
            this.DSDRate = BassUtils.GetChannelDsdRate(outputStream.ChannelHandle);
            this.Channels = BassUtils.GetChannelCount(outputStream.ChannelHandle);
            this.InputFlags = BassUtils.GetChannelFlags(outputStream.ChannelHandle);
            try
            {
                this.CreateChannel();
                this.OnChannelStarted();
            }
            catch (Exception e)
            {
                this.StopChannel();
                this.OnError(e);
                throw;
            }
        }

        protected virtual void CreateChannel()
        {
            this.CreateGaplessChannel();
            if (this.ShouldResample)
            {
                this.CreateResamplingChannel();
            }
        }

        protected virtual void CreateGaplessChannel()
        {
            var flags = this.OutputFlags;
            if (this.ShouldResample)
            {
                flags |= BassFlags.Decode;
            }
            Logger.Write(this, LogLevel.Debug, "Initializing BASS GAPLESS.");
            BassUtils.OK(BassGapless.Init());
            BassUtils.OK(BassGapless.SetConfig(BassGaplessAttriubute.KeepAlive, true));
            Logger.Write(this, LogLevel.Debug, "Creating BASS GAPLESS stream with rate {0} and {1} channels.", this.PCMRate, this.Channels);
            this.GaplessChannelHandle = BassGapless.StreamCreate(this.PCMRate, this.Channels, flags);
            if (this.GaplessChannelHandle == 0)
            {
                BassUtils.Throw();
            }
        }

        protected virtual void CreateResamplingChannel()
        {
            Logger.Write(this, LogLevel.Debug, "Initializing BASS SOX.");
            BassUtils.OK(BassSox.Init());
            Logger.Write(this, LogLevel.Debug, "Creating BASS SOX stream with rate {0} => {1} and {2} channels.", this.PCMRate, this.Output.Rate, this.Channels);
            this.ResamplerChannelHandle = BassSox.StreamCreate(this.Output.Rate, this.OutputFlags, this.ChannelHandle);
            if (this.ResamplerChannelHandle == 0)
            {
                BassUtils.Throw();
            }
            BassUtils.OK(BassSox.ChannelSetAttribute(this.ResamplerChannelHandle, SoxChannelAttribute.KeepAlive, true));
            this.IsResampling = true;
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
            this.FreeResamplingChannel();
            this.FreeGaplessChannel();
        }

        protected virtual void FreeGaplessChannel()
        {
            if (this.GaplessChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS GAPLESS stream: {0}", this.GaplessChannelHandle);
                this.Output.FreeStream(this.GaplessChannelHandle);
                this.GaplessChannelHandle = 0;
            }
        }

        protected virtual void FreeResamplingChannel()
        {
            if (this.ResamplerChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Freeing BASS SOX stream: {0}", this.ResamplerChannelHandle);
                BassSox.StreamFree(this.ResamplerChannelHandle);
                this.ResamplerChannelHandle = 0;
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

        public int CurrentChannelHandle
        {
            get
            {
                return this.Queue.FirstOrDefault();
            }
        }

        public bool QueueContains(BassOutputStream outputStream)
        {
            return this.Queue.Contains(outputStream.ChannelHandle);
        }

        public int QueuePosition(BassOutputStream outputStream)
        {
            var count = default(int);
            var channelHandles = BassGapless.GetChannels(out count);
            return channelHandles.IndexOf(outputStream.ChannelHandle);
        }

        public virtual void Enqueue(BassOutputStream outputStream)
        {
            var clearBuffer = default(bool);
            if (this.CurrentChannelHandle == 0)
            {
                clearBuffer = true;
            }
            Logger.Write(this, LogLevel.Debug, "Adding stream to the queue: {0}", outputStream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelEnqueue(outputStream.ChannelHandle));
            if (clearBuffer)
            {
                this.ClearBuffer();
            }
        }

        public virtual void Remove(BassOutputStream outputStream)
        {
            var clearBuffer = default(bool);
            if (this.CurrentChannelHandle == outputStream.ChannelHandle)
            {
                clearBuffer = true;
            }
            Logger.Write(this, LogLevel.Debug, "Removing stream from the queue: {0}", outputStream.ChannelHandle);
            BassUtils.OK(BassGapless.ChannelRemove(outputStream.ChannelHandle));
            if (clearBuffer)
            {
                this.ClearBuffer();
            }
        }

        public void Clear()
        {
            Logger.Write(this, LogLevel.Debug, "Clearing the queue.");
            foreach (var channelHandle in this.Queue)
            {
                BassUtils.OK(BassGapless.ChannelRemove(channelHandle));
            }
        }

        public virtual void ClearBuffer()
        {
            if (this.ResamplerChannelHandle != 0)
            {
                Logger.Write(this, LogLevel.Debug, "Clearing resampler buffer: {0}", this.ResamplerChannelHandle);
                BassUtils.OK(BassSox.StreamBufferClear(this.ResamplerChannelHandle));
            }
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
            else if (this.QueuePosition(outputStream) == 0)
            {
                this.Play();
                return;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Resetting the queue after reconfiguration.");
                this.Stop();
                this.Clear();
                this.ClearBuffer();
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
