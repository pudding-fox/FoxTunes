using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    public abstract class UIComponentBase : UserControl, IUIComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty IsComponentEnabledProperty = DependencyProperty.Register(
           "IsComponentEnabled",
           typeof(bool),
           typeof(UIComponentBase),
           new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsComponentEnabledChanged))
       );

        public static bool GetIsComponentEnabled(UIComponentBase source)
        {
            return (bool)source.GetValue(IsComponentEnabledProperty);
        }

        public static void SetIsComponentEnabled(UIComponentBase source, bool value)
        {
            source.SetValue(IsComponentEnabledProperty, value);
        }

        private static void OnIsComponentEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentBase = sender as UIComponentBase;
            if (componentBase == null)
            {
                return;
            }
            componentBase.OnIsComponentEnabledChanged();
        }

        protected virtual bool IsHostedIn<T>() where T : Window
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                return false;
            }
            return typeof(T).IsAssignableFrom(window.GetType());
        }

        public bool IsComponentEnabled
        {
            get
            {
                return GetIsComponentEnabled(this);
            }
            set
            {
                SetIsComponentEnabled(this, value);
            }
        }

        protected virtual void OnIsComponentEnabledChanged()
        {
            if (this.IsComponentEnabledChanged != null)
            {
                this.IsComponentEnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsComponentEnabled");
        }

        public event EventHandler IsComponentEnabledChanged;

        public virtual void InitializeComponent(ICore core)
        {
            //Nothing to do.
        }

        protected virtual void Dispatch(Func<Task> function)
        {
#if NET40
            var task = TaskEx.Run(function);
#else
            var task = Task.Run(function);
#endif
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (Error == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
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
