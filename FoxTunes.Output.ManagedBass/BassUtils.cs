using ManagedBass;
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
            throw new ApplicationException(Enum.GetName(typeof(Errors), Bass.LastError));
        }

        public static int GetChannelRate(int channelHandle)
        {
            return (int)Convert.ChangeType(Bass.ChannelGetAttribute(channelHandle, ChannelAttribute.Frequency), typeof(int));
        }
    }
}
