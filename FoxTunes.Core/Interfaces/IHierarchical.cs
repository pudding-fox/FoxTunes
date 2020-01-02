using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IHierarchical
    {
        IHierarchical Parent { get; }

        IEnumerable<IHierarchical> Children { get; }

        void LoadChildren();

        Task LoadChildrenAsync();
    }
}