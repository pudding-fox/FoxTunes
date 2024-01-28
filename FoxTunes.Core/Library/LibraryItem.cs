using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class LibraryItem : PersistableComponent, IFileData
    {
        public string DirectoryName { get; set; }

        public string FileName { get; set; }

        public LibraryItemStatus Status { get; set; }

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
        Update = 2
    }
}
