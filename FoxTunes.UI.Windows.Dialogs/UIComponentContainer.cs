using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FoxTunes
{
    public class UIComponentContainer : ContentControl, IUIComponent, IValueConverter
    {
        public static readonly UIComponentFactory Factory = ComponentRegistry.Instance.GetComponent<UIComponentFactory>();

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component",
            typeof(UIComponentConfiguration),
            typeof(UIComponentContainer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnComponentChanged))
       );

        public static UIComponentConfiguration GetComponent(UIComponentContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(ComponentProperty);
        }

        public static void SetComponent(UIComponentContainer source, UIComponentConfiguration value)
        {
            source.SetValue(ComponentProperty, value);
        }

        public static void OnComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentContainer = sender as UIComponentContainer;
            if (componentContainer == null)
            {
                return;
            }
            componentContainer.OnComponentChanged();
        }

        public UIComponentContainer()
        {
            this.SetBinding(
                ContentControl.ContentProperty,
                new Binding()
                {
                    Path = new PropertyPath("Component"),
                    Source = this,
                    Converter = this
                }
            );
        }

        public UIComponentConfiguration Component
        {
            get
            {
                return GetComponent(this);
            }
            set
            {
                SetComponent(this, value);
            }
        }

        protected virtual void OnComponentChanged()
        {
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ComponentChanged;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var configuration = value as UIComponentConfiguration;
            if (configuration == null || string.IsNullOrEmpty(configuration.Component))
            {
                return this.CreateSelector();
            }
            return Factory.CreateControl(configuration);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected virtual UIComponentSelector CreateSelector()
        {
            var selector = new UIComponentSelector();
            selector.HorizontalAlignment = HorizontalAlignment.Center;
            selector.VerticalAlignment = VerticalAlignment.Center;
            selector.SetBinding(
                UIComponentSelector.ComponentProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("Component")
                }
            );
            return selector;
        }

        public void InitializeComponent(ICore core)
        {
            throw new NotImplementedException();
        }

        public ICommand ClearCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Clear);
            }
        }

        public void Clear()
        {
            this.Component = null;
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
