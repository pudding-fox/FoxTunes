using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public class FlacEncoderSettings : BassEncoderSettings, IStandardComponent, IConfigurableComponent
    {
        public FlacEncoderSettings()
        {
            var directory = Path.GetDirectoryName(
                typeof(FlacEncoderSettings).Assembly.Location
            );
            this.Executable = Path.Combine(directory, "Encoders\\flac.exe");
        }

        public override string Name
        {
            get
            {
                return "FLAC";
            }
        }

        public override string Extension
        {
            get
            {
                return "flac";
            }
        }

        public override IBassEncoderFormat Format
        {
            get
            {
                return new BassEncoderFormat(this.Depth);
            }
        }

        public int Depth { get; private set; }

        public int Compression { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                FlacEncoderSettingsConfiguration.SECTION,
                FlacEncoderSettingsConfiguration.DEPTH_ELEMENT
            ).ConnectValue(option => this.Depth = FlacEncoderSettingsConfiguration.GetDepth(option));
            core.Components.Configuration.GetElement<IntegerConfigurationElement>(
                FlacEncoderSettingsConfiguration.SECTION,
                FlacEncoderSettingsConfiguration.COMPRESSION_ELEMENT
            ).ConnectValue(value => this.Compression = value);
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
                "--no-seektable --force-raw-format --endian=little --sign=signed --sample-rate={0} --channels={1} --bps={2} --input-size={3} - --compression-level-{4} -o \"{5}\"",
                channelInfo.Frequency,
                channelInfo.Channels,
                this.GetDepth(encoderItem, stream),
                this.GetLength(encoderItem, stream),
                this.Compression,
                encoderItem.OutputFileName
            );
        }

        public override int GetDepth(EncoderItem encoderItem, IBassStream stream)
        {
            var depth = base.GetDepth(encoderItem, stream);
            if (depth < DEPTH_16)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Requsted bit depth {0} is invalid, using minimum: 16 bit", depth);
                return DEPTH_16;
            }
            if (depth > DEPTH_24)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Requsted bit depth {0} is invalid, using maximum: 24 bit", depth);
                return DEPTH_24;
            }
            return depth;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return FlacEncoderSettingsConfiguration.GetConfigurationSections(this);
        }
    }
}
