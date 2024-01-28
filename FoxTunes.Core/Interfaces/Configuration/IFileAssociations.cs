using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IFileAssociations
    {
        IEnumerable<IFileAssociation> Associations { get; }

        IFileAssociation Create(string extension);

        void Enable();

        void Enable(IEnumerable<IFileAssociation> associations);

        void Disable();

        void Disable(IEnumerable<IFileAssociation> associations);
    }
}
