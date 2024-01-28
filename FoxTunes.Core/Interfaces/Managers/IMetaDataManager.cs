using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataManager : IStandardManager, IBackgroundTaskSource, IReportSource
    {
        Task Rescan(IEnumerable<IFileData> fileDatas);

        Task Save(IEnumerable<IFileData> fileDatas, bool write, bool report, params string[] names);

        Task Synchronize();

        Task<bool> Synchronize(IEnumerable<IFileData> fileDatas, params string[] names);
    }
}
