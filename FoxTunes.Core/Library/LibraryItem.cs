using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IMetaDataSource, IFileData, IEquatable<LibraryItem>
    {
        public LibraryItem()
        {
            this.MetaDatas = new ObservableCollection<MetaDataItem>();
        }

        public LibraryItem(string directoryName, string fileName, LibraryItemStatus status, IMetaDataSource metaData)
            : this()
        {
            this.DirectoryName = directoryName;
            this.FileName = fileName;
            this.Status = status;
            this.MetaDatas = metaData.MetaDatas;
        }

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public LibraryItemStatus Status { get; set; }

        public ObservableCollection<MetaDataItem> MetaDatas { get; private set; }

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

    public enum LibraryItemStatus : byte
    {
        None = 0,
        Import = 1,
        Update = 2
    }
}
