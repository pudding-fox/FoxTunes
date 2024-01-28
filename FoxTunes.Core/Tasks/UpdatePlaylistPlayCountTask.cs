using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class UpdatePlaylistPlayCountTask : PlaylistTaskBase
    {
        public UpdatePlaylistPlayCountTask(PlaylistItem playlistItem)
        {
            this.PlaylistItem = playlistItem;
        }

        public PlaylistItem PlaylistItem { get; private set; }

        public IMetaDataManager MetaDataManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.Configuration = core.Components.Configuration;
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override async Task OnRun()
        {
            var metaDataItem = this.PlaylistItem.MetaDatas.FirstOrDefault(
                _metaDataItem => string.Equals(_metaDataItem.Name, CommonMetaData.PlayCount, StringComparison.OrdinalIgnoreCase)
            );
            if (metaDataItem == null)
            {
                metaDataItem = new MetaDataItem(CommonMetaData.PlayCount, MetaDataItemType.Tag);
                this.PlaylistItem.MetaDatas.Add(metaDataItem);
            }
            var playCount = default(int);
            if (!string.IsNullOrEmpty(metaDataItem.Value) && int.TryParse(metaDataItem.Value, out playCount))
            {
                metaDataItem.Value = (playCount + 1).ToString();
            }
            else
            {
                metaDataItem.Value = "1";
            }
            var write = MetaDataBehaviourConfiguration.GetWriteBehaviour(
                this.Write.Value
            ).HasFlag(WriteBehaviour.Statistics);
            await this.MetaDataManager.Save(
                new[] { this.PlaylistItem },
                write,
                CommonMetaData.PlayCount
            ).ConfigureAwait(false);
        }
    }
}
