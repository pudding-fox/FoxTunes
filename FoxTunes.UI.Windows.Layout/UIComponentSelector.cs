using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    public class UIComponentSelector : ContentControl, IUIComponent, IValueConverter
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
            typeof(UIComponentSelector),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnComponentChanged))
        );

        public static UIComponentConfiguration GetComponent(UIComponentSelector source)
        {
            return (UIComponentConfiguration)source.GetValue(ComponentProperty);
        }

        public static void SetComponent(UIComponentSelector source, UIComponentConfiguration value)
        {
            source.SetValue(ComponentProperty, value);
        }

        private static void OnComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentSelector = sender as UIComponentSelector;
            if (componentSelector == null)
            {
                return;
            }
            componentSelector.OnComponentChanged();
        }

        public UIComponentSelector()
        {
            this.InitializeComponent();
        }

        protected virtual void InitializeComponent()
        {
            var comboBox = new ComboBox();
            if (LayoutManager.Instance != null)
            {
                comboBox.ItemsSource = LayoutManager.Instance.Components;
            }
            comboBox.DisplayMemberPath = "Name";
            comboBox.SetBinding(
                ComboBox.SelectedValueProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("Component"),
                    Converter = this
                }
            );

            this.Content = comboBox;
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
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if (value is UIComponent component)
            {
                return Factory.CreateConfiguration(component);
            }
            if (value is UIComponentConfiguration configuration)
            {
                return Factory.CreateComponent(configuration);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            if (value is UIComponent component)
            {
                return Factory.CreateConfiguration(component);
            }
            if (value is UIComponentConfiguration configuration)
            {
                return Factory.CreateComponent(configuration);
            }
            return value;
        }

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
