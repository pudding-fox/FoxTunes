using FoxTunes.Interfaces;
using ManagedBass;
using System;

namespace FoxTunes
{
    public static class ResamplerFactory
    {
        public static Resampler Create(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            var inputFormat = GetInputFormat(encoderItem, stream, settings);
            var outputFormat = GetOutputFormat(encoderItem, stream, settings);
            return new Resampler(inputFormat, outputFormat);
        }

        public static ResamplerFormat GetInputFormat(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            //I think all supported platforms are little endian.
            var encoding = ResamplerEncoding.EndianLittle;
            var depth = default(int);
            if (channelInfo.Flags.HasFlag(BassFlags.Float))
            {
                encoding |= ResamplerEncoding.FloatingPoint;
                depth = 32;
            }
            else
            {
                encoding |= ResamplerEncoding.SignedInteger;
                depth = 16;
            }
            var rate = channelInfo.Frequency;
            var channels = channelInfo.Channels;
            return new ResamplerFormat(encoding, rate, depth, channels);
        }

        public static ResamplerFormat GetOutputFormat(EncoderItem encoderItem, IBassStream stream, IBassEncoderSettings settings)
        {
            var channelInfo = default(ChannelInfo);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            //I think all supported platforms are little endian;
            //TODO: Encoders should expose more format info.
            var encoding = ResamplerEncoding.EndianLittle | ResamplerEncoding.SignedInteger;
            var depth = settings.GetDepth(encoderItem, stream);
            var rate = settings.GetRate(encoderItem, stream);
            var channels = channelInfo.Channels;
            return new ResamplerFormat(encoding, rate, depth, channels);
        }
    }
}
