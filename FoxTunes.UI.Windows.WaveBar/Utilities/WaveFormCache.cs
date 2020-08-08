using FoxTunes.Interfaces;
using System;
using System.IO;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WaveFormCache : StandardComponent
    {
        const int CACHE_SIZE = 4;

        private static readonly string PREFIX = typeof(WaveFormCache).Name;

        public WaveFormCache()
        {
            this.Store = new CappedDictionary<string, WaveFormGenerator.WaveFormGeneratorData>(CACHE_SIZE, StringComparer.OrdinalIgnoreCase);
        }

        public CappedDictionary<string, WaveFormGenerator.WaveFormGeneratorData> Store { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.CACHE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public WaveFormGenerator.WaveFormGeneratorData GetOrCreate(IOutputStream stream, int resolution, Func<WaveFormGenerator.WaveFormGeneratorData> factory)
        {
            return this.Store.GetOrAdd(stream.FileName, () =>
            {
                if (this.Enabled.Value)
                {
                    var id = this.GetDataId(stream, resolution);
                    var fileName = default(string);
                    if (FileMetaDataStore.Exists(PREFIX, id, out fileName))
                    {
                        var data = default(WaveFormGenerator.WaveFormGeneratorData);
                        if (this.TryLoad(fileName, out data))
                        {
                            return data;
                        }
                    }
                }
                return factory();
            });
        }

        protected virtual bool TryLoad(string fileName, out WaveFormGenerator.WaveFormGeneratorData data)
        {
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    data = Serializer.Instance.Read(stream) as WaveFormGenerator.WaveFormGeneratorData;
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load wave form from file \"{0}\": {1}", fileName, e.Message);
            }
            data = null;
            return false;
        }

        public void Save(IOutputStream stream, WaveFormGenerator.WaveFormGeneratorData data)
        {
            if (!this.Enabled.Value)
            {
                return;
            }

            var id = this.GetDataId(stream, data.Resolution);
            this.Save(id, data);
        }

        protected virtual void Save(string id, WaveFormGenerator.WaveFormGeneratorData data)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    Serializer.Instance.Write(stream, data);
                    stream.Seek(0, SeekOrigin.Begin);
                    FileMetaDataStore.Write(PREFIX, id, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to save wave form: {0}", e.Message);
            }
        }

        private string GetDataId(IOutputStream stream, int resolution)
        {
            var hashCode = default(int);
            unchecked
            {
                hashCode = (hashCode * 29) + stream.FileName.GetHashCode();
                hashCode = (hashCode * 29) + resolution.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
        }
    }
}
