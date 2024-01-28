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
            return CodecFactory.Instance
                .GetSupportedFileExtensions()
                .Contains(extension.Substring(1), StringComparer.OrdinalIgnoreCase);
        }

        public override IWaveSource CreateWaveSource(string fileName)
        {
            return CodecFactory.Instance.GetCodec(fileName);
        }
    }
}
