using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class BackgroundTask : ViewModelBase
    {
        public BackgroundTask(IBackgroundTask backgroundTask)
        {
            if (backgroundTask != null)
            {
                this.InnerBackgroundTask = backgroundTask;
                this.InnerBackgroundTask.NameChanged += this.OnNameChanged;
                this.InnerBackgroundTask.DescriptionChanged += this.OnDescriptionChanged;
                this.InnerBackgroundTask.PositionChanged += this.OnPositionChanged;
                this.InnerBackgroundTask.CountChanged += this.OnCountChanged;
                this.InnerBackgroundTask.IsIndeterminateChanged += this.OnIsIndeterminateChanged;
            }
        }

        public IBackgroundTask InnerBackgroundTask { get; private set; }

        public string Name
        {
            get
            {
                return this.InnerBackgroundTask.Name;
            }
        }

        protected virtual async void OnNameChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await Windows.Invoke(() =>
                {
                    if (this.NameChanged != null)
                    {
                        this.NameChanged(sender, e);
                    }
                    this.OnPropertyChanged("Name");
                });
            }
        }

        public event EventHandler NameChanged = delegate { };

        public string Description
        {
            get
            {
                return this.InnerBackgroundTask.Description;
            }
        }

        protected virtual async void OnDescriptionChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await Windows.Invoke(() =>
                {
                    if (this.DescriptionChanged != null)
                    {
                        this.DescriptionChanged(sender, e);
                    }
                    this.OnPropertyChanged("Description");
                });
            }
        }

        public event EventHandler DescriptionChanged = delegate { };

        public int Position
        {
            get
            {
                return this.InnerBackgroundTask.Position;
            }
        }

        protected virtual async void OnPositionChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await Windows.Invoke(() =>
                {
                    if (this.PositionChanged != null)
                    {
                        this.PositionChanged(sender, e);
                    }
                    this.OnPropertyChanged("Position");
                });
            }
        }

        public event EventHandler PositionChanged = delegate { };

        public int Count
        {
            get
            {
                return this.InnerBackgroundTask.Count;
            }
        }

        protected virtual async void OnCountChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await Windows.Invoke(() =>
                {
                    if (this.CountChanged != null)
                    {
                        this.CountChanged(sender, e);
                    }
                    this.OnPropertyChanged("Count");
                });
            }
        }

        public event EventHandler CountChanged = delegate { };

        public bool IsIndeterminate
        {
            get
            {
                return this.InnerBackgroundTask.IsIndeterminate;
            }
        }

        protected virtual async void OnIsIndeterminateChanged(object sender, AsyncEventArgs e)
        {
            using (e.Defer())
            {
                await Windows.Invoke(() =>
                {
                    if (this.IsIndeterminateChanged != null)
                    {
                        this.IsIndeterminateChanged(sender, e);
                    }
                    this.OnPropertyChanged("IsIndeterminate");
                });
            }
        }

        public event EventHandler IsIndeterminateChanged = delegate { };

        protected override void OnDisposing()
        {
            this.InnerBackgroundTask.NameChanged -= this.OnNameChanged;
            this.InnerBackgroundTask.DescriptionChanged -= this.OnDescriptionChanged;
            this.InnerBackgroundTask.PositionChanged -= this.OnPositionChanged;
            this.InnerBackgroundTask.CountChanged -= this.OnCountChanged;
            this.InnerBackgroundTask.IsIndeterminateChanged -= this.OnIsIndeterminateChanged;
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BackgroundTask(null);
        }
    }
}
