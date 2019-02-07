using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public static class ResourceExtractor
    {
        public static void Extract(Type owner, IDictionary<string, string> resources)
        {
            foreach (var key in resources.Keys)
            {
                var path = Path.Combine(ComponentScanner.Instance.Location, resources[key]);
                if (File.Exists(path))
                {
                    continue;
                }
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var input = owner.Assembly.GetManifestResourceStream(key))
                {
                    using (var output = File.Create(path))
                    {
                        input.CopyTo(output);
                    }
                }
            }
        }
    }
}
