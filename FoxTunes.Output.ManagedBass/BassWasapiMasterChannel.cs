using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Wasapi;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public class BassWasapiMasterChannel : BassMasterChannel
    {
        const int PRIMARY_CHANNEL = 0;

        public BassWasapiMasterChannel(BassOutput output) : base(output)
        {

        }

        public override BassFlags Flags
        {
            get
            {
                return base.Flags | BassFlags.Decode;
            }
        }

        public WasapiInitFlags InitFlags
        {
            get
            {
                return WasapiInitFlags.Exclusive;
            }
        }

        private int PreferredRate
        {
            get
            {
                if (this.Config.EffectiveRate > this.Output.Rate)
                {
                    return this.Output.Rate;
                }
                return this.Config.EffectiveRate;
            }
        }

        protected override void OnStartedStream()
        {
            this.Setup();
            if (this.Config.DsdDirect && this.Config.DsdRate > 0)
            {
                this.SetupDsd();
            }
            else
            {
                this.SetupPcm();
            }
            Logger.Write(this, LogLevel.Debug, "Enabled WASAPI on master stream: {0}", this.ChannelHandle);
            base.OnStartedStream();
        }

        protected virtual void Setup()
        {
            BassUtils.OK(BASS_WASAPI_InitGaplessMaster(this.Output.WasapiDevice, this.PreferredRate, this.Config.Channels, this.InitFlags));
        }

        protected virtual void SetupDsd()
        {

        }

        protected virtual void SetupPcm()
        {

        }

        protected override void OnFreeingStream()
        {
            if (BassWasapi.IsStarted)
            {
                BassUtils.OK(BassWasapi.Stop());
            }
            BassUtils.OK(BassWasapi.Free());
            base.OnFreeingStream();
        }

        public override void SetPrimaryChannel(int channelHandle)
        {
            base.SetPrimaryChannel(channelHandle);
            if (channelHandle != 0)
            {
                if (!BassWasapi.IsStarted)
                {
                    BassUtils.OK(BassWasapi.Start());
                }
            }
            else
            {
                if (BassWasapi.IsStarted)
                {
                    BassUtils.OK(BassWasapi.Stop());
                }
            }
        }

        [DllImport("bass_foxtunes.dll", EntryPoint = "BASS_WASAPI_InitGaplessMaster")]
        static extern int BASS_WASAPI_InitGaplessMaster(int Device, int Frequency = 0, int Channels = 0, WasapiInitFlags Flags = WasapiInitFlags.Shared, float Buffer = 0, float Period = 0, IntPtr User = default(IntPtr));
    }
}
