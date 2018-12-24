using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        static BackgroundTask()
        {
            Semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private static ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; set; }

        protected BackgroundTask(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public virtual bool Visible
        {
            get
            {
                return false;
            }
        }

        public virtual bool Cancellable
        {
            get
            {
                return false;
            }
        }

        protected bool IsCancellationRequested { get; set; }

        public virtual int Concurrency
        {
            get
            {
                return 1;
            }
        }

        public SemaphoreSlim Semaphore
        {
            get
            {
                return Semaphores.GetOrAdd(this.Id, type => new SemaphoreSlim(this.Concurrency, this.Concurrency));
            }
        }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            protected set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual Task OnNameChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.NameChanged != null)
                {
                    this.NameChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Name");
            });
        }

        public event EventHandler NameChanged = delegate { };

        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
            protected set
            {
                this._Description = value;
                this.OnDescriptionChanged();
            }
        }

        protected virtual Task OnDescriptionChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.DescriptionChanged != null)
                {
                    this.DescriptionChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Description");
            });
        }

        public event EventHandler DescriptionChanged = delegate { };

        private int _Position { get; set; }

        public int Position
        {
            get
            {
                return this._Position;
            }
            protected set
            {
                this._Position = value;
                this.OnPositionChanged();
            }
        }

        protected virtual Task OnPositionChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.PositionChanged != null)
                {
                    this.PositionChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Position");
                return this.OnIsIndeterminateChanged();
            });
        }

        public event EventHandler PositionChanged = delegate { };

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
            protected set
            {
                this._Count = value;
                this.OnCountChanged();
            }
        }

        protected virtual Task OnCountChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.CountChanged != null)
                {
                    this.CountChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Count");
                return this.OnIsIndeterminateChanged();
            });
        }

        public event EventHandler CountChanged = delegate { };

        public bool IsIndeterminate
        {
            get
            {
                return this.Position == 0 && this.Count == 0;
            }
            set
            {
                if (value)
                {
                    this.Position = 0;
                    this.Count = 0;
                }
                else
                {
                    //Nothing to do.
                }
                this.OnIsIndeterminateChanged();
            }
        }

        protected virtual Task OnIsIndeterminateChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.IsIndeterminateChanged != null)
                {
                    this.IsIndeterminateChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("IsIndeterminate");
            });
        }

        public event EventHandler IsIndeterminateChanged = delegate { };

        public virtual Task Run()
        {
            Logger.Write(this, LogLevel.Debug, "Running background task.");
            return this.BackgroundTaskRunner.Run(async () =>
            {
                await this.OnStarted();
                await Semaphore.WaitAsync();
                try
                {
                    try
                    {
                        await this.OnRun();
                    }
                    finally
                    {
                        Semaphore.Release();
                    }
                    Logger.Write(this, LogLevel.Debug, "Background task succeeded.");
                    await this.OnCompleted();
                }
                catch (AggregateException e)
                {
                    foreach (var innerException in e.InnerExceptions)
                    {
                        Logger.Write(this, LogLevel.Error, "Background task failed: {0}", innerException.Message);
                    }
                    this.Exception = e;
                    await this.OnFaulted();
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Background task failed: {0}", e.Message);
                    this.Exception = e;
                    await this.OnFaulted();
                }
            });
        }

        protected abstract Task OnRun();

        protected virtual Task OnStarted()
        {
            if (this.Started == null)
            {
                return Task.CompletedTask;
            }
            return this.ForegroundTaskRunner.Run(() =>
            {
                var e = new AsyncEventArgs();
                this.Started(this, e);
                return e.Complete();
            });
        }

        public event AsyncEventHandler Started = delegate { };

        protected virtual Task OnCompleted()
        {
            if (this.Completed == null)
            {
                return Task.CompletedTask;
            }
            return this.ForegroundTaskRunner.Run(() =>
            {
                var e = new AsyncEventArgs();
                this.Completed(this, e);
                return e.Complete();
            });
        }

        public event AsyncEventHandler Completed = delegate { };

        private Exception _Exception { get; set; }

        public Exception Exception
        {
            get
            {
                return this._Exception;
            }
            protected set
            {
                this._Exception = value;
                this.OnExceptionChanged();
            }
        }

        protected virtual Task OnExceptionChanged()
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.ExceptionChanged != null)
                {
                    this.ExceptionChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Exception");
            });
        }

        public event EventHandler ExceptionChanged = delegate { };

        protected virtual Task OnFaulted()
        {
            if (this.Faulted == null)
            {
                return Task.CompletedTask;
            }
            return this.ForegroundTaskRunner.Run(() =>
            {
                var e = new AsyncEventArgs();
                this.Faulted(this, e);
                return e.Complete();
            });
        }

        public event AsyncEventHandler Faulted = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }

        public virtual void Cancel()
        {
            if (!this.Cancellable)
            {
                throw new NotImplementedException();
            }
            this.IsCancellationRequested = true;
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
            //Nothing to do.
        }

        ~BackgroundTask()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
