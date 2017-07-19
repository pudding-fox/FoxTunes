using System;
using CSCore;
using CSCore.Codecs;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class NativeWaveSourceFactory : WaveSourceFactory
    {
        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName).Substring(1); //Why is the dot included?
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
