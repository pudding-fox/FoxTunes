using System.IO;

namespace FoxTunes
{
    [BassEncoder(NAME)]
    public class FlacEncoderSettings : BassEncoderSettings
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(FlacEncoderSettings).Assembly.Location);
            }
        }

        public const string NAME = "FLAC";

        public FlacEncoderSettings()
        {
            this.Executable = Path.Combine(Location, "Encoders\\flac.exe");
        }

        public override string GetArguments(int rate, int channels, long length)
        {
            return string.Format(
                "--silent --force-raw-format --endian=little --sign=signed --sample-rate={0} --bps={1} --channels={2} --input-size={3} -",
                rate,
                this.Format.Depth,
                channels,
                length
            );
        }
    }
}
