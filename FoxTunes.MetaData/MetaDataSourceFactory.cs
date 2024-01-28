using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class MetaDataSourceFactory : StandardFactory, IMetaDataSourceFactory
    {
        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public abstract IEnumerable<KeyValuePair<string, MetaDataItemType>> Supported { get; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            base.InitializeComponent(core);
        }

        public abstract IMetaDataSource Create();
    }
}
