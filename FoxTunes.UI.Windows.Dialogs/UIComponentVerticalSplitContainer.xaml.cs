using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for UIComponentVerticalSplitContainer.xaml
    /// </summary>
    [UIComponent("A6820FDA-E415-40C6-AEFB-A73B6FBE4C93", UIComponentSlots.NONE, "Vertical Split", role: UIComponentRole.Hidden)]
    public partial class UIComponentVerticalSplitContainer : UIComponentPanel
    {
        public static readonly DependencyProperty TopComponentProperty = DependencyProperty.Register(
            "TopComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnTopComponentChanged))
        );

        public static UIComponentConfiguration GetTopComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(TopComponentProperty);
        }

        public static void SetTopComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(TopComponentProperty, value);
        }

        public static void OnTopComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnTopComponentChanged();
        }

        public static readonly DependencyProperty BottomComponentProperty = DependencyProperty.Register(
            "BottomComponent",
            typeof(UIComponentConfiguration),
            typeof(UIComponentVerticalSplitContainer),
            new PropertyMetadata(new PropertyChangedCallback(OnBottomComponentChanged))
        );

        public static UIComponentConfiguration GetBottomComponent(UIComponentVerticalSplitContainer source)
        {
            return (UIComponentConfiguration)source.GetValue(BottomComponentProperty);
        }

        public static void SetBottomComponent(UIComponentVerticalSplitContainer source, UIComponentConfiguration value)
        {
            source.SetValue(BottomComponentProperty, value);
        }

        public static void OnBottomComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentVerticalSplitContainer;
            if (container == null)
            {
                return;
            }
            container.OnBottomComponentChanged();
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
            if (this.Component.Children != null && this.Component.Children.Count == 2)
            {
                this.TopComponent = this.Component.Children[0];
                this.BottomComponent = this.Component.Children[1];
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

        public UIComponentConfiguration TopComponent
        {
            get
            {
                return this.GetValue(TopComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(TopComponentProperty, value);
            }
        }

        protected virtual void OnTopComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.TopComponent != null)
            {
                this.Component.Children[0] = this.TopComponent;
            }
            if (this.TopComponentChanged != null)
            {
                this.TopComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopComponent");
        }

        public event EventHandler TopComponentChanged;

        public UIComponentConfiguration BottomComponent
        {
            get
            {
                return this.GetValue(BottomComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(BottomComponentProperty, value);
            }
        }

        protected virtual void OnBottomComponentChanged()
        {
            if (this.Component != null && this.Component.Children.Count == 2 && this.BottomComponent != null)
            {
                this.Component.Children[1] = this.BottomComponent;
            }
            if (this.BottomComponentChanged != null)
            {
                this.BottomComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomComponent");
        }

        public event EventHandler BottomComponentChanged;

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
