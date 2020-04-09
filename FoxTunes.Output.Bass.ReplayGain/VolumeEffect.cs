using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class VolumeEffect : BaseComponent, IDisposable
    {
        private VolumeEffect()
        {
            this.Parameters = new VolumeParameters();
        }

        public VolumeEffect(BassOutputStream stream, float replayGain, ReplayGainMode mode) : this()
        {
            this.Stream = stream;
            this.ReplayGain = replayGain;
            this.Mode = mode;
            this.Activate();
        }

        public int EffectHandle { get; private set; }

        public bool IsActive { get; private set; }

        public VolumeParameters Parameters { get; private set; }

        public BassOutputStream Stream { get; private set; }

        public float ReplayGain { get; private set; }

        public ReplayGainMode Mode { get; private set; }

        public int Channel
        {
            get
            {
                return this.Parameters.lChannel;
            }
            set
            {
                this.Parameters.lChannel = value;
                this.OnPropertyChanged("Channel");
            }
        }

        public float Volume
        {
            get
            {
                return this.Parameters.fVolume;
            }
            set
            {
                this.Parameters.fVolume = value;
                this.OnPropertyChanged("Volume");
            }
        }

        public void Activate()
        {
            this.EffectHandle = Bass.ChannelSetFX(this.Stream.ChannelHandle, this.Parameters.FXType, 0);
            if (this.EffectHandle == 0)
            {
                BassUtils.Throw();
            }
            this.Channel = 0;
            this.Volume = GetVolume(this.ReplayGain);
            if (!Bass.FXSetParameters(this.EffectHandle, this.Parameters))
            {
                BassUtils.Throw();
            }
            this.IsActive = true;
        }

        public void Deactivate()
        {
            if (!this.IsActive)
            {
                return;
            }
            //Ignoring result on purpose.
            Bass.ChannelRemoveFX(this.Stream.ChannelHandle, this.EffectHandle);
            this.EffectHandle = 0;
            this.IsActive = false;
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
            this.Deactivate();
        }

        ~VolumeEffect()
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

        public static float GetVolume(float replayGain)
        {
            return Convert.ToSingle(Math.Pow(10, replayGain / 20));
        }
    }
}
