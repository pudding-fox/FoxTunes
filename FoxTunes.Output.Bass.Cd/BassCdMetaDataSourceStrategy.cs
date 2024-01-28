using FoxTunes.Interfaces;
using ManagedBass.Cd;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassCdMetaDataSourceStrategy : BaseComponent, IBassCdMetaDataSourceStrategy
    {
        public BassCdMetaDataSourceStrategy(int drive)
        {
            this.Drive = drive;
        }

        public int Drive { get; private set; }

        public virtual bool InitializeComponent()
        {
            return true;
        }

        public virtual IEnumerable<MetaDataItem> GetMetaDatas(int track)
        {
            yield return new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag)
            {
                NumericValue = track + 1
            };
            yield return new MetaDataItem(CommonMetaData.Title, MetaDataItemType.Tag)
            {
                TextValue = string.Format("CD Track {0:00}", track + 1)
            };
        }

        public virtual IEnumerable<MetaDataItem> GetProperties(int track)
        {
            yield return new MetaDataItem(CommonProperties.Duration, MetaDataItemType.Property)
            {
                NumericValue = (BassCd.GetTrackLength(this.Drive, track) / 176400) * 1000
            };
        }
    }
}
