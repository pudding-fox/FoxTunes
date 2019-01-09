using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public abstract class ViewModelBase : Freezable, IBaseComponent, INotifyPropertyChanged
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty CoreProperty = DependencyProperty.Register(
            "Core",
            typeof(ICore),
            typeof(ViewModelBase),
            new PropertyMetadata(new PropertyChangedCallback(OnCoreChanged))
        );

        public static ICore GetCore(ViewModelBase source)
        {
            return (ICore)source.GetValue(CoreProperty);
        }

        public static void SetCore(ViewModelBase source, ICore value)
        {
            source.SetValue(CoreProperty, value);
        }

        public static void OnCoreChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = sender as ViewModelBase;
            if (viewModel == null)
            {
                return;
            }
            if (viewModel.Core != null && !viewModel.IsInitialized)
            {
                viewModel.InitializeComponent(viewModel.Core);
            }
            viewModel.OnCoreChanged();
        }

        public ICore Core
        {
            get
            {
                return this.GetValue(CoreProperty) as ICore;
            }
            set
            {
                this.SetValue(CoreProperty, value);
            }
        }

        protected virtual void OnCoreChanged()
        {
            if (this.CoreChanged != null)
            {
                this.CoreChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Core");
        }

        public event EventHandler CoreChanged = delegate { };

        public bool IsInitialized { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.IsInitialized = true;
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (Error == null)
            {
                return Task.CompletedTask;
            }
            return Error(this, new ComponentErrorEventArgs(message, exception));
        }

        event ComponentErrorEventHandler IBaseComponent.Error
        {
            add
            {
                Error += value;
            }
            remove
            {
                Error -= value;
            }
        }

        public static event ComponentErrorEventHandler Error;
    }
}
