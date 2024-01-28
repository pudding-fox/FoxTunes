using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class BassSkipSilenceStreamAdvice : BassStreamAdvice
    {
        public BassSkipSilenceStreamAdvice(string fileName, TimeSpan leadIn, TimeSpan leadOut) : base(fileName)
        {
            this.LeadIn = leadIn;
            this.LeadOut = leadOut;
        }

        public TimeSpan LeadIn { get; private set; }

        public TimeSpan LeadOut { get; private set; }

        public override bool Wrap(IBassStreamProvider provider, int channelHandle, out IBassStream stream)
        {
            var offset = default(long);
            var length = default(long);
            if (this.LeadIn != TimeSpan.Zero)
            {
                offset = Bass.ChannelSeconds2Bytes(
                    channelHandle,
                    this.LeadIn.TotalSeconds
                );
            }
            if (this.LeadOut != TimeSpan.Zero)
            {
                length = Bass.ChannelGetLength(
                    channelHandle,
                    PositionFlags.Bytes
                ) - Bass.ChannelSeconds2Bytes(
                    channelHandle,
                    this.LeadOut.TotalSeconds
                );
            }
            if (offset != 0 || length != 0)
            {
                if (length == 0)
                {
                    length = Bass.ChannelGetLength(channelHandle, PositionFlags.Bytes) - offset;
                }
                stream = new BassSubstream(
                    provider,
                    BassSubstreamHandler.CreateStream(channelHandle, offset, length, BassFlags.AutoFree),
                    channelHandle,
                    offset,
                    length
                );
                return true;
            }
            stream = null;
            return false;
        }
    }
}
