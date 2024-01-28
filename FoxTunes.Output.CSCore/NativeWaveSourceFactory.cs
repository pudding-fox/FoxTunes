using CSCore;
using CSCore.Codecs;
using System;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class NativeWaveSourceFactory : WaveSourceFactory
    {
        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            extension = extension.Substring(1);
            return CodecFactory.Instance
                .GetSupportedFileExtensions()
                .Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public override IWaveSource CreateWaveSource(string fileName)
        {
            return CodecFactory.Instance.GetCodec(fileName);
        }
    }
}
