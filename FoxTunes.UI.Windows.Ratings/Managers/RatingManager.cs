using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class RatingManager : StandardManager
    {
        public IMetaDataManager MetaDataManager { get; private set; }

        public IHierarchyManager HierarchyManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.HierarchyManager = core.Managers.Hierarchy;
            base.InitializeComponent(core);
        }

        public async Task SetRating(IEnumerable<IFileData> fileDatas, byte rating)
        {
            foreach (var fileData in fileDatas)
            {
                lock (fileData.MetaDatas)
                {
                    fileData.AddOrUpdate(CommonStatistics.Rating, MetaDataItemType.Tag, Convert.ToString(rating));
                }
            }
            await this.MetaDataManager.Save(fileDatas, true, false, CommonStatistics.Rating).ConfigureAwait(false);
            await this.HierarchyManager.Refresh(fileDatas, CommonStatistics.Rating).ConfigureAwait(false);
        }
    }
}
