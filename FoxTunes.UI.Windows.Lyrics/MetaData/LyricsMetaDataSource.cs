using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LyricsMetaDataSource : StandardComponent, IOnDemandMetaDataSource
    {
        public LyricsBehaviour Behaviour { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement AutoLookup { get; private set; }

        public SelectionConfigurationElement AutoLookupProvider { get; private set; }

        public override void InitializeComponent(Interfaces.ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<LyricsBehaviour>();
            this.Configuration = core.Components.Configuration;
            this.AutoLookup = this.Configuration.GetElement<BooleanConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_LOOKUP
            );
            this.AutoLookupProvider = this.Configuration.GetElement<SelectionConfigurationElement>(
                LyricsBehaviourConfiguration.SECTION,
                LyricsBehaviourConfiguration.AUTO_LOOKUP_PROVIDER
            );
            base.InitializeComponent(core);
        }

        public bool Enabled
        {
            get
            {
                return this.Behaviour.Enabled.Value && this.AutoLookup.Value;
            }
        }

        public string Name
        {
            get
            {
                return CommonMetaData.Lyrics;
            }
        }

        public MetaDataItemType Type
        {
            get
            {
                return MetaDataItemType.Tag;
            }
        }

        public bool CanGetValue(IFileData fileData)
        {
            return true;
        }

        public async Task<OnDemandMetaDataValues> GetValues(IEnumerable<IFileData> fileDatas, object state)
        {
            var provider = state as LyricsProvider ?? this.GetAutoLookupProvider();
            if (provider == null)
            {
                return null;
            }
            var values = new List<OnDemandMetaDataValue>();
            foreach (var fileData in fileDatas)
            {
                Logger.Write(this, LogLevel.Debug, "Looking up lyrics for file \"{0}\"..", fileData.FileName);
                var result = await provider.Lookup(fileData).ConfigureAwait(false);
                if (!result.Success)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to look up lyrics for file \"{0}\".", fileData.FileName);
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Looking up lyrics for file \"{0}\": OK.", fileData.FileName);
                values.Add(new OnDemandMetaDataValue(fileData, result.Lyrics));
            }
            return new OnDemandMetaDataValues(values, this.Behaviour.WriteTags.Value);
        }

        protected virtual LyricsProvider GetAutoLookupProvider()
        {
            var provider = default(LyricsProvider);
            if (this.AutoLookupProvider.Value != null)
            {
                provider = this.Behaviour.Providers.FirstOrDefault(
                   _provider => string.Equals(_provider.Id, this.AutoLookupProvider.Value.Id, StringComparison.OrdinalIgnoreCase)
               );
            }
            if (provider == null)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to determine the preferred provider.");
                provider = this.Behaviour.Providers.FirstOrDefault();
            }
            if (provider == null)
            {
                Logger.Write(this, LogLevel.Warn, "No providers.");
            }
            return provider;
        }
    }
}
