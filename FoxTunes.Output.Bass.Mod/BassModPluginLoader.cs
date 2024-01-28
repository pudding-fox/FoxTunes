using System;
using System.IO;

namespace FoxTunes
{
    public class BassModPluginLoader
    {
        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassModPluginLoader).Assembly.Location);
            }
        }

        public BassModPluginLoader()
        {

        }

        public bool IsLoaded { get; private set; }

        public void Load()
        {
            if (this.IsLoaded)
            {
                return;
            }
            if (string.Equals(Location, BassPluginLoader.Location, StringComparison.OrdinalIgnoreCase))
            {
                //We have the same location as the base plugin loaded, nothing to do.
                this.IsLoaded = true;
                return;
            }
            var directoryName = Path.Combine(Location, BassPluginLoader.DIRECTORY_NAME_ADDON);
            if (Directory.Exists(directoryName))
            {
                foreach (var fileName in Directory.EnumerateFiles(directoryName, BassPluginLoader.FILE_NAME_MASK))
                {
                    try
                    {
                        BassPluginLoader.Instance.Load(fileName);
                    }
                    catch
                    {
                        //TODO: Warn.
                    }
                }
            }
            this.IsLoaded = true;
        }

        public static readonly BassModPluginLoader Instance = new BassModPluginLoader();
    }
}
