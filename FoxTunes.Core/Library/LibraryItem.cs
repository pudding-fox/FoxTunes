using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IFileData
    {
        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public string ImportDate { get; set; }

        public LibraryItemStatus Status { get; set; }

        [Relation(Flags = RelationFlags.AutoExpression | RelationFlags.EagerFetch | RelationFlags.ManyToMany)]
        public IList<MetaDataItem> MetaDatas { get; set; }

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
            return DateTimeHelper.FromString(this.ImportDate);
        }

        public void SetImportDate(DateTime value)
        {
            this.ImportDate = DateTimeHelper.ToString(value);
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
        Remove = 3,
        Export = 4
    }
}
