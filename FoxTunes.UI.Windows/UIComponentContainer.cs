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
    public class UIComponentContainer : ContentControl, IUIComponent, IValueConverter
    {
        public static readonly IConfiguration Configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty ContentNameProperty = DependencyProperty.Register(
           "ContentName",
           typeof(string),
           typeof(UIComponentContainer),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnContentNameChanged))
       );

        public static string GetContentName(UIComponentContainer source)
        {
            return (string)source.GetValue(ContentNameProperty);
        }

        public static void SetContentName(UIComponentContainer source, string value)
        {
            source.SetValue(ContentNameProperty, value);
        }

        private static void OnContentNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentContainer = sender as UIComponentContainer;
            if (componentContainer == null)
            {
                return;
            }
            componentContainer.OnContentNameChanged();
        }

        public static readonly DependencyProperty ContentStringProperty = DependencyProperty.Register(
           "ContentString",
           typeof(string),
           typeof(UIComponentContainer),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnContentStringChanged))
       );

        public static string GetContentString(UIComponentContainer source)
        {
            return (string)source.GetValue(ContentStringProperty);
        }

        public static void SetContentString(UIComponentContainer source, string value)
        {
            source.SetValue(ContentStringProperty, value);
        }

        private static void OnContentStringChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var componentContainer = sender as UIComponentContainer;
            if (componentContainer == null)
            {
                return;
            }
            componentContainer.OnContentStringChanged();
        }

        public UIComponentContainer()
        {
            this.SetBinding(
                ContentControl.ContentProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath("ContentString"),
                    Converter = this
                }
            );
        }

        public string ContentName
        {
            get
            {
                return GetContentName(this);
            }
            set
            {
                SetContentName(this, value);
            }
        }

        protected virtual void OnContentNameChanged()
        {
            if (this.ContentNameChanged != null)
            {
                this.ContentNameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ContentName");
        }

        public event EventHandler ContentNameChanged;

        public string ContentString
        {
            get
            {
                return GetContentString(this);
            }
            set
            {
                SetContentString(this, value);
            }
        }

        protected virtual void OnContentStringChanged()
        {
            if (this.ContentStringChanged != null)
            {
                this.ContentStringChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ContentString");
        }

        public event EventHandler ContentStringChanged;

        public virtual void InitializeComponent(ICore core)
        {
            //Nothing to do.
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contentString = value as string;
            if (string.IsNullOrEmpty(contentString))
            {
                var selector = new UIComponentSelector();
                selector.HorizontalAlignment = HorizontalAlignment.Center;
                selector.VerticalAlignment = VerticalAlignment.Center;
                selector.ComponentChanged += this.OnComponentChanged;
                return selector;
            }
            try
            {
                var component = Configuration.LoadValue<UIComponent>(contentString);
                return ComponentActivator.Instance.Activate<UIComponentBase>(component.Type);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Error loading component: {0}", e.Message);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnComponentChanged(object sender, EventArgs e)
        {
            var selector = sender as UIComponentSelector;
            if (selector == null || selector.Component == null)
            {
                return;
            }
            this.LoadComponent(selector.Component);
            selector.ComponentChanged -= this.OnComponentChanged;
        }

        protected virtual void LoadComponent(UIComponent component)
        {
            try
            {
                this.ContentName = component.Name;
                this.ContentString = Configuration.SaveValue(component);
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Error loading component: {0}", e.Message);
                this.ContentName = null;
                this.ContentString = null;
            }
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
