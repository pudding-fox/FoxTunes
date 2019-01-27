using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassCdStreamProvider : BassStreamProvider
    {
        public const string SCHEME = "cda";

        public override byte Priority
        {
            get
            {
                return PRIORITY_HIGH;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            var drive = default(int);
            var track = default(int);
            return ParseUrl(playlistItem.FileName, out drive, out track);
        }

        public override Task<int> CreateStream(PlaylistItem playlistItem)
        {
            var drive = default(int);
            var track = default(int);
            if (!ParseUrl(playlistItem.FileName, out drive, out track))
            {
#if NET40
                return TaskEx.FromResult(0);
#else
                return Task.FromResult(0);
#endif
            }
            var channelHandle = default(int);
            if (this.GetCurrentStream(drive, track, out channelHandle))
            {
#if NET40
                return TaskEx.FromResult(channelHandle);
#else
                return Task.FromResult(channelHandle);
#endif
            }
            var flags = BassFlags.Decode;
            if (this.Output.Float)
            {
                flags |= BassFlags.Float;
            }
            if (this.Output.PlayFromMemory)
            {
                Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
            }
#if NET40
            return TaskEx.FromResult(BassCd.CreateStream(drive, track, flags));
#else
            return Task.FromResult(BassCd.CreateStream(drive, track, flags));
#endif
        }

        protected virtual bool GetCurrentStream(int drive, int track, out int channelHandle)
        {
            if (this.Output.IsStarted)
            {
                var enqueuedChannelHandle = default(int);
                this.PipelineManager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        using (var sequence = pipeline.Input.Queue.GetEnumerator())
                        {
                            while (sequence.MoveNext())
                            {
                                if (BassCd.StreamGetTrack(sequence.Current) == track)
                                {
                                    enqueuedChannelHandle = sequence.Current;
                                    break;
                                }
                            }
                        }
                    }
                });
                if (enqueuedChannelHandle != 0)
                {
                    channelHandle = enqueuedChannelHandle;
                    return true;
                }
            }
            channelHandle = 0;
            return false;
        }

        public static string CreateUrl(int drive, int track)
        {
            return string.Format("{0}://{1}/{2}", SCHEME, drive, track);
        }

        public static bool ParseUrl(string url, out int drive, out int track)
        {
            return ParseUrl(new Uri(url), out drive, out track);
        }

        public static bool ParseUrl(Uri url, out int drive, out int track)
        {
            drive = default(int);
            track = default(int);
            if (!string.Equals(url.Scheme, SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return
                int.TryParse(url.GetComponents(UriComponents.Host, UriFormat.Unescaped), out drive) &&
                int.TryParse(url.GetComponents(UriComponents.Path, UriFormat.Unescaped), out track);
        }
    }
}
