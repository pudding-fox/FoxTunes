using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        protected BackgroundTask(string id, bool visible = true)
        {
            this.Id = id;
            this.Visible = visible;
        }

        public string Id { get; private set; }

        public bool Visible { get; private set; }

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
            });
        }

        public event EventHandler CountChanged = delegate { };

        public void Run()
        {
            this.BackgroundTaskRunner.Run(() =>
            {
                this.OnStarted();
                try
                {
                    this.OnRun();
                    this.OnCompleted();
                }
                catch (Exception e)
                {
                    this.Exception = e;
                    this.OnFaulted();
                }
            });
        }

        protected abstract void OnRun();

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
