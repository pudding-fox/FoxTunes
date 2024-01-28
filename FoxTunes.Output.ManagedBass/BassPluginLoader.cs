using ManagedBass;
using System.Collections.Generic;
using System.IO;

namespace FoxTunes
{
    public class BassPluginLoader
    {
        public const string DIRECTORY_NAME_ADDON = "Addon";

        public const string FILE_NAME_MASK = "bass*.dll";

        public static readonly string[] Blacklist = new[]
        {
            "basscd.dll",
            "bassenc.dll",
            "bassenc_ogg.dll",
            "bassenc_opus.dll",
            "basshls.dll",
            "bassmidi.dll",
            "bassopus.dll",
            "basswma.dll",
            "basswv.dll",
            "basszxtune.dll",
            "bass_aac.dll",
            "bass_ac3.dll",
            "bass_adx.dll",
            "bass_ape.dll",
            "bass_mpc.dll",
            "bass_ofr.dll",
            "bass_spx.dll",
            "bass_tta.dll",
            "bass_winamp.dll"
        };

        public bool IsLoaded { get; private set; }

        public IEnumerable<PluginInfo> Plugins { get; private set; }

        public void Load()
        {
            if (this.IsLoaded)
            {
                return;
            }
            var plugins = new List<PluginInfo>();
            var directoryName = Path.Combine(ComponentScanner.Instance.Location, DIRECTORY_NAME_ADDON);
            if (!Directory.Exists(directoryName))
            {
                return;
            }
            foreach (var fileName in Directory.EnumerateFiles(directoryName, FILE_NAME_MASK))
            {
                if (Blacklist.Contains(Path.GetFileName(fileName), true))
                {
                    continue;
                }
                var result = Bass.PluginLoad(fileName);
                if (result == 0)
                {
                    continue;
                }
                var info = Bass.PluginGetInfo(result);
                plugins.Add(info);
            }
            this.Plugins = plugins;
            this.IsLoaded = true;
        }

        public static readonly BassPluginLoader Instance = new BassPluginLoader();
    }
}
