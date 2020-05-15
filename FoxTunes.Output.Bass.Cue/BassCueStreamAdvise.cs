using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public class BassCueStreamAdvice : BassStreamAdvice
    {
        static BassCueStreamAdvice()
        {
            BassSubstreamHandler.Init();
        }

        public BassCueStreamAdvice(string fileName, string offset, string length)
        {
            this.FileName = fileName;
            this.Offset = CueSheetIndex.ToTimeSpan(offset);
            this.Length = CueSheetIndex.ToTimeSpan(length);
        }

        public override string FileName { get; protected set; }

        public override TimeSpan Offset { get; protected set; }

        public override TimeSpan Length { get; protected set; }

        public override bool Wrap(IBassStreamProvider provider, int channelHandle, out IBassStream stream)
        {
            var offset = default(long);
            var length = default(long);
            if (this.Offset != TimeSpan.Zero)
            {
                offset = Bass.ChannelSeconds2Bytes(channelHandle, this.Offset.TotalSeconds);
            }
            if (this.Length != TimeSpan.Zero)
            {
                length = Bass.ChannelSeconds2Bytes(channelHandle, this.Length.TotalSeconds);
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
