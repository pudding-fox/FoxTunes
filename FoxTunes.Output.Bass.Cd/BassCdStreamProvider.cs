using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Cd;
using System;
using System.Collections.Generic;
using System.Data.Odbc;

namespace FoxTunes
{
    public class BassCdStreamProvider : BassStreamProvider
    {
        public override BassStreamProviderFlags Flags
        {
            get
            {
                return base.Flags | BassStreamProviderFlags.Serial;
            }
        }

        public BassCdStreamProviderBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassCdStreamProviderBehaviour>();
            base.InitializeComponent(core);
        }

        public override bool CanCreateStream(PlaylistItem playlistItem)
        {
            if (this.Behaviour == null || !this.Behaviour.Enabled)
            {
                return false;
            }
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            return BassCdUtils.ParseUrl(playlistItem.FileName, out drive, out id, out track);
        }

        public override IBassStream CreateBasicStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            var fileName = this.GetFileName(playlistItem, advice);
            if (!BassCdUtils.ParseUrl(fileName, out drive, out id, out track))
            {
                //This shouldn't happen as CanCreateStream would have returned false.
                return BassStream.Empty;
            }
            this.AssertDiscId(drive, id);
            var channelHandle = default(int);
            if (this.GetCurrentStream(drive, track, out channelHandle))
            {
                return this.CreateBasicStream(channelHandle, advice);
            }
            if (BassCd.FreeOld)
            {
                Logger.Write(this, LogLevel.Debug, "Updating config: BASS_CONFIG_CD_FREEOLD = FALSE");
                BassCd.FreeOld = false;
            }
            channelHandle = BassCd.CreateStream(drive, track, flags);
            return this.CreateBasicStream(channelHandle, advice);
        }

        public override IBassStream CreateInteractiveStream(PlaylistItem playlistItem, IEnumerable<IBassStreamAdvice> advice, BassFlags flags)
        {
            var drive = default(int);
            var id = default(string);
            var track = default(int);
            var fileName = this.GetFileName(playlistItem, advice);
            if (!BassCdUtils.ParseUrl(fileName, out drive, out id, out track))
            {
                //This shouldn't happen as CanCreateStream would have returned false.
                return BassStream.Empty;
            }
            this.AssertDiscId(drive, id);
            var channelHandle = default(int);
            if (this.GetCurrentStream(drive, track, out channelHandle))
            {
                return this.CreateInteractiveStream(channelHandle, advice);
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
            return this.CreateInteractiveStream(channelHandle, advice);
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
    }
}
