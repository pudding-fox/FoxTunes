using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class RawProfile : BaseComponent, IBassEncoderSettings
    {
        public const string PCM16 = "PCM16";

        public static readonly string[] RawProfiles = new[]
        {
            PCM16
        };

        public RawProfile(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public string Executable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Directory
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Extension
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IBassEncoderFormat Format
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public long GetLength(EncoderItem encoderItem, IBassStream stream)
        {
            throw new NotImplementedException();
        }

        public int GetDepth(EncoderItem encoderItem, IBassStream stream)
        {
            throw new NotImplementedException();
        }

        public string GetArguments(EncoderItem encoderItem, IBassStream stream)
        {
            throw new NotImplementedException();
        }

        public static bool IsRawProfile(string name)
        {
            return RawProfiles.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public static void CopyTo(ChannelReader channelReader, FileStream fileStream, IBassEncoderSettings settings, CancellationToken cancellationToken)
        {

        }
    }
}
