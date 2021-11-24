using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class DiscogsFrontCoverMetaDataSource : StandardComponent, IOnDemandMetaDataSource
    {
        public DiscogsBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<DiscogsBehaviour>();
            base.InitializeComponent(core);
        }

        public bool Enabled
        {
            get
            {
                return this.Behaviour.Enabled.Value && this.Behaviour.AutoLookup.Value;
            }
        }

        public string Name
        {
            get
            {
                return FetchArtworkTask.FRONT_COVER;
            }
        }

        public MetaDataItemType Type
        {
            get
            {
                return MetaDataItemType.Image;
            }
        }

        public async Task<OnDemandMetaDataValues> GetValues(IEnumerable<IFileData> fileDatas, object state)
        {
            var releaseLookups = await this.Behaviour.FetchArtwork(fileDatas).ConfigureAwait(false);
            return this.Behaviour.GetMetaDataValues(releaseLookups);
        }
    }
}
