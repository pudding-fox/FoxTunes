using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("F2F587A5-489B-429F-9C65-E60E7384D50B", ComponentSlots.Output)]
    public class CSCoreOutput : Output, IConfigurableComponent
    {
        public IConfiguration Configuration { get; private set; }

        public ISoundOutFactory SoundOutFactory
        {
            get
            {
                var element = this.Configuration.GetElement<SelectionConfigurationElement>(
                    CSCoreOutputConfiguration.OUTPUT_SECTION,
                    CSCoreOutputConfiguration.BACKEND_ELEMENT
                );
                if (element != null && element.SelectedOption != null)
                {
                    switch (element.SelectedOption.Id)
                    {
                        case CSCoreOutputConfiguration.WASAPI_OPTION:
                            return new WasapiSoundOutFactory();
                    }
                }
                return new DirectSoundOutFactory();
            }
        }

        public IWaveSourceFactory WaveSourceFactory
        {
            get
            {
                var element = this.Configuration.GetElement<SelectionConfigurationElement>(
                    CSCoreOutputConfiguration.OUTPUT_SECTION,
                    CSCoreOutputConfiguration.DECODER_ELEMENT
                );
                if (element != null && element.SelectedOption != null)
                {
                    switch (element.SelectedOption.Id)
                    {
                        case CSCoreOutputConfiguration.FFMPEG_OPTION:
                            return new FfmpegWaveSourceFactory();
                    }
                }
                return new NativeWaveSourceFactory();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            return this.WaveSourceFactory.IsSupported(fileName);
        }

        public override IOutputStream Load(string fileName)
        {
            var waveSource = this.WaveSourceFactory.CreateWaveSource(fileName);
            var soundOut = this.SoundOutFactory.CreateSoundOut();
            return new CSCoreOutputStream(fileName, waveSource, soundOut);
        }

        public override void Unload(IOutputStream stream)
        {
            if (!stream.IsStopped)
            {
                stream.Stop();
            }
            stream.Dispose();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return CSCoreOutputConfiguration.GetConfigurationSections();
        }
    }
}
