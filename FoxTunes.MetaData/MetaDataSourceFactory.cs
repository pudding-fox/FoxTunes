using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class MetaDataSourceFactory : StandardFactory, IMetaDataSourceFactory
    {
        public IConfiguration Configuration { get; private set; }

        public virtual bool Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.ENABLE_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        public abstract IMetaDataSource Create();
    }
}
