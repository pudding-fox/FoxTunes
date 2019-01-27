using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassStreamPipelineManager : IBaseComponent, IDisposable
    {
        void WithPipeline(Action<IBassStreamPipeline> action);

        T WithPipeline<T>(Func<IBassStreamPipeline, T> func);

        Task WithPipelineExclusive(Action<IBassStreamPipeline> action);

        Task<T> WithPipelineExclusive<T>(Func<IBassStreamPipeline, T> func);

        Task WithPipelineExclusive(BassOutputStream stream, Action<IBassStreamPipeline> action);

        Task FreePipeline();
    }
}
