using FoxTunes.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassStreamPipelineManager : StandardComponent, IBassStreamPipelineManager
    {
        const int SYNCHRONIZE_PIPELINE_TIMEOUT = 1000;

        public BassStreamPipelineManager()
        {
#if NET40
            this.Semaphore = new AsyncSemaphore(1);
#else
            this.Semaphore = new SemaphoreSlim(1, 1);
#endif
        }

#if NET40
        public AsyncSemaphore Semaphore { get; private set; }
#else
        public SemaphoreSlim Semaphore { get; private set; }
#endif

        public IBassStreamPipelineFactory PipelineFactory { get; private set; }

        public IBassStreamPipeline Pipeline { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            base.InitializeComponent(core);
        }

        public async Task WithPipelineExclusive(Action<IBassStreamPipeline> action)
        {
            if (this.Semaphore == null)
            {
                return;
            }
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
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
        }

        public void WithPipeline(Action<IBassStreamPipeline> action)
        {
            action(this.Pipeline);
        }

        public async Task<T> WithPipelineExclusive<T>(Func<IBassStreamPipeline, T> func)
        {
            if (this.Semaphore == null)
            {
                return default(T);
            }
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
            {
                throw new InvalidOperationException(string.Format("{0} is already locked.", this.GetType().Name));
            }
            try
            {
                return this.WithPipeline(func);
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
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
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

        protected virtual async Task CreatePipeline(BassOutputStream stream)
        {
            if (this.Semaphore == null)
            {
                return;
            }
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
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
        }

        protected virtual void CreatePipelineCore(BassOutputStream stream)
        {
            this.Pipeline = this.PipelineFactory.CreatePipeline(stream);
            this.Pipeline.Input.Add(stream.ChannelHandle);
            this.Pipeline.Error += this.OnError;
        }

        public async Task FreePipeline()
        {
            if (this.Semaphore == null)
            {
                return;
            }
            if (!await this.Semaphore.WaitAsync(SYNCHRONIZE_PIPELINE_TIMEOUT))
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
            this.Dispose(true);
        }
    }
}
