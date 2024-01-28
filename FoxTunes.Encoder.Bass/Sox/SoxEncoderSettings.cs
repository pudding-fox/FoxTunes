using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;

namespace FoxTunes
{
    public class SoxEncoderSettings : BassEncoderSettings
    {
        private SoxEncoderSettings()
        {
            var directory = Path.GetDirectoryName(
                typeof(SoxEncoderSettings).Assembly.Location
            );
            this.Executable = Path.Combine(directory, "Encoders\\sox.exe");
        }

        public SoxEncoderSettings(IBassEncoderSettings settings) : this()
        {
            this.Settings = settings;
        }

        public IBassEncoderSettings Settings { get; }

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
            if (channelInfo.Flags.HasFlag(BassFlags.Float))
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Resampling file \"{0}\" from 32 bit float to {1} bit integer.", encoderItem.InputFileName, depth);
                return string.Format(
                    "-t raw -e floating-point --bits 32 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {0} -c {1} -",
                    channelInfo.Frequency,
                    channelInfo.Channels,
                    depth
                );
            }
            else
            {
                Logger.Write(this.GetType(), LogLevel.Debug, "Resampling file \"{0}\" from 16 bit integer to {1} bit integer.", encoderItem.InputFileName, depth);
                return string.Format(
                    "-t raw -e signed-integer --bits 16 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {0} -c {1} -",
                    channelInfo.Frequency,
                    channelInfo.Channels,
                    depth
                );
            }
        }
    }
}
