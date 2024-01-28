﻿using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class BassUtils
    {
        private static readonly string[] BUILT_IN_FORMATS = new[]
        {
            "mp1", "mp2", "mp3", "ogg", "wav", "aif"
        };

        public static IEnumerable<string> GetInputFormats()
        {
            foreach (var format in BUILT_IN_FORMATS)
            {
                yield return format;
            }
            foreach (var plugin in BassPluginLoader.Instance.Plugins)
            {
                foreach (var format in plugin.Formats)
                {
                    foreach (var extension in format.FileExtensions.Split(';'))
                    {
                        yield return extension.TrimStart('*', '.');
                    }
                }
            }
        }

        public static bool OK(bool result)
        {
            if (!result)
            {
                if (Bass.LastError != Errors.OK)
                {
                    Throw();
                }
            }
            return result;
        }

        public static int OK(int result)
        {
            if (result == 0)
            {
                if (Bass.LastError != Errors.OK)
                {
                    Throw();
                }
            }
            return result;
        }

        public static void Throw()
        {
            if (Bass.LastError != Errors.OK)
            {
                throw new BassException(Bass.LastError);
            }
        }

        public static bool GetChannelFloat(int channelHandle)
        {
            return GetChannelFlags(channelHandle).HasFlag(BassFlags.Float);
        }

        public static bool GetChannelDsdRaw(int channelHandle)
        {
            return GetChannelFlags(channelHandle).HasFlag(BassFlags.DSDRaw);
        }

        public static int GetChannelCount(int channelHandle)
        {
            var channelInfo = default(ChannelInfo);
            OK(Bass.ChannelGetInfo(channelHandle, out channelInfo));
            return channelInfo.Channels;
        }

        public static int GetChannelPcmRate(int channelHandle)
        {
            return Convert.ToInt32(Bass.ChannelGetAttribute(channelHandle, ChannelAttribute.Frequency));
        }

        public static int GetChannelDsdRate(int channelHandle)
        {
            return Convert.ToInt32(Bass.ChannelGetAttribute(channelHandle, ChannelAttribute.DSDRate));
        }

        public static BassFlags GetChannelFlags(int channelHandle)
        {
            var channelInfo = default(ChannelInfo);
            OK(Bass.ChannelGetInfo(channelHandle, out channelInfo));
            return channelInfo.Flags;
        }
    }

    public class BassException : Exception
    {
        public BassException(Errors error) : this(Enum.GetName(typeof(Errors), error), error)
        {

        }

        public BassException(string message, Errors error) : base(message)
        {
            this.Error = error;
        }

        public Errors Error { get; private set; }
    }
}