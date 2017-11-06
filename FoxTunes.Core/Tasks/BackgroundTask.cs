using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        static BackgroundTask()
        {
            Semaphores = new ConcurrentDictionary<Type, SemaphoreSlim>();
        }

        private static ConcurrentDictionary<Type, SemaphoreSlim> Semaphores { get; set; }

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
                return Semaphores.GetOrAdd(this.GetType(), type => new SemaphoreSlim(this.Concurrency, this.Concurrency));
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

        protected virtual void OnNameChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
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

        protected virtual void OnDescriptionChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
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

        protected virtual void OnPositionChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
            {
                if (this.PositionChanged != null)
                {
                    this.PositionChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Position");
                this.OnIsIndeterminateChanged();
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

        protected virtual void OnCountChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
            {
                if (this.CountChanged != null)
                {
                    this.CountChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Count");
                this.OnIsIndeterminateChanged();
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

        protected virtual void OnIsIndeterminateChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
            {
                if (this.IsIndeterminateChanged != null)
                {
                    this.IsIndeterminateChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("IsIndeterminate");
            });
        }

        public event EventHandler IsIndeterminateChanged = delegate { };

        public Task Run()
        {
            Logger.Write(this, LogLevel.Debug, "Running background task.");
            return this.BackgroundTaskRunner.Run(async () =>
            {
                await Semaphore.WaitAsync();
                this.OnStarted();
                try
                {
                    await this.OnRun().ContinueWith(task =>
                    {
                        switch (task.Status)
                        {
                            case TaskStatus.Faulted:
                                Logger.Write(this, LogLevel.Error, "Background task failed: {0}", task.Exception.Message);
                                this.Exception = task.Exception;
                                this.OnFaulted();
                                break;
                            default:
                                Logger.Write(this, LogLevel.Debug, "Background task succeeded.");
                                this.OnCompleted();
                                break;
                        }
                        Semaphore.Release();
                    });
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Background task failed: {0}", e.Message);
                    this.Exception = e;
                    this.OnFaulted();
                    Semaphore.Release();
                }
            });
        }

        public void RunSynchronously()
        {
            throw new NotImplementedException();
        }

        protected abstract Task OnRun();

        protected virtual void OnStarted()
        {
            if (this.Started == null)
            {
                return;
            }
            this.ForegroundTaskRunner.Run(() => this.Started(this, EventArgs.Empty));
        }

        public event EventHandler Started = delegate { };

        protected virtual void OnCompleted()
        {
            if (this.Completed == null)
            {
                return;
            }
            this.ForegroundTaskRunner.Run(() => this.Completed(this, EventArgs.Empty));
        }

        public event EventHandler Completed = delegate { };

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

        protected virtual void OnExceptionChanged()
        {
            this.ForegroundTaskRunner.Run(() =>
            {
                if (this.ExceptionChanged != null)
                {
                    this.ExceptionChanged(this, EventArgs.Empty);
                }
                this.OnPropertyChanged("Exception");
            });
        }

        public event EventHandler ExceptionChanged = delegate { };

        protected virtual void OnFaulted()
        {
            if (this.Faulted == null)
            {
                return;
            }
            this.ForegroundTaskRunner.Run(() => this.Faulted(this, EventArgs.Empty));
        }

        public event EventHandler Faulted = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }
    }
}
