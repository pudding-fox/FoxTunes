using ManagedBass;
using ManagedBass.Fx;
using ManagedBass.Mix;
using System;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public class BassPluginLoader
    {
        public const string DIRECTORY_NAME_ADDON = "Addon";

        public const string FILE_NAME_MASK = "bass*.dll";

        public static readonly Version FxVersion;

        public static readonly Version MixVersion;

        static BassPluginLoader()
        {
            FxVersion = BassFx.Version;
#if NET40
            MixVersion = BassMix.Version;
#else
            //TODO: Why is BassMix.Version not available?
            MixVersion = default(Version);
#endif
        }

        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(BassPluginLoader).Assembly.Location);
            }
        }

        public bool IsLoaded { get; private set; }

        public IEnumerable<PluginInfo> Plugins { get; private set; }

        public void Load()
        {
            if (this.IsLoaded)
            {
                return;
            }
            var plugins = new List<PluginInfo>();
            var directoryName = Path.Combine(Location, DIRECTORY_NAME_ADDON);
            if (Directory.Exists(directoryName))
            {
                foreach (var fileName in Directory.EnumerateFiles(directoryName, FILE_NAME_MASK))
                {
                    var result = Bass.PluginLoad(fileName);
                    if (result == 0)
                    {
                        continue;
                    }
                    var info = Bass.PluginGetInfo(result);
                    plugins.Add(info);
                }
            }
            this.Plugins = plugins;
            this.IsLoaded = true;
        }

        public static readonly BassPluginLoader Instance = new BassPluginLoader();
    }
}
