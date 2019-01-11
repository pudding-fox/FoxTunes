using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IFileNameMetaDataExtractor
    {
        bool Extract(string value, out IDictionary<string, string> metaData);
    }
}
