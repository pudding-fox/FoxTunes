using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IFileData
    {
        public const string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public string ImportDate { get; set; }

        public bool Favorite { get; set; }

        public LibraryItemStatus Status { get; set; }

        [Relation(Flags = RelationFlags.AutoExpression | RelationFlags.EagerFetch | RelationFlags.ManyToMany)]
        public ObservableCollection<MetaDataItem> MetaDatas { get; set; }

        protected virtual void OnMetaDatasChanged()
        {
            if (this.MetaDatasChanged != null)
            {
                this.MetaDatasChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MetaDatas");
        }

        public event EventHandler MetaDatasChanged;

        public DateTime GetImportDate()
        {
            if (string.IsNullOrEmpty(this.ImportDate))
            {
                return default(DateTime);
            }
            return DateTime.ParseExact(this.ImportDate, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public void SetImportDate(DateTime value)
        {
            this.ImportDate = value.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public override bool Equals(IPersistableComponent other)
        {
            if (other is LibraryItem)
            {
                return base.Equals(other) && string.Equals(this.FileName, (other as LibraryItem).FileName, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(other);
        }
    }

    public enum LibraryItemStatus : byte
    {
        None = 0,
        Import = 1,
        Update = 2,
        Remove = 3
    }
}
