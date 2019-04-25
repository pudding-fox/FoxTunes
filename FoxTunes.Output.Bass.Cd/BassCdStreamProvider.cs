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
            var id = default(string);
            var track = default(int);
            return ParseUrl(playlistItem.FileName, out drive, out id, out track);
        }

        public override Task<int> CreateStream(PlaylistItem playlistItem, BassFlags flags)
        {
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            if (!ParseUrl(playlistItem.FileName, out drive, out id, out track))
            {
#if NET40
                return TaskEx.FromResult(0);
#else
                return Task.FromResult(0);
#endif
            }
            this.AssertDiscId(drive, id);
            var channelHandle = default(int);
            if (this.GetCurrentStream(drive, track, out channelHandle))
            {
#if NET40
                return TaskEx.FromResult(channelHandle);
#else
                return Task.FromResult(channelHandle);
#endif
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

        protected virtual void AssertDiscId(int drive, string expected)
        {
            var actual = BassCd.GetID(drive, CDID.CDPlayer);
            if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            throw new InvalidOperationException(string.Format("Found disc with identifier \"{0}\" when \"{1}\" was required.", actual, expected));
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

        public static string CreateUrl(int drive, string id, int track)
        {
            return string.Format("{0}://{1}/{2}?{3}", SCHEME, drive, id, track);
        }

        public static bool ParseUrl(string url, out int drive, out string id, out int track)
        {
            return ParseUrl(new Uri(url), out drive, out id, out track);
        }

        public static bool ParseUrl(Uri url, out int drive, out string id, out int track)
        {
            drive = default(int);
            id = default(string);
            track = default(int);
            if (!string.Equals(url.Scheme, SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return
                int.TryParse(url.GetComponents(UriComponents.Host, UriFormat.Unescaped), out drive) &&
                !string.IsNullOrEmpty(id = url.GetComponents(UriComponents.Path, UriFormat.Unescaped)) &&
                int.TryParse(url.GetComponents(UriComponents.Query, UriFormat.Unescaped), out track);
        }
    }
}
