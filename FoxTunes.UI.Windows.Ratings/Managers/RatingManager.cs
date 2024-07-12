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

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
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
            await this.MetaDataManager.Save(
                fileDatas,
                new[] { CommonStatistics.Rating },
                MetaDataUpdateType.User,
                MetaDataUpdateFlags.All
            ).ConfigureAwait(false);
        }
    }
}
