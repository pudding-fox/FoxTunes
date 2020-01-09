using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IReport
    {
        string Title { get; }

        string Description { get; }

        string[] Headers { get; }

        IEnumerable<IReportRow> Rows { get; }

        Action<Guid> Action { get; }
    }

    public interface IReportRow
    {
        Guid Id { get; }

        string[] Values { get; }
    }
}
