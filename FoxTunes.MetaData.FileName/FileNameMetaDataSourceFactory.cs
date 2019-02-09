using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [Component("BDAAF3E1-84CC-4D36-A7CB-278663E65844", ComponentSlots.MetaData)]
    public class FileNameMetaDataSourceFactory : MetaDataSourceFactory, IConfigurableComponent
    {
        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IEnumerable<IFileNameMetaDataExtractor> Extractors { get; private set; }

        public string _Patterns { get; private set; }

        public string Patterns
        {
            get
            {
                return this._Patterns;
            }
            set
            {
                this._Patterns = value;
                this.OnPatternsChanged();
            }
        }

        protected virtual void OnPatternsChanged()
        {
            this.Extractors = CreateExtractors(this.Patterns);
        }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<TextConfigurationElement>(
                FileNameMetaDataSourceFactoryConfiguration.SECTION,
                FileNameMetaDataSourceFactoryConfiguration.PATTERNS_ELEMENT
            ).ConnectValue(value => this.Patterns = value);
            base.InitializeComponent(core);
        }

        public override bool Enabled
        {
            get
            {
                return this.Extractors != null && this.Extractors.Any();
            }
        }

        public override IMetaDataSource Create()
        {
            var source = new FileNameMetaDataSource(this.Extractors);
            source.InitializeComponent(this.Core);
            return source;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return FileNameMetaDataSourceFactoryConfiguration.GetConfigurationSections();
        }

        public static IEnumerable<IFileNameMetaDataExtractor> CreateExtractors(string patterns)
        {
            if (!string.IsNullOrEmpty(patterns))
            {
                foreach (var pattern in patterns.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (string.IsNullOrEmpty(pattern) || pattern.StartsWith("//"))
                    {
                        continue;
                    }
                    yield return new FileNameMetaDataExtractor(pattern);
                }
            }
        }
    }
}
