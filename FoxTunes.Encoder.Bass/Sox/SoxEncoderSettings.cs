using ManagedBass;
using System;
using System.IO;

namespace FoxTunes
{
    public class SoxEncoderSettings : BassEncoderSettings
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(SoxEncoderSettings).Assembly.Location);
            }
        }

        private SoxEncoderSettings()
        {
            this.Executable = Path.Combine(Location, "Encoders\\sox.exe");
        }

        public SoxEncoderSettings(int depth, BassFlags flags) : this()
        {
            this.Depth = depth;
            this.Flags = flags;
        }

        public int Depth { get; private set; }

        public BassFlags Flags { get; private set; }

        public override string GetArguments(int rate, int channels, long length)
        {
            if (this.Flags.HasFlag(BassFlags.Float))
            {
                return string.Format(
                    "-t raw -e floating-point --bits 32 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {0} -c {1} -",
                    rate,
                    channels,
                    this.Depth
                );
            }
            else
            {
                return string.Format(
                    "-t raw -e signed-integer --bits 16 -r {0} -c {1} - -t raw -e signed-integer --bits {2} -r {0} -c {1} -",
                    rate,
                    channels,
                    this.Depth
                );
            }
        }
    }
}
