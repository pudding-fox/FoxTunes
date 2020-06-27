using FoxTunes.Interfaces;

namespace FoxTunes
{
    [Component("679D9459-BBCE-4D95-BB65-DD20C335719C", ComponentSlots.MetaData, @default: true)]
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
