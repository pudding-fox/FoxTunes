using CSCore.Codecs;
using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    [Component("F2F587A5-489B-429F-9C65-E60E7384D50B", ComponentSlots.Output)]
    public class CSCoreOutput : Output
    {
        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName).Substring(1); //Why is the dot included?
            return CodecFactory.Instance
                .GetSupportedFileExtensions()
                .Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public override IOutputStream Load(string fileName)
        {
            return new CSCoreOutputStream(fileName);
        }

        public override void Unload(IOutputStream stream)
        {
            if (!stream.IsStopped)
            {
                stream.Stop();
            }
            stream.Dispose();
        }
    }
}
