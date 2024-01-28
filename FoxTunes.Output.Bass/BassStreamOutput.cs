using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Mix;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public abstract class BassStreamOutput : BassStreamComponent, IBassStreamOutput
    {
        protected BassStreamOutput(IBassStreamPipeline pipeline, BassFlags flags) : base(pipeline, flags)
        {

        }

        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        public IOutputVolume Volume { get; private set; }

        public IErrorEmitter ErrorEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Volume = core.Components.OutputEffects.Volume;
            if (this.Volume != null)
            {
                this.Volume.EnabledChanged += this.OnVolumeEnabledChanged;
                this.Volume.ValueChanged += this.OnVolumeValueChanged;
            }
            this.ErrorEmitter = core.Components.ErrorEmitter;
            base.InitializeComponent(core);
        }

        protected virtual void OnVolumeEnabledChanged(object sender, EventArgs e)
        {
            this.UpdateVolume();
        }

        protected virtual void OnVolumeValueChanged(object sender, EventArgs e)
        {
            this.UpdateVolume();
        }

        protected virtual void UpdateVolume()
        {
            var volume = default(float);
            if (this.Volume != null && this.Volume.Available && this.Volume.Enabled)
            {
                volume = this.Volume.Value;
            }
            else
            {
                volume = 1;
            }
            this.SetVolume(volume);
        }

        public abstract bool CheckFormat(int rate, int channels);

        public abstract bool IsPlaying { get; }

        public abstract bool IsPaused { get; }

        public abstract bool IsStopped { get; }

        public virtual void PreviewPlay()
        {
            //Nothing to do.
        }

        public virtual void PreviewPause()
        {
            //Nothing to do.
        }

        public virtual void PreviewResume()
        {
            //Nothing to do.
        }

        public virtual void PreviewStop()
        {
            //Nothing to do.
        }

        public abstract void Play();

        public abstract void Pause();

        public abstract void Resume();

        public abstract void Stop();

        public virtual bool CanGetData
        {
            get
            {
                return this.GetMixerChannelHandles().Any();
            }
        }

        protected abstract IEnumerable<int> GetMixerChannelHandles();

        private static readonly object ChannelDataSyncRoot = new object();

        public virtual int GetData(short[] buffer)
        {
            foreach (var channelHandle in this.GetMixerChannelHandles())
            {
                //Critical: BassMix.ChannelGetData will trigger an access violation in a random place if called concurrently with different buffer sizes. Yes this took a long time to work out.
                lock (ChannelDataSyncRoot)
                {
                    return BassMix.ChannelGetData(channelHandle, buffer, buffer.Length);
                }
            }
            return 0;
        }

        public virtual int GetData(float[] buffer)
        {
            foreach (var channelHandle in this.GetMixerChannelHandles())
            {
                //Critical: BassMix.ChannelGetData will trigger an access violation in a random place if called concurrently with different buffer sizes. Yes this took a long time to work out.
                lock (ChannelDataSyncRoot)
                {
                    return BassMix.ChannelGetData(channelHandle, buffer, buffer.Length);
                }
            }
            return 0;
        }

        public virtual int GetData(float[] buffer, int fftSize)
        {
            var length = default(uint);
            switch (fftSize)
            {
                case BassFFT.FFT256:
                    length = BassFFT.FFT256_MASK;
                    break;
                case BassFFT.FFT512:
                    length = BassFFT.FFT512_MASK;
                    break;
                case BassFFT.FFT1024:
                    length = BassFFT.FFT1024_MASK;
                    break;
                case BassFFT.FFT2048:
                    length = BassFFT.FFT2048_MASK;
                    break;
                case BassFFT.FFT4096:
                    length = BassFFT.FFT4096_MASK;
                    break;
                case BassFFT.FFT8192:
                    length = BassFFT.FFT8192_MASK;
                    break;
                case BassFFT.FFT16384:
                    length = BassFFT.FFT16384_MASK;
                    break;
                case BassFFT.FFT32768:
                    length = BassFFT.FFT32768_MASK;
                    break;
                default:
                    throw new NotImplementedException();
            }
            foreach (var channelHandle in this.GetMixerChannelHandles())
            {
                //Critical: BassMix.ChannelGetData will trigger an access violation in a random place if called concurrently with different buffer sizes. Yes this took a long time to work out.
                lock (ChannelDataSyncRoot)
                {
                    return BassMix.ChannelGetData(channelHandle, buffer, unchecked((int)length));
                }
            }
            return 0;
        }

        protected abstract float GetVolume();

        protected abstract void SetVolume(float volume);

        protected override void OnDisposing()
        {
            if (this.Volume != null)
            {
                this.Volume.EnabledChanged -= this.OnVolumeEnabledChanged;
                this.Volume.ValueChanged -= this.OnVolumeValueChanged;
            }
        }
    }
}
