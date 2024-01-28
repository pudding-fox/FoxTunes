using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IDataManager : IStandardManager
    {
        Task Reload();

        IDatabaseContext ReadContext { get; }

        event EventHandler ReadContextChanged;

        IDatabaseContext CreateWriteContext();
    }
}
