using FoxTunes.Interfaces;

namespace FoxTunes
{
    public class TagLibMetaDataSourceFactory : MetaDataSourceFactory
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            base.InitializeComponent(core);
        }

        public override IMetaDataSource Create(string fileName)
        {
            Logger.Write(this, LogLevel.Trace, "Creating meta data source: {0}", fileName);
            var source = new TagLibMetaDataSource(fileName);
            source.InitializeComponent(this.Core);
            Logger.Write(this, LogLevel.Trace, "Created meta data source: {0}, {1} tags, {2} properties", fileName, source.MetaDatas.Count, source.Properties.Count);
            return source;
        }
    }
}
