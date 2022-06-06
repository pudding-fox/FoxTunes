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

        public virtual bool Fetch()
        {
            return false;
        }

        public virtual IEnumerable<MetaDataItem> GetMetaDatas(int track)
        {
            yield return new MetaDataItem(CommonMetaData.Track, MetaDataItemType.Tag)
            {
                Value = (track + 1).ToString()
            };
            yield return new MetaDataItem(CommonMetaData.Title, MetaDataItemType.Tag)
            {
                Value = string.Format("CD Track {0:00}", track + 1)
            };
        }

        public virtual IEnumerable<MetaDataItem> GetProperties(int track)
        {
            yield return new MetaDataItem(CommonProperties.Duration, MetaDataItemType.Property)
            {
                //What the fuck is this? Something to do with 44.1kHz/16bit?
                Value = ((BassCd.GetTrackLength(this.Drive, track) / 176400) * 1000).ToString()
            };
        }
    }
}
