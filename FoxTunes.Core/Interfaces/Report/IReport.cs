using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IReportComponent : IBaseComponent
    {
        string Title { get; }

        string Description { get; }

        string[] Headers { get; }

        IEnumerable<IReportComponentRow> Rows { get; }

        string ActionName { get; }

        Task<bool> Action();
    }

    public interface IReportComponentRow : IBaseComponent
    {
        string[] Values { get; }

        string ActionName { get; }

        Task<bool> Action();
    }
}
