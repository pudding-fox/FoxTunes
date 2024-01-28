using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("F2F587A5-489B-429F-9C65-E60E7384D50B", ComponentSlots.Output)]
    public class CSCoreOutput : Output, IConfigurableComponent
    {
        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public ISoundOutFactory SoundOutFactory { get; private set; }

        protected virtual ISoundOutFactory GetSoundOutFactory(string id)
        {
            switch (id)
            {
                case CSCoreOutputConfiguration.WASAPI_OPTION:
                    return new WasapiSoundOutFactory();
            }
            return new DirectSoundOutFactory();
        }

        public IWaveSourceFactory WaveSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            {
                var element = this.Configuration.GetElement<SelectionConfigurationElement>(
                   CSCoreOutputConfiguration.OUTPUT_SECTION,
                   CSCoreOutputConfiguration.BACKEND_ELEMENT
                );
                element.ConnectValue<string>(id => this.SoundOutFactory = this.GetSoundOutFactory(id));
            }
            this.WaveSourceFactory = new NativeWaveSourceFactory();
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            return this.WaveSourceFactory.IsSupported(fileName);
        }

        public override Task<IOutputStream> Load(PlaylistItem playlistItem)
        {
            Logger.Write(this, LogLevel.Debug, "Loading output stream from playlist item: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            var waveSource = this.WaveSourceFactory.CreateWaveSource(playlistItem.FileName);
            Logger.Write(this, LogLevel.Debug, "Using wave source: {0}", waveSource.GetType().Name);
            var soundOut = this.SoundOutFactory.CreateSoundOut();
            Logger.Write(this, LogLevel.Debug, "Using sound out: {0}", soundOut.GetType().Name);
            var outputStream = new CSCoreOutputStream(playlistItem, waveSource, soundOut);
            outputStream.InitializeComponent(this.Core);
            Logger.Write(this, LogLevel.Debug, "Loaded output stream: {0}", outputStream.Description);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        public override Task Unload(IOutputStream stream)
        {
            Logger.Write(this, LogLevel.Debug, "Unloading output stream for playlist item: {0} => {1}", stream.Id, stream.FileName);
            if (!stream.IsStopped)
            {
                Logger.Write(this, LogLevel.Debug, "Stopping output stream for playlist item: {0} => {1}", stream.Id, stream.FileName);
                stream.Stop();
            }
            Logger.Write(this, LogLevel.Debug, "Disposing output stream for playlist item: {0} => {1}", stream.Id, stream.FileName);
            stream.Dispose();
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return CSCoreOutputConfiguration.GetConfigurationSections();
        }
    }
}
