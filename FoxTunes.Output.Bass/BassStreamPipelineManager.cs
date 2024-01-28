using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class BassStreamPipelineManager : StandardComponent, IBassStreamPipelineManager
    {
        const int SYNCHRONIZE_PIPELINE_TIMEOUT = 1000;

        public BassStreamPipelineManager()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public IBassStreamPipelineFactory PipelineFactory { get; private set; }

        public IBassStreamPipeline Pipeline { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            base.InitializeComponent(core);
        }

#if NET40
        public Task WithPipelineExclusive(Action<IBassStreamPipeline> action)
#else
        public async Task WithPipelineExclusive(Action<IBassStreamPipeline> action)
#endif
        {
            if (this.Semaphore == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return;
#endif
            }
#if NET40
            if (!this.Semaphore.Wait(SYNCHRONIZE_PIPELINE_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
                this.WithPipeline(action);
            }
            finally
            {
                this.Semaphore.Release();
            }
#if NET40
            return TaskEx.FromResult(false);
#endif
        }

        public void WithPipeline(Action<IBassStreamPipeline> action)
        {
            action(this.Pipeline);
        }

#if NET40
        public Task<T> WithPipelineExclusive<T>(Func<IBassStreamPipeline, T> func)
#else
        public async Task<T> WithPipelineExclusive<T>(Func<IBassStreamPipeline, T> func)
#endif
        {
            if (this.Semaphore == null)
            {
#if NET40
                return TaskEx.FromResult(default(T));
#else
                return default(T);
#endif
            }
#if NET40
            if (!this.Semaphore.Wait(SYNCHRONIZE_PIPELINE_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
#if NET40
                return TaskEx.FromResult(this.WithPipeline(func));
#else
                return this.WithPipeline(func);
#endif
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        public T WithPipeline<T>(Func<IBassStreamPipeline, T> func)
        {
            return func(this.Pipeline);
        }

        public async Task WithPipelineExclusive(BassOutputStream stream, Action<IBassStreamPipeline> action)
        {
            if (this.Semaphore == null)
            {
                return;
            }
#if NET40
            if (!this.Semaphore.Wait(SYNCHRONIZE_PIPELINE_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
                await this.WithPipeline(stream, action);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task WithPipeline(BassOutputStream stream, Action<IBassStreamPipeline> action)
        {
            if (this.Pipeline == null)
            {
                this.CreatePipelineCore(stream);
            }
            else if (!this.Pipeline.Input.CheckFormat(stream.Rate, stream.Channels))
            {
                Logger.Write(this, LogLevel.Debug, "Current pipeline cannot accept stream, shutting it down: {0}", stream.ChannelHandle);
                this.FreePipelineCore();
                await this.WithPipeline(stream, action);
                return;
            }
            action(this.Pipeline);
        }

#if NET40
        protected virtual Task CreatePipeline(BassOutputStream stream)
#else
        protected virtual async Task CreatePipeline(BassOutputStream stream)
#endif
        {
            if (this.Semaphore == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return;
#endif
            }
#if NET40
            if (!this.Semaphore.Wait(SYNCHRONIZE_PIPELINE_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
                this.CreatePipelineCore(stream);
            }
            finally
            {
                this.Semaphore.Release();
            }
#if NET40
            return TaskEx.FromResult(false);
#endif
        }

        protected virtual void CreatePipelineCore(BassOutputStream stream)
        {
            this.Pipeline = this.PipelineFactory.CreatePipeline(stream);
            this.Pipeline.Input.Add(stream.ChannelHandle);
            this.Pipeline.Error += this.OnError;
        }

#if NET40
        public Task FreePipeline()
#else
        public async Task FreePipeline()
#endif
        {
            if (this.Semaphore == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return;
#endif
            }
#if NET40
            if (!this.Semaphore.Wait(SYNCHRONIZE_PIPELINE_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
                this.FreePipelineCore();
            }
            finally
            {
                this.Semaphore.Release();
            }
#if NET40
            return TaskEx.FromResult(false);
#endif
        }

        protected virtual void FreePipelineCore()
        {
            if (this.Pipeline != null)
            {
                Logger.Write(this, LogLevel.Debug, "Shutting down the pipeline.");
                this.Pipeline.Error -= this.OnError;
                this.Pipeline.Dispose();
                this.Pipeline = null;
            }
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.FreePipelineCore();
            if (this.Semaphore != null)
            {
                this.Semaphore.Dispose();
                this.Semaphore = null;
            }
        }

        ~BassStreamPipelineManager()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
