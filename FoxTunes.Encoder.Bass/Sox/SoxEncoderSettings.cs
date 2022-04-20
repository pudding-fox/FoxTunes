using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;

namespace FoxTunes
{
    public class SoxEncoderSettings : BassEncoderTool
    {
        private SoxEncoderSettings()
        {

        }

        public SoxEncoderSettings(IBassEncoderSettings settings) : this()
        {
            this.Settings = settings;
        }

        public override string Executable
        {
            get
            {
                var directory = Path.GetDirectoryName(
                    typeof(SoxEncoderSettings).Assembly.Location
                );
                return Path.Combine(directory, "Encoders\\sox.exe");
            }
        }

        public IBassEncoderSettings Settings { get; }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Extension
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IBassEncoderFormat Format
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string GetArguments(EncoderItem encoderItem, IBassStream stream)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            var depth = this.Settings.GetDepth(encoderItem, stream);
            if (depth == 0)
            {
                depth = DEPTH_16;
            }
            var rate = this.Settings.GetRate(encoderItem, stream);
            if (rate == 0)
            {
                rate = OutputRate.PCM_44100;
            }
            if (channelInfo.Flags.HasFlag(BassFlags.Float))
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Resampling file \"{0}\": Depth 32 bit floating point -> {1} bit integer, Rate {2}Hz -> {3}Hz", encoderItem.InputFileName, depth, channelInfo.Frequency, rate);
                return string.Format(
                    "-t raw -e floating-point --bits 32 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {3} -c {1} -",
                    channelInfo.Frequency,
                    channelInfo.Channels,
                    depth,
                    rate
                );
            }
            else
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Resampling file \"{0}\": Depth 16 bit integer -> {1} bit integer, Rate {2}Hz -> {3}Hz", encoderItem.InputFileName, depth, channelInfo.Frequency, rate);
                return string.Format(
                    "-t raw -e signed-integer --bits 16 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {3} -c {1} -",
                    channelInfo.Frequency,
                    channelInfo.Channels,
                    depth,
                    rate
                );
            }
        }
    }
}
