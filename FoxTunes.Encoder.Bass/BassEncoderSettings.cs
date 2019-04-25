using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public abstract class BassEncoderSettings : MarshalByRefObject, IBassEncoderSettings
    {
        public string Executable { get; protected set; }

        public virtual string Directory
        {
            get
            {
                return Path.GetDirectoryName(this.Executable);
            }
        }

        public IBassEncoderFormat Format
        {
            get
            {
                return new BassEncoderFormat()
                {
                    
                };
            }
        }

        public abstract string GetArguments(int rate,  int channels, long length);

        public virtual string GetOutput(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            return Path.Combine(directory, string.Format("{0}.flac", name));
        }

        public virtual IEnumerable<ConfigurationElement> GetConfigurationElements()
        {
            return Enumerable.Empty<ConfigurationElement>();
        }
    }
}
