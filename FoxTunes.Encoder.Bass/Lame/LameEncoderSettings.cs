using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public class LameEncoderSettings : BassEncoderTool, IStandardComponent, IConfigurableComponent
    {
        public override string Executable
        {
            get
            {
                var directory = Path.GetDirectoryName(
                    typeof(LameEncoderSettings).Assembly.Location
                );
                return Path.Combine(directory, "Encoders\\lame.exe");
            }
        }

        public override string Name
        {
            get
            {
                return "MP3";
            }
        }

        public override string Extension
        {
            get
            {
                return "mp3";
            }
        }

        public override IBassEncoderFormat Format
        {
            get
            {
                return new BassEncoderFormat(
                    DEPTH_16,
                    OutputRate.PCM_32000,
                    OutputRate.PCM_44100,
                    OutputRate.PCM_48000
                );
            }
        }

        public int Bitrate { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                LameEncoderSettingsConfiguration.SECTION,
                LameEncoderSettingsConfiguration.BITRATE_ELEMENT
            ).ConnectValue(option => this.Bitrate = LameEncoderSettingsConfiguration.GetBitrate(option));
            base.InitializeComponent(core);
        }

        public override string GetArguments(EncoderItem encoderItem, IBassStream stream)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            return string.Format(
                "--silent -r --little-endian --signed -s {0} -m {1} --bitwidth {2} - --cbr -b {3} \"{4}\"",
                channelInfo.Frequency,
                this.GetStereoMode(channelInfo.Channels),
                this.GetDepth(encoderItem, stream),
                this.Bitrate,
                encoderItem.OutputFileName
            );
        }

        protected virtual string GetStereoMode(int channels)
        {
            switch (channels)
            {
                case 1:
                    return "m";
                case 2:
                    return "s";
                default:
                    throw new NotImplementedException("Cannot encode multi channel input.");
            }
        }

        public override int GetDepth(EncoderItem encoderItem, IBassStream stream)
        {
            return DEPTH_16;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return LameEncoderSettingsConfiguration.GetConfigurationSections(this);
        }
    }
}
