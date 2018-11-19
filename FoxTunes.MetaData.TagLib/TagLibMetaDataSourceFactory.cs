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

        public override IMetaDataSource Create()
        {
            var source = new TagLibMetaDataSource();
            source.InitializeComponent(this.Core);
            return source;
        }
    }
}
