using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;
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

        public override BassStreamProviderFlags Flags
        {
            get
            {
                return base.Flags | BassStreamProviderFlags.Serial;
            }
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            return ParseUrl(playlistItem.FileName, out drive, out id, out track);
        }

#if NET40
        public override Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
#else
        public override async Task<IBassStream> CreateStream(PlaylistItem playlistItem, BassFlags flags, IEnumerable<IBassStreamAdvice> advice)
#endif
        {
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            if (!ParseUrl(playlistItem.FileName, out drive, out id, out track))
            {
#if NET40
                return TaskEx.FromResult(BassStream.Empty);
#else
                return BassStream.Empty;
#endif
            }
            this.AssertDiscId(drive, id);
            var channelHandle = default(int);
#if NET40
            this.Semaphore.Wait();
#else
            await this.Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                if (this.GetCurrentStream(drive, track, out channelHandle))
                {
#if NET40
                    return TaskEx.FromResult(this.CreateStream(channelHandle, advice));
#else
                    return this.CreateStream(channelHandle, advice);
#endif
                }
                if (this.Output != null && this.Output.PlayFromMemory)
                {
                    Logger.Write(this, LogLevel.Warn, "This provider cannot play from memory.");
                }
                if (BassCd.FreeOld)
                {
                    Logger.Write(this, LogLevel.Debug, "Updating config: BASS_CONFIG_CD_FREEOLD = FALSE");
                    BassCd.FreeOld = false;
                }
                channelHandle = BassCd.CreateStream(drive, track, flags);
#if NET40
                return TaskEx.FromResult(this.CreateStream(channelHandle, advice));
#else
                return this.CreateStream(channelHandle, advice);
#endif
            }
            finally
            {
                this.Semaphore.Release();
            }
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
            if (this.Output != null && this.Output.IsStarted)
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
            return string.Format("{0}://{1}/{2}/{3}", SCHEME, id, drive, track);
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
            var host = url.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var path = url.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var parts = path.Split('/');
            return
                !string.IsNullOrEmpty(id = host) &&
                parts.Length > 0 && int.TryParse(parts[0], out drive) &&
                parts.Length > 1 && int.TryParse(parts[1], out track);
        }
    }
}
