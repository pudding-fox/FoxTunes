using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FoxTunes
{
    public class Program
    {
        const string DISTRIBUTION = "distribution";

        const string NIGHTLY = "nightly";

        const string DEFAULT = "main";

        const string MINIMAL = "minimal";

        const string PLUGINS = "plugins";

        const string X86 = "x86";

        const string X64 = "x64";

        const string NET40 = "net40";

        const string NET462 = "net462";

        const string LIB = "lib";

        const string L10N_FR = "fr";

        public static Package Launcher = GetLauncher();

        public static Package Core = GetCore();

        public static Package[] Plugins = GetPlugins();

        public static void Main(string[] args)
        {
            var version = string.Join(string.Empty, args);
            if (string.IsNullOrEmpty(version))
            {
                version = string.Format("{0}-{1}", DateTime.UtcNow.ToString("ddmmyyyy"), NIGHTLY);
            }
            Console.WriteLine("Version: {0}", version);
            CreateRelease(version, ReleaseFlags.FrameworkNET40 | ReleaseFlags.PlatformX86 | ReleaseFlags.L10N_FR);
            CreateRelease(version, ReleaseFlags.FrameworkNET40 | ReleaseFlags.PlatformX64 | ReleaseFlags.L10N_FR);
            CreateRelease(version, ReleaseFlags.FrameworkNET462 | ReleaseFlags.PlatformX86 | ReleaseFlags.L10N_FR);
            CreateRelease(version, ReleaseFlags.FrameworkNET462 | ReleaseFlags.PlatformX64 | ReleaseFlags.L10N_FR);
        }

        private static void CreateRelease(string version, ReleaseFlags flags)
        {
            var target = GetTarget(flags);
            var source = GetSource(target, flags);

            if (!Directory.Exists(source))
            {
                Console.WriteLine("Source was not build: {0}", source);
                return;
            }

            Console.WriteLine("Creating release: {0}", target);
            if (Directory.Exists(target))
            {
                Console.WriteLine("Removing previous release..");
                Directory.Delete(target, true);
            }
            Directory.CreateDirectory(target);

            Console.WriteLine("Creating base files..");

            AddPackage(Path.Combine(target, DEFAULT), Launcher, flags);
            AddPackage(Path.Combine(target, MINIMAL), Launcher, flags);

            AddPackage(Path.Combine(target, DEFAULT, LIB), Core, flags);
            AddPackage(Path.Combine(target, MINIMAL, LIB), Core, flags);

            Console.WriteLine("Installing plugins..");

            foreach (var plugin in Plugins)
            {
                Console.WriteLine("Installing plugins: {0}", plugin.Name);

                if (plugin.Flags.HasFlag(PackageFlags.Default))
                {
                    AddPackage(Path.Combine(target, DEFAULT, LIB), plugin, flags);
                }

                if (plugin.Flags.HasFlag(PackageFlags.Minimal))
                {
                    AddPackage(Path.Combine(target, MINIMAL, LIB), plugin, flags);
                }

                AddPackage(Path.Combine(target, PLUGINS), plugin, flags);
            }
        }

        private static void AddPackage(string target, Package package, ReleaseFlags flags)
        {
            foreach (var element in package.Elements)
            {
                AddPackageElement(target, package, element, flags);
            }
        }

        private static void AddPackageElement(string target, Package package, PackageElement element, ReleaseFlags flags)
        {
            //Filter by framework.
            if (element.Flags.HasFlag(PackageElementFlags.FrameworkNET40) || element.Flags.HasFlag(PackageElementFlags.FrameworkNET462))
            {
                if (flags.HasFlag(ReleaseFlags.FrameworkNET40) && !element.Flags.HasFlag(PackageElementFlags.FrameworkNET40))
                {
                    return;
                }
                if (flags.HasFlag(ReleaseFlags.FrameworkNET462) && !element.Flags.HasFlag(PackageElementFlags.FrameworkNET462))
                {
                    return;
                }
            }

            //Filter by platform.
            if (element.Flags.HasFlag(PackageElementFlags.PlatformX86) || element.Flags.HasFlag(PackageElementFlags.PlatformX64))
            {
                if (flags.HasFlag(ReleaseFlags.PlatformX86) && !element.Flags.HasFlag(PackageElementFlags.PlatformX86))
                {
                    return;
                }
                if (flags.HasFlag(ReleaseFlags.PlatformX64) && !element.Flags.HasFlag(PackageElementFlags.PlatformX64))
                {
                    return;
                }
            }

            var source = GetSource(target, package, element, flags);
            var destination = GetDestination(target, package, element, flags);

            CopyFile(source, destination, flags);

            if (element.Flags.HasFlag(PackageElementFlags.LargeAddressAware))
            {
                MakeLargeAddressAware(destination, flags);
            }
        }

        private static void CopyFile(string source, string destination, ReleaseFlags flags)
        {
            var directoryName = Path.GetDirectoryName(destination);
            if (!Directory.Exists(directoryName))
            {
                Console.WriteLine("Creating directory: {0}", directoryName);
                Directory.CreateDirectory(directoryName);
            }

            Console.WriteLine("Creating file: {0}", destination);
            File.Copy(source, destination);

            CopyResouces(source, destination, flags);
        }

        private static void CopyResouces(string source, string destination, ReleaseFlags flags)
        {
            if (flags.HasFlag(ReleaseFlags.L10N_FR))
            {
                CopyResouces(source, destination, L10N_FR, flags);
            }
        }

        private static void CopyResouces(string source, string destination, string culture, ReleaseFlags flags)
        {
            var sourceDirectoryName = Path.Combine(Path.GetDirectoryName(source), culture);
            if (!Directory.Exists(sourceDirectoryName))
            {
                return;
            }
            var fileNames = Directory.GetFiles(sourceDirectoryName, string.Format("{0}.resources.dll", Path.GetFileNameWithoutExtension(source)));
            if (fileNames.Length == 0)
            {
                return;
            }
            var destinationDirectoryName = Path.Combine(Path.GetDirectoryName(destination), culture);
            if (!Directory.Exists(destinationDirectoryName))
            {
                Console.WriteLine("Creating directory: {0}", destinationDirectoryName);
                Directory.CreateDirectory(destinationDirectoryName);
            }
            foreach (var fileName in fileNames)
            {
                Console.WriteLine("Creating satellite: {0}", destination);
                File.Copy(fileName, Path.Combine(destinationDirectoryName, Path.GetFileName(fileName)));
            }
        }

        private static void MakeLargeAddressAware(string fileName, ReleaseFlags flags)
        {
            var tool = default(string);
            if (flags.HasFlag(ReleaseFlags.PlatformX86))
            {
                tool = @"..\.tools\x86\editbin.exe";
            }
            else if (flags.HasFlag(ReleaseFlags.PlatformX64))
            {
                tool = @"..\.tools\x64\editbin.exe";
            }
            else
            {
                return;
            }
            var arguments = string.Format("/largeaddressaware \"{0}\"", fileName);
            Console.WriteLine("Running tool: {0} {1}", tool, arguments);
            var info = new ProcessStartInfo(tool, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process.Start(info).WaitForExit();
        }

        private static string GetSource(string target, ReleaseFlags flags)
        {
            var parts = new List<string>()
            {
                "..",
                DISTRIBUTION
            };
            if (flags.HasFlag(ReleaseFlags.PlatformX86))
            {
                parts.Add(X86);
            }
            if (flags.HasFlag(ReleaseFlags.PlatformX64))
            {
                parts.Add(X64);
            }
            if (flags.HasFlag(ReleaseFlags.FrameworkNET40))
            {
                parts.Add(NET40);
            }
            if (flags.HasFlag(ReleaseFlags.FrameworkNET462))
            {
                parts.Add(NET462);
            }
            return Path.Combine(parts.ToArray());
        }

        private static string GetSource(string target, Package package, PackageElement element, ReleaseFlags flags)
        {
            return Path.Combine(GetSource(target, flags), element.FileName);
        }

        private static string GetDestination(string target, Package package, PackageElement element, ReleaseFlags flags)
        {
            var parts = new List<string>()
            {
                target
            };
            if (!string.IsNullOrEmpty(package.Name))
            {
                parts.Add(package.Name);
            }
            if (!string.IsNullOrEmpty(element.FolderName))
            {
                parts.Add(element.FolderName);
            }
            if (element.Flags.HasFlag(PackageElementFlags.Flatten))
            {
                parts.Add(Path.GetFileName(element.FileName));
            }
            else
            {
                parts.Add(element.FileName);
            }
            return Path.Combine(parts.ToArray());
        }


        private static string GetTarget(ReleaseFlags flags)
        {
            var parts = new List<string>();
            if (flags.HasFlag(ReleaseFlags.PlatformX86))
            {
                parts.Add(X86);
            }
            if (flags.HasFlag(ReleaseFlags.PlatformX64))
            {
                parts.Add(X64);
            }
            if (flags.HasFlag(ReleaseFlags.FrameworkNET40))
            {
                parts.Add(NET40);
            }
            if (flags.HasFlag(ReleaseFlags.FrameworkNET462))
            {
                parts.Add(NET462);
            }
            return Path.Combine(parts.ToArray());
        }

        private static Package GetLauncher()
        {
            return new Package(new PackageElement[]
            {
                new PackageElement("FoxTunes.Launcher.exe", PackageElementFlags.LargeAddressAware),
                "FoxTunes.Launcher.exe.config"
            });
        }

        private static Package GetCore()
        {
            return new Package(new PackageElement[]
            {
                "FoxDb.Core.dll",
                "FoxDb.Linq.dll",
                "FoxDb.Sql.dll",
                "FoxTunes.Core.dll",
                "FoxTunes.DB.dll",
                "FoxTunes.MetaData.dll",
                "FoxTunes.Output.dll",
                "FoxTunes.Scripting.dll",
                "FoxTunes.UI.dll",
                new PackageElement("Microsoft.Threading.Tasks.Extensions.Desktop.dll", PackageElementFlags.FrameworkNET40),
                new PackageElement("Microsoft.Threading.Tasks.Extensions.dll", PackageElementFlags.FrameworkNET40),
                new PackageElement("Microsoft.Threading.Tasks.dll", PackageElementFlags.FrameworkNET40),
                new PackageElement("Microsoft.Windows.Shell.dll", PackageElementFlags.FrameworkNET40),
                new PackageElement("System.IO.dll", PackageElementFlags.FrameworkNET40),
                new PackageElement("System.Runtime.dll",PackageElementFlags.FrameworkNET40),
                new PackageElement("System.Threading.Tasks.dll", PackageElementFlags.FrameworkNET40),
                "System.Windows.Interactivity.dll"
            });
        }

        private static Package[] GetPlugins()
        {
            return new Package[]
            {
                new Package(
                    "archive",
                    new PackageElement[]
                    {
                        "bass_zipstream.dll",
                        "FoxTunes.Output.Bass.Archive.dll",
                        "ManagedBass.ZipStream.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "asio",
                    new PackageElement[]
                    {
                        "bass_asio_handler.dll",
                        "bassasio.dll",
                        "FoxTunes.Output.Bass.Asio.dll",
                        "ManagedBass.Asio.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "bass",
                    new PackageElement[]
                    {
                        "addon/bass_aac.dll",
                        "addon/bass_ac3.dll",
                        "addon/bass_ape.dll",
                        "addon/bassalac.dll",
                        "addon/bassflac.dll",
                        "addon/bassmidi.dll",
                        "addon/bassopus.dll",
                        "addon/basswma.dll",
                        "addon/basswv.dll",
                        "bass.dll",
                        "bass_fx.dll",
                        "bassmix.dll",
                        "FoxTunes.Output.Bass.dll",
                        "ManagedBass.dll",
                        "ManagedBass.Fx.dll",
                        "ManagedBass.Gapless.dll",
                        "ManagedBass.Mix.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "cd",
                    new PackageElement[]
                    {
                        "addon/basscd.dll",
                        "FoxTunes.Output.Bass.Cd.dll",
                        "ManagedBass.Cd.dll",
                        new PackageElement("x86/bass_gapless_cd.dll", "addon", PackageElementFlags.PlatformX86 | PackageElementFlags.Flatten),
                        new PackageElement("x64/bass_gapless_cd.dll", "addon", PackageElementFlags.PlatformX64 | PackageElementFlags.Flatten)
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "conf",
                    new PackageElement[]
                    {
                        "FoxTunes.Config.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "crossfade",
                    new PackageElement[]
                    {
                        "bass_crossfade.dll",
                        "FoxTunes.Output.Bass.Crossfade.dll",
                        "ManagedBass.Crossfade.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "cue",
                    new PackageElement[]
                    {
                        "bass_substream.dll",
                        "FoxTunes.Output.Bass.Cue.dll",
                        "ManagedBass.Substream.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "directsound",
                    new PackageElement[]
                    {
                        "FoxTunes.Output.Bass.DirectSound.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "discogs",
                    new PackageElement[]
                    {
                        "FoxTunes.MetaData.Discogs.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "dsd",
                    new PackageElement[]
                    {
                        "addon/bassdsd.dll",
                        "FoxTunes.Output.Bass.Dsd.dll",
                        "ManagedBass.Dsd.dll",
                        new PackageElement("x86/bass_memory_dsd.dll", "addon", PackageElementFlags.PlatformX86 | PackageElementFlags.Flatten),
                        new PackageElement("x64/bass_memory_dsd.dll", "addon", PackageElementFlags.PlatformX64 | PackageElementFlags.Flatten)
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "dts",
                    new PackageElement[]
                    {
                        "addon/bass_dts.dll",
                        "FoxTunes.Output.Bass.Dts.dll",
                        "ManagedBass.Dts.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "encoder",
                    new PackageElement[]
                    {
                        "encoders/flac.exe",
                        "encoders/flac_license.txt",
                        "encoders/lame.exe",
                        "encoders/lame_license.txt",
                        "encoders/nsutil.dll",
                        "encoders/oggenc2.exe",
                        "encoders/opusenc.exe",
                        "encoders/opusenc_license.txt",
                        "encoders/refalac.exe",
                        "encoders/wavpack.exe",
                        "encoders/wavpack_license.txt",
                        "sox/libflac-8.dll",
                        "sox/libgcc_s_sjlj-1.dll",
                        "sox/libgomp-1.dll",
                        "sox/libid3tag-0.dll",
                        "sox/libogg-0.dll",
                        "sox/libpng16-16.dll",
                        "sox/libsox-3.dll",
                        "sox/libssp-0.dll",
                        "sox/libvorbis-0.dll",
                        "sox/libvorbisenc-2.dll",
                        "sox/libvorbisfile-3.dll",
                        "sox/libwavpack-1.dll",
                        "sox/libwinpthread-1.dll",
                        "sox/LICENSE.GPL.txt",
                        "sox/sox.exe",
                        "sox/zlib1.dll",
                        new PackageElement("FoxTunes.Encoder.Bass.exe", PackageElementFlags.LargeAddressAware),
                        "FoxTunes.Encoder.Bass.exe.config"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "eq",
                    new PackageElement[]
                    {
                        "presets/Bass.txt",
                        "presets/Flat.txt",
                        "presets/Pop.txt",
                        "presets/Rock.txt",
                        "FoxTunes.Output.Bass.ParametricEqualizer.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "gapless",
                    new PackageElement[]
                    {
                        "FoxTunes.Output.Bass.Gapless.dll",
                        new PackageElement("x86/bass_gapless.dll", "addon", PackageElementFlags.PlatformX86 | PackageElementFlags.Flatten),
                        new PackageElement("x64/bass_gapless.dll", "addon", PackageElementFlags.PlatformX64 | PackageElementFlags.Flatten)
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "js",
                    new PackageElement[]
                    {
                        "FoxTunes.Scripting.JS.dll",
                        new PackageElement("msvcp100.dll", PackageElementFlags.PlatformX86),
                        new PackageElement("msvcr100.dll", PackageElementFlags.PlatformX86),
                        new PackageElement("Noesis.Javascript.dll", PackageElementFlags.PlatformX86),
                        new PackageElement("V8.Net.dll", PackageElementFlags.PlatformX64),
                        new PackageElement("V8_Net_Proxy_x64.dll", PackageElementFlags.PlatformX64)
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "layout",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.Layout.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "librarybrowser",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.LibraryBrowser.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "logger",
                    new PackageElement[]
                    {
                        "FoxTunes.Logging.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "lyrics",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.Lyrics.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "memory",
                    new PackageElement[]
                    {
                        "FoxTunes.Output.Bass.Memory.dll",
                        "ManagedBass.Memory.dll",
                        new PackageElement("x86/bass_memory.dll", "addon", PackageElementFlags.PlatformX86 | PackageElementFlags.Flatten),
                        new PackageElement("x64/bass_memory.dll", "addon", PackageElementFlags.PlatformX64 | PackageElementFlags.Flatten)
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "minidisc",
                    new PackageElement[]
                    {
                        "FoxTunes.Minidisc.Bass.dll",
                        "MD.Net.dll",
                        "x86/atracdenc.exe",
                        "x86/gcrypt.dll",
                        "x86/himdcli.exe",
                        "x86/libgcc_s_dw2-1.dll",
                        "x86/libgcc_s_sjlj-1.dll",
                        "x86/libgcrypt-20.dll",
                        "x86/libglib-2.0-0.dll",
                        "x86/libgpg-error-0.dll",
                        "x86/libjson-c-5.dll",
                        "x86/libusb-1.0.dll",
                        "x86/libwinpthread-1.dll",
                        "x86/netmdcli.exe",
                        "x86/zlib1.dll"
                    }
                ),
                new Package(
                    "miniplayer",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.MiniPlayer.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "metadataeditor",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.MetaDataEditor.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "mod",
                    new PackageElement[]
                    {
                        "addon/basszxtune.dll",
                        "FoxTunes.Output.Bass.Mod.dll"
                    }
                ),
                new Package(
                    "ratings",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.Ratings.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "replaygain",
                    new PackageElement[]
                    {
                        "bass_replay_gain.dll",
                        new PackageElement("FoxTunes.Output.Bass.ReplayGain.exe", PackageElementFlags.LargeAddressAware),
                        "FoxTunes.Output.Bass.ReplayGain.exe.config",
                        "ManagedBass.ReplayGain.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "simplemetadata",
                    new PackageElement[]
                    {
                        "FoxTunes.MetaData.FileName.dll"
                    }
                ),
                new Package(
                    "snapping",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.Snapping.dll"
                    }
                ),
                new Package(
                    "sox",
                    new PackageElement[]
                    {
                        "bass_sox.dll",
                        "FoxTunes.Output.Bass.Resampler.dll",
                        "ManagedBass.Sox.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "spectrum",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.Spectrum.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "sqlite",
                    new PackageElement[]
                    {
                        "FoxDb.SQLite.dll",
                        "FoxTunes.DB.SQLite.dll",
                        new PackageElement("x86/SQLite.Interop.dll", PackageElementFlags.PlatformX86),
                        new PackageElement("x64/SQLite.Interop.dll", PackageElementFlags.PlatformX64),
                        "System.Data.SQLite.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "sqlserver",
                    new PackageElement[]
                    {
                        "FoxDb.SqlServer.2012.dll",
                        "FoxDb.SqlServer.dll",
                        "FoxTunes.DB.SqlServer.dll"
                    }
                ),
                new Package(
                    "statistics",
                    new PackageElement[]
                    {
                        "FoxTunes.Statistics.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "taglibmetadata",
                    new PackageElement[]
                    {
                        "FoxTunes.MetaData.TagLib.dll",
                        "taglib-sharp.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                ),
                new Package(
                    "tempo",
                    new PackageElement[]
                    {
                        "FoxTunes.Output.Bass.Tempo.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "tools",
                    new PackageElement[]
                    {
                        "FoxTunes.Tools.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "wasapi",
                    new PackageElement[]
                    {
                        "bass_wasapi_handler.dll",
                        "basswasapi.dll",
                        "FoxTunes.Output.Bass.Wasapi.dll",
                        "ManagedBass.Wasapi.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "wavebar",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.WaveBar.dll"
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "windows",
                    new PackageElement[]
                    {
                        "FoxTunes.Core.Windows.dll",
                        new PackageElement("FoxTunes.Core.Windows.UWP.dll", PackageElementFlags.FrameworkNET462)
                    },
                    PackageFlags.Default
                ),
                new Package(
                    "wpf",
                    new PackageElement[]
                    {
                        "FoxTunes.UI.Windows.dll",
                        "FoxTunes.UI.Windows.Themes.dll"
                    },
                    PackageFlags.Default | PackageFlags.Minimal
                )
            };
        }

        [Flags]
        public enum ReleaseFlags : byte
        {
            None = 0,
            FrameworkNET40 = 1,
            FrameworkNET462 = 2,
            PlatformX86 = 4,
            PlatformX64 = 8,
            L10N_FR = 16
        }

        public class Package
        {
            public Package(PackageElement[] elements, PackageFlags flags = PackageFlags.None) : this(null, elements, flags)
            {

            }

            public Package(string name, PackageElement[] elements, PackageFlags flags = PackageFlags.None)
            {
                this.Name = name;
                this.Elements = elements;
                this.Flags = flags;
            }

            public string Name { get; private set; }

            public PackageElement[] Elements { get; private set; }

            public PackageFlags Flags { get; private set; }
        }

        [Flags]
        public enum PackageFlags : byte
        {
            None = 0,
            Default = 1,
            Minimal = 2
        }

        public class PackageElement
        {
            public PackageElement(string fileName, PackageElementFlags flags = PackageElementFlags.None)
            {
                this.FileName = fileName;
                this.Flags = flags;
            }

            public PackageElement(string fileName, string folderName, PackageElementFlags flags = PackageElementFlags.None) : this(fileName, flags)
            {
                this.FolderName = folderName;
            }

            public string FileName { get; private set; }

            public string FolderName { get; private set; }

            public PackageElementFlags Flags { get; private set; }

            public static implicit operator PackageElement(string fileName)
            {
                return new PackageElement(fileName, PackageElementFlags.None);
            }
        }

        [Flags]
        public enum PackageElementFlags : byte
        {
            None = 0,
            FrameworkNET40 = 1,
            FrameworkNET462 = 2,
            PlatformX86 = 4,
            PlatformX64 = 8,
            LargeAddressAware = 16,
            Flatten = 32
        }
    }
}
