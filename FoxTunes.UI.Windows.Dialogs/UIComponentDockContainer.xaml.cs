using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for UIComponentDockContainer.xaml
    /// </summary>
    [UIComponent("3E899F79-380C-4EF7-8570-4B4E3B3467CB", UIComponentSlots.NONE, "Dock", role: UIComponentRole.Hidden)]
    public partial class UIComponentDockContainer : UIComponentPanel
    {
        public static readonly DependencyProperty ContentComponentProperty = DependencyProperty.Register(
            "ContentComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentDockContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnContentComponentChanged))
        );

        public static UIComponentConfiguration GetContentComponent(UIComponentDockContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(ContentComponentProperty);
        }

        public static void SetContentComponent(UIComponentDockContainer source, UIComponentConfiguration value)
        {
            source.SetValue(ContentComponentProperty, value);
        }

        public static void OnContentComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentDockContainer;
            if (container == null)
            {
                return;
            }
            container.OnContentComponentChanged();
        }

        public static readonly DependencyProperty DockComponentProperty = DependencyProperty.Register(
            "DockComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentDockContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnDockComponentChanged))
        );

        public static UIComponentConfiguration GetDockComponent(UIComponentDockContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(DockComponentProperty);
        }

        public static void SetDockComponent(UIComponentDockContainer source, UIComponentConfiguration value)
        {
            source.SetValue(DockComponentProperty, value);
        }

        public static void OnDockComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentDockContainer;
            if (container == null)
            {
                return;
            }
            container.OnDockComponentChanged();
        }

        public static readonly DependencyProperty DockLocationProperty = DependencyProperty.Register(
           "DockLocation",
           typeof(string),
           typeof(UIComponentDockContainer),
           new PropertyMetadata(new PropertyChangedCallback(OnDockLocationChanged))
       );

        public static string GetDockLocation(UIComponentDockContainer source)
        {
            return (string)source.GetValue(DockLocationProperty);
        }

        public static void SetDockLocation(UIComponentDockContainer source, string value)
        {
            source.SetValue(DockLocationProperty, value);
        }

        public static void OnDockLocationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentDockContainer;
            if (container == null)
            {
                return;
            }
            container.OnDockLocationChanged();
        }

        public UIComponentDockContainer()
        {
            this.InitializeComponent();
        }

        protected override void OnComponentChanged()
        {
            if (this.Component != null)
            {
                this.UpdateChildren();
                this.UpdateDockLocation();
            }
            base.OnComponentChanged();
        }

        protected virtual void UpdateChildren()
        {
            if (this.Component.Children != null && this.Component.Children.Count == 2)
            {
                this.ContentComponent = this.Component.Children[0];
                this.DockComponent = this.Component.Children[1];
            }
            else
            {
                this.Component.Children = new ObservableCollection<UIComponentConfiguration>()
                {
                    new UIComponentConfiguration(),
                    new UIComponentConfiguration()
                };
            }
        }

        protected virtual void UpdateDockLocation()
        {
            var DockLocation = default(string);
            if (this.Component.TryGet(nameof(this.DockLocation), out DockLocation))
            {
                this.DockLocation = DockLocation;
            }
            else
            {
                this.DockLocation = "Top";
            }
        }

        public UIComponentConfiguration ContentComponent
        {
            get
            {
                return this.GetValue(ContentComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(ContentComponentProperty, value);
            }
        }

        protected virtual void OnContentComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count > 0 && this.ContentComponent != null)
            {
                this.Component.Children[0] = this.ContentComponent;
            }
            if (this.ContentComponentChanged != null)
            {
                this.ContentComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ContentComponent");
        }

        public event EventHandler ContentComponentChanged;

        public UIComponentConfiguration DockComponent
        {
            get
            {
                return this.GetValue(DockComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(DockComponentProperty, value);
            }
        }

        protected virtual void OnDockComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.DockComponent != null)
            {
                this.Component.Children[1] = this.DockComponent;
            }
            if (this.DockComponentChanged != null)
            {
                this.DockComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DockComponent");
        }

        public event EventHandler DockComponentChanged;

        public string DockLocation
        {
            get
            {
                return (string)this.GetValue(DockLocationProperty);
            }
            set
            {
                this.SetValue(DockLocationProperty, value);
            }
        }

        protected virtual void OnDockLocationChanged()
        {
            if (this.Component != null)
            {
                this.Component.AddOrUpdate(
                    nameof(this.DockLocation),
                    this.DockLocation
                );
            }
            if (this.DockLocationChanged != null)
            {
                this.DockLocationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("DockLocation");
        }

        public event EventHandler DockLocationChanged;
    }
}
