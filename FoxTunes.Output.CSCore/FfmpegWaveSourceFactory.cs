using CSCore;
using CSCore.Ffmpeg;
using System;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class FfmpegWaveSourceFactory : WaveSourceFactory
    {
        static FfmpegWaveSourceFactory()
        {
            FfmpegUtils.LogToDefaultLogger = false;
            FfmpegUtils.FfmpegLogReceived += (sender, e) => Logger.Write(typeof(FfmpegWaveSourceFactory), Interfaces.LogLevel.Trace, e.Message);
        }

        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            extension = extension.Substring(1);
            return FfmpegUtils
                .GetInputFormats()
                .Any(format => format.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }

        public override IWaveSource CreateWaveSource(string fileName)
        {
            return new FfmpegDecoder(fileName);
        }
    }
}
