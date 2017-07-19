using CSCore;
using CSCore.Ffmpeg;
using System.Linq;
using System.IO;

namespace FoxTunes
{
    public class FfmpegWaveSourceFactory : WaveSourceFactory
    {
        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName).Substring(1); //Why is the dot included?
            return FfmpegUtils
                .GetInputFormats()
                .Any(format => format.FileExtensions.Contains(extension));
        }

        public override IWaveSource CreateWaveSource(string fileName)
        {
            return new FfmpegDecoder(fileName);
        }
    }
}
