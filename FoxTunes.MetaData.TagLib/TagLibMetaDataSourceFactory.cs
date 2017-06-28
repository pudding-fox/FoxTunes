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
            var source = new TagLibMetaDataSource(fileName);
            source.InitializeComponent(this.Core);
            return source;
        }
    }
}
