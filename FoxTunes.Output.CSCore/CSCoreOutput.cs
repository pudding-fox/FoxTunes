using FoxTunes.Interfaces;

namespace FoxTunes
{
    [Component("F2F587A5-489B-429F-9C65-E60E7384D50B", ComponentSlots.Output)]
    public class CSCoreOutput : Output
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
    }
}
