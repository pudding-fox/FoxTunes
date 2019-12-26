using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistItem : PersistableComponent, IFileData
    {
        public PlaylistItem()
        {

        }

        public int? LibraryItem_Id { get; set; }

        public int Sequence { get; set; }

        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public PlaylistItemStatus Status { get; set; }

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

        public override bool Equals(IPersistableComponent other)
        {
            if (other is PlaylistItem)
            {
                return base.Equals(other) && string.Equals(this.FileName, (other as PlaylistItem).FileName, StringComparison.OrdinalIgnoreCase);
            }
            return base.Equals(other);
        }
    }

    public enum PlaylistItemStatus : byte
    {
        None = 0,
        Import = 1,
        Update = 2,
        Remove = 3
    }
}
