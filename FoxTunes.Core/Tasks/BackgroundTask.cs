using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        protected BackgroundTask(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            private set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged = delegate { };

        protected virtual void SetName(string name)
        {
            this.ForegroundTaskRunner.Run(() => this.Name = name);
        }

        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
            private set
            {
                this._Description = value;
                this.OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                this.DescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Description");
        }

        public event EventHandler DescriptionChanged = delegate { };

        protected virtual void SetDescription(string description)
        {
            this.ForegroundTaskRunner.Run(() => this.Description = description);
        }

        private int _Position { get; set; }

        public int Position
        {
            get
            {
                return this._Position;
            }
            private set
            {
                this._Position = value;
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged = delegate { };

        protected virtual void SetPosition(int position)
        {
            this.ForegroundTaskRunner.Run(() => this.Position = position);
        }

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
            private set
            {
                this._Count = value;
                this.OnCountChanged();
            }
        }

        protected virtual void OnCountChanged()
        {
            if (this.CountChanged != null)
            {
                this.CountChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Count");
        }

        public event EventHandler CountChanged = delegate { };

        protected virtual void SetCount(int count)
        {
            this.ForegroundTaskRunner.Run(() => this.Count = count);
        }

        public void Run()
        {
            this.BackgroundTaskRunner.Run(() =>
            {
                this.OnStarted();
                this.OnRun();
                this.OnCompleted();
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

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = core.Components.BackgroundTaskRunner;
            this.ForegroundTaskRunner = core.Components.ForegroundTaskRunner;
            base.InitializeComponent(core);
        }
    }
}
