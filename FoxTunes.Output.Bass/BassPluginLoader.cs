using FoxTunes.Interfaces;
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

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static object SyncRoot = new object();

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

        public BassPluginLoader()
        {
            this.Plugins = new HashSet<BassPlugin>();
        }

        public bool IsLoaded { get; private set; }

        public HashSet<BassPlugin> Plugins { get; private set; }

        public void Load()
        {
            if (this.IsLoaded)
            {
                return;
            }
            var directoryName = Path.Combine(Location, DIRECTORY_NAME_ADDON);
            if (Directory.Exists(directoryName))
            {
                foreach (var fileName in Directory.EnumerateFiles(directoryName, FILE_NAME_MASK))
                {
                    try
                    {
                        this.Load(fileName);
                    }
                    catch
                    {
                        //TODO: Warn.
                    }
                }
            }
            this.IsLoaded = true;
        }

        public void Load(string fileName)
        {
            var result = Bass.PluginLoad(fileName);
            if (result == 0)
            {
                Logger.Write(typeof(BassPluginLoader), LogLevel.Warn, "Failed to load plugin: {0}", fileName);
                return;
            }
            var info = Bass.PluginGetInfo(result);
            Logger.Write(typeof(BassPluginLoader), LogLevel.Debug, "Plugin loaded \"{0}\": {1}", fileName, info.Version);
            this.Plugins.Add(new BassPlugin(
                fileName,
                info
            ));
        }

        public static readonly BassPluginLoader Instance = new BassPluginLoader();

        public class BassPlugin : IEquatable<BassPlugin>
        {
            public BassPlugin(string fileName, PluginInfo info)
            {
                this.FileName = fileName;
                this.Info = info;
            }

            public string FileName { get; private set; }

            public PluginInfo Info { get; private set; }

            public override int GetHashCode()
            {
                var hashCode = default(int);
                if (!string.IsNullOrEmpty(this.FileName))
                {
                    hashCode += this.FileName.ToLower().GetHashCode();
                }
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as BassPlugin);
            }

            public bool Equals(BassPlugin other)
            {
                if (other == null)
                {
                    return false;
                }
                if (object.ReferenceEquals(this, other))
                {
                    return true;
                }
                if (!string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return true;
            }

            public static bool operator ==(BassPlugin a, BassPlugin b)
            {
                if ((object)a == null && (object)b == null)
                {
                    return true;
                }
                if ((object)a == null || (object)b == null)
                {
                    return false;
                }
                if (object.ReferenceEquals((object)a, (object)b))
                {
                    return true;
                }
                return a.Equals(b);
            }

            public static bool operator !=(BassPlugin a, BassPlugin b)
            {
                return !(a == b);
            }
        }
    }
}
