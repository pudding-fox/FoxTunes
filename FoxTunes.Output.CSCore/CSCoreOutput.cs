using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("F2F587A5-489B-429F-9C65-E60E7384D50B", ComponentSlots.Output)]
    public class CSCoreOutput : Output, IConfigurableComponent
    {
        public ISoundOutFactory SoundOutFactory { get; private set; }

        public IWaveSourceFactory WaveSourceFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.SoundOutFactory = new DirectSoundOutFactory();
            this.WaveSourceFactory = new FfmpegWaveSourceFactory();
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
            yield return new ConfigurationSection("B60E0AFE-9B14-4919-A88E-F810F037FFA0", "Output")
                .WithElement(
                    new SelectionConfigurationElement("88DAB8DB-E99A-44E9-B190-730F05A51B01", "Backend")
                        .WithOption(new SelectionConfigurationOption("950D696C-CBFF-44DC-AEFD-20664C2F13D3", "DirectSound"), true)
                        .WithOption(new SelectionConfigurationOption("C83680D5-D1F7-491E-9096-2BDF5BB1AF5C", "Wasapi")))
                .WithElement(
                    new SelectionConfigurationElement("3928EE4A-65A8-4ADA-B8B2-0C4D89C47027", "Decoder")
                        .WithOption(new SelectionConfigurationOption("3AFF0EAE-FA4C-4D5C-AB3B-8C95BF4F4DA1", "Native"), true)
                        .WithOption(new SelectionConfigurationOption("3D553CB8-53F6-43D2-9205-9BC18BC7D0F6", "Ffmpeg"))
            );
        }
    }
}
