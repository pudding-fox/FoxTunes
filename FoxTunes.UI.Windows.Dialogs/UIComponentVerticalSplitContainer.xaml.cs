using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for UIComponentVerticalSplitContainer.xaml
    /// </summary>
    [UIComponent("18E98420-F039-4504-A116-3D0F26BEAAD5", UIComponentSlots.NONE, "Vertical Split", role: UIComponentRole.Hidden)]
    public partial class UIComponentVerticalSplitContainer : UIComponentPanel
    {
        public static readonly DependencyProperty LeftComponentProperty = DependencyProperty.Register(
            "LeftComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnLeftComponentChanged))
        );

        public static UIComponentConfiguration GetLeftComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(LeftComponentProperty);
        }

        public static void SetLeftComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(LeftComponentProperty, value);
        }

        public static void OnLeftComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnLeftComponentChanged();
        }

        public static readonly DependencyProperty RightComponentProperty = DependencyProperty.Register(
            "RightComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnRightComponentChanged))
        );

        public static UIComponentConfiguration GetRightComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(RightComponentProperty);
        }

        public static void SetRightComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(RightComponentProperty, value);
        }

        public static void OnRightComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnRightComponentChanged();
        }

        public static readonly DependencyProperty SplitterDistanceProperty = DependencyProperty.Register(
            "SplitterDistance",
            typeof(string),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnSplitterDistanceChanged))
        );

        public static string GetSplitterDistance(UIComponentVerticalSplitContainer source)
        {
            return (string)source.GetValue(SplitterDistanceProperty);
        }

        public static void SetSplitterDistance(UIComponentVerticalSplitContainer source, string value)
        {
            source.SetValue(SplitterDistanceProperty, value);
        }

        public static void OnSplitterDistanceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnSplitterDistanceChanged();
        }

        public UIComponentVerticalSplitContainer()
        {
            this.InitializeComponent();
        }

        protected override void OnComponentChanged()
        {
            if (this.Component != null)
            {
                this.UpdateChildren();
                this.UpdateSplitterDistance();
            }
            base.OnComponentChanged();
        }

        protected virtual void UpdateChildren()
        {
            if (this.Component.Children.Count == 2)
            {
                this.LeftComponent = this.Component.Children[0];
                this.RightComponent = this.Component.Children[1];
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

        protected virtual void UpdateSplitterDistance()
        {
            var splitterDistance = default(string);
            if (this.Component.TryGet(nameof(this.SplitterDistance), out splitterDistance))
            {
                this.SplitterDistance = splitterDistance;
            }
            else
            {
                this.SplitterDistance = "1*";
            }
        }

        public UIComponentConfiguration LeftComponent
        {
            get
            {
                return this.GetValue(LeftComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(LeftComponentProperty, value);
            }
        }

        protected virtual void OnLeftComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.LeftComponent != null)
            {
                this.Component.Children[0] = this.LeftComponent;
            }
            if (this.LeftComponentChanged != null)
            {
                this.LeftComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("LeftComponent");
        }

        public event EventHandler LeftComponentChanged;

        public UIComponentConfiguration RightComponent
        {
            get
            {
                return this.GetValue(RightComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(RightComponentProperty, value);
            }
        }

        protected virtual void OnRightComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.RightComponent != null)
            {
                this.Component.Children[1] = this.RightComponent;
            }
            if (this.RightComponentChanged != null)
            {
                this.RightComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("RightComponent");
        }

        public event EventHandler RightComponentChanged;

        public string SplitterDistance
        {
            get
            {
                return (string)this.GetValue(SplitterDistanceProperty);
            }
            set
            {
                this.SetValue(SplitterDistanceProperty, value);
            }
        }

        protected virtual void OnSplitterDistanceChanged()
        {
            if (this.Component != null)
            {
                this.Component.AddOrUpdate(
                    nameof(this.SplitterDistance),
                    this.SplitterDistance
                );
            }
            if (this.SplitterDistanceChanged != null)
            {
                this.SplitterDistanceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SplitterDistance");
        }

        public event EventHandler SplitterDistanceChanged;
    }
}
