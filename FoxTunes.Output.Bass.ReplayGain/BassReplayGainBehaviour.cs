using FoxTunes.Interfaces;
using ManagedBass.Fx;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class BassReplayGainBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        //Make sure bass_fx.dll is loaded.
        private static readonly Version Version = BassFx.Version;

        public BassReplayGainBehaviour()
        {
            this.Effects = new ConditionalWeakTable<BassOutputStream, VolumeEffect>();
        }

        public ConditionalWeakTable<BassOutputStream, VolumeEffect> Effects { get; private set; }

        public ICore Core { get; private set; }

        public IBassOutput Output { get; private set; }

        public IBassStreamPipelineFactory BassStreamPipelineFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public ReplayGainMode Mode { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Output = ComponentRegistry.Instance.GetComponent<IBassOutput>();
            this.BassStreamPipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.ENABLED
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassReplayGainBehaviourConfiguration.MODE
            ).ConnectValue(option => this.Mode = BassReplayGainBehaviourConfiguration.GetMode(option));
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            if (this.Output != null)
            {
                this.Output.Loaded += this.OnLoaded;
                this.Output.Unloaded += this.OnUnloaded;
            }
            if (BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline += this.OnCreatingPipeline;
            }
        }

        public void Disable()
        {
            if (this.Output != null)
            {
                this.Output.Loaded -= this.OnLoaded;
                this.Output.Unloaded -= this.OnUnloaded;
            }
            if (BassStreamPipelineFactory != null)
            {
                this.BassStreamPipelineFactory.CreatingPipeline -= this.OnCreatingPipeline;
            }
        }

        protected virtual void OnLoaded(object sender, OutputStreamEventArgs e)
        {
            if (e.Stream is BassOutputStream stream)
            {
                this.Add(stream);
            }
        }

        protected virtual void OnUnloaded(object sender, OutputStreamEventArgs e)
        {
            if (e.Stream is BassOutputStream stream)
            {
                this.Remove(stream);
            }
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            if (BassUtils.GetChannelDsdRaw(e.Stream.ChannelHandle))
            {
                return;
            }
            var component = new BassReplayGainStreamComponent(this, e.Stream);
            component.InitializeComponent(this.Core);
            e.Components.Add(component);
        }

        protected virtual void Add(BassOutputStream stream)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                return;
            }
            var replayGain = default(float);
            var mode = default(ReplayGainMode);
            if (!this.TryGetReplayGain(stream.PlaylistItem, out replayGain, out mode))
            {
                return;
            }
            var effect = new VolumeEffect(stream, replayGain, mode);
            this.Effects.Add(stream, effect);
        }

        protected virtual void Remove(BassOutputStream stream)
        {
            var effect = default(VolumeEffect);
            if (this.Effects.TryGetValue(stream, out effect))
            {
                this.Effects.Remove(stream);
                effect.Dispose();
            }
        }

        protected virtual bool TryGetReplayGain(PlaylistItem playlistItem, out float replayGain, out ReplayGainMode mode)
        {
            var albumGain = default(float);
            var trackGain = default(float);
            lock (playlistItem.MetaDatas)
            {
                foreach (var metaDataItem in playlistItem.MetaDatas)
                {
                    if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumGain, StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(metaDataItem.Value, out albumGain))
                        {
                            if (!this.IsValidReplayGain(albumGain))
                            {
                                albumGain = default(float);
                                continue;
                            }
                            if (this.Mode == ReplayGainMode.Album)
                            {
                                Logger.Write(this, LogLevel.Debug, "Found preferred replay gain data for album:  \"{0}\" => {1}", playlistItem.FileName, albumGain);
                                mode = ReplayGainMode.Album;
                                replayGain = albumGain;
                                return true;
                            }
                        }
                    }
                    else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackGain, StringComparison.OrdinalIgnoreCase))
                    {
                        if (float.TryParse(metaDataItem.Value, out trackGain))
                        {
                            if (!this.IsValidReplayGain(trackGain))
                            {
                                trackGain = default(float);
                                continue;
                            }
                            if (this.Mode == ReplayGainMode.Track)
                            {
                                Logger.Write(this, LogLevel.Debug, "Found preferred replay gain data for track:  \"{0}\" => {1}", playlistItem.FileName, trackGain);
                                mode = ReplayGainMode.Track;
                                replayGain = trackGain;
                                return true;
                            }
                        }
                    }
                }
            }
            if (this.IsValidReplayGain(albumGain))
            {
                Logger.Write(this, LogLevel.Debug, "Using album replay gain data: \"{0}\" => {1}", playlistItem.FileName, albumGain);
                mode = ReplayGainMode.Album;
                replayGain = albumGain;
                return true;
            }
            if (this.IsValidReplayGain(trackGain))
            {
                Logger.Write(this, LogLevel.Debug, "Using track replay gain data: \"{0}\" => {1}", playlistItem.FileName, trackGain);
                mode = ReplayGainMode.Track;
                replayGain = trackGain;
                return true;
            }
            Logger.Write(this, LogLevel.Debug, "No replay gain data: \"{0}\".", playlistItem.FileName);
            mode = ReplayGainMode.None;
            replayGain = 0;
            return false;
        }

        protected virtual bool IsValidReplayGain(float replayGain)
        {
            //TODO: I'm sure there is a valid range of values.
            return replayGain != 0 && !float.IsNaN(replayGain);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassReplayGainBehaviourConfiguration.GetConfigurationSections();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Disable();
        }

        ~BassReplayGainBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
