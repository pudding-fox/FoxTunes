using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IMetaDataSource
    {
        public LibraryItem()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
            this.Properties = new ObservableCollection<PropertyItem>();
            this.Images = new ObservableCollection<ImageItem>();
            this.Statistics = new ObservableCollection<StatisticItem>();
        }

        public LibraryItem(string fileName, IMetaDataSource metaData) : this()
        {
            this.FileName = fileName;
            this.MetaDatas = metaData.MetaDatas;
            this.Properties = metaData.Properties;
            this.Images = metaData.Images;
        }

        public string FileName { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

        public ObservableCollection<PropertyItem> Properties { get; private set; }

        public ObservableCollection<ImageItem> Images { get; private set; }

        public ObservableCollection<StatisticItem> Statistics { get; private set; }

        public bool Equals(LibraryItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            return this.Id == other.Id && string.Equals(this.FileName, other.FileName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LibraryItem);
        }

        public override int GetHashCode()
        {
            return this.FileName.GetHashCode();
        }

        public static bool operator ==(LibraryItem a, LibraryItem b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(LibraryItem a, LibraryItem b)
        {
            return !(a == b);
        }
    }
}
