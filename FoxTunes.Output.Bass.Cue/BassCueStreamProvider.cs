using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassCueStreamProvider : BassStreamProvider
    {
        public const string SCHEME = "cue";

        public static readonly SyncProcedure EndProcedure = new SyncProcedure((Handle, Channel, Data, User) => Bass.ChannelStop(Handle));

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            var fileName = default(string);
            var position = default(string);
            var length = default(string);
            return ParseUrl(playlistItem.FileName, out fileName, out position, out length);
        }

#if NET40
        public override Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags)
#else
        public override async Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags)
#endif
        {
            var fileName = default(string);
            var position = default(string);
            var length = default(string);
            if (!ParseUrl(playlistItem.FileName, out fileName, out position, out length))
            {
#if NET40
                return TaskEx.FromResult(0);
#else
                return 0;
#endif
            }
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                if (this.Output != null && this.Output.PlayFromMemory)
                {
                    Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
                }
                var channelHandle = Bass.CreateStream(fileName, 0, 0, flags);
                if (channelHandle != 0)
                {
                    if (!string.IsNullOrEmpty(position))
                    {
                        Bass.ChannelSetPosition(
                            channelHandle,
                            Bass.ChannelSeconds2Bytes(
                                channelHandle,
                                CueSheetIndex.ToTimeSpan(position).TotalSeconds
                           )
                        );
                    }
                    if (!string.IsNullOrEmpty(length))
                    {
                        Bass.ChannelSetSync(
                            channelHandle,
                            SyncFlags.Position,
                            Bass.ChannelSeconds2Bytes(
                                channelHandle,
                                CueSheetIndex.ToTimeSpan(length).TotalSeconds
                            ),
                            EndProcedure
                        );
                    }
                }
#if NET40
                return TaskEx.FromResult(channelHandle);
#else
                return channelHandle;
#endif
            }
            finally
            {
                this.Semaphore.Release();
            }
        }
        
        public static string CreateUrl(string fileName, string position)
        {
            return string.Format(
                "{0}://{1}?position={2}",
                SCHEME,
                fileName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                position
            );
        }

        public static string CreateUrl(string fileName, string position, string length)
        {
            return string.Format(
                "{0}://{1}?position={2}&length={3}",
                SCHEME,
                fileName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                position,
                length
            );
        }

        public static bool ParseUrl(string url, out string fileName, out string position, out string length)
        {
            return ParseUrl(new Uri(url), out fileName, out position, out length);
        }

        public static bool ParseUrl(Uri url, out string fileName, out string position, out string length)
        {
            fileName = default(string);
            position = default(string);
            length = default(string);
            if (!string.Equals(url.Scheme, SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            fileName = Uri.UnescapeDataString(
                url.AbsolutePath
            ).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            position = url.GetQueryParameter("position");
            length = url.GetQueryParameter("length");
            return true;
        }
    }
}
