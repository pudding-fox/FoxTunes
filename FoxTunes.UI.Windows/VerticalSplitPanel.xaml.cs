using FoxDb;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for VerticalSplitPanel.xaml
    /// </summary>
    public partial class VerticalSplitPanel : UserControl
    {
        public static readonly DependencyProperty ControlType1Property = DependencyProperty.Register(
            "ControlType1",
            typeof(Type),
            typeof(VerticalSplitPanel),
            new FrameworkPropertyMetadata(LayoutManager.PLACEHOLDER, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnControlType1Changed))
        );

        public static Type GetControlType1(VerticalSplitPanel source)
        {
            return (Type)source.GetValue(ControlType1Property);
        }

        public static void SetControlType1(VerticalSplitPanel source, Type value)
        {
            source.SetValue(ControlType1Property, value);
        }

        private static void OnControlType1Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var horizontalSplitPanel = sender as VerticalSplitPanel;
            if (horizontalSplitPanel == null)
            {
                return;
            }
            horizontalSplitPanel.OnControlType1Changed();
        }

        public static readonly DependencyProperty ControlType2Property = DependencyProperty.Register(
            "ControlType2",
            typeof(Type),
            typeof(VerticalSplitPanel),
            new FrameworkPropertyMetadata(LayoutManager.PLACEHOLDER, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnControlType2Changed))
        );

        public static Type GetControlType2(VerticalSplitPanel source)
        {
            return (Type)source.GetValue(ControlType2Property);
        }

        public static void SetControlType2(VerticalSplitPanel source, Type value)
        {
            source.SetValue(ControlType2Property, value);
        }

        private static void OnControlType2Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var horizontalSplitPanel = sender as VerticalSplitPanel;
            if (horizontalSplitPanel == null)
            {
                return;
            }
            horizontalSplitPanel.OnControlType2Changed();
        }

        public static readonly DependencyProperty SplitterHeightProperty = DependencyProperty.Register(
            "SplitterHeight",
            typeof(GridLength),
            typeof(VerticalSplitPanel),
            new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnSplitterHeightChanged))
        );

        public static GridLength GetSplitterHeight(VerticalSplitPanel source)
        {
            return (GridLength)source.GetValue(SplitterHeightProperty);
        }

        public static void SetSplitterHeight(VerticalSplitPanel source, GridLength value)
        {
            source.SetValue(SplitterHeightProperty, value);
        }

        private static void OnSplitterHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var horizontalSplitPanel = sender as VerticalSplitPanel;
            if (horizontalSplitPanel == null)
            {
                return;
            }
            horizontalSplitPanel.OnSplitterHeightChanged();
        }

        public VerticalSplitPanel()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
            this.RefreshLayout();
        }

        public Type ControlType1
        {
            get
            {
                return GetControlType1(this);
            }
            set
            {
                SetControlType1(this, value);
            }
        }

        protected virtual void OnControlType1Changed()
        {
            if (this.Component1 != null)
            {
                this.Component1.IsComponentEnabledChanged -= this.OnIsComponentEnabledChanged;
                this.Component1.IsComponentValidChanged -= this.OnIsComponentValidChanged;
                this.Component1 = null;
            }
            if (this.HasControlType1)
            {
                this.Component1 = ComponentActivator.Instance.Activate<UIComponentBase>(this.ControlType1);
                this.Component1.DataContext = this.DataContext;
                this.Component1.IsComponentEnabledChanged += this.OnIsComponentEnabledChanged;
                this.Component1.IsComponentValidChanged += this.OnIsComponentValidChanged;
            }
            this.RefreshLayout();
        }

        public bool HasControlType1
        {
            get
            {
                return this.ControlType1 != null && this.ControlType1 != LayoutManager.PLACEHOLDER;
            }
        }

        public UIComponentBase Component1 { get; private set; }

        public bool HasComponent1
        {
            get
            {
                return this.Component1 != null && this.Component1.IsComponentEnabled && this.Component1.IsComponentValid;
            }
        }

        public Type ControlType2
        {
            get
            {
                return GetControlType2(this);
            }
            set
            {
                SetControlType2(this, value);
            }
        }

        protected virtual void OnControlType2Changed()
        {
            if (this.Component2 != null)
            {
                this.Component2.IsComponentEnabledChanged -= this.OnIsComponentEnabledChanged;
                this.Component2.IsComponentValidChanged -= this.OnIsComponentValidChanged;
                this.Component2 = null;
            }
            if (this.HasControlType2)
            {
                this.Component2 = ComponentActivator.Instance.Activate<UIComponentBase>(this.ControlType2);
                this.Component2.DataContext = this.DataContext;
                this.Component2.IsComponentEnabledChanged += this.OnIsComponentEnabledChanged;
                this.Component2.IsComponentValidChanged += this.OnIsComponentValidChanged;
            }
            this.RefreshLayout();
        }

        public bool HasControlType2
        {
            get
            {
                return this.ControlType2 != null && this.ControlType2 != LayoutManager.PLACEHOLDER;
            }
        }

        public UIComponentBase Component2 { get; private set; }

        public bool HasComponent2
        {
            get
            {
                return this.Component2 != null && this.Component2.IsComponentEnabled && this.Component2.IsComponentValid;
            }
        }

        public GridLength SplitterHeight
        {
            get
            {
                return GetSplitterHeight(this);
            }
            set
            {
                SetSplitterHeight(this, value);
            }
        }

        protected virtual void OnSplitterHeightChanged()
        {

        }

        public bool HasSplitPanel
        {
            get
            {
                return this.Content is Grid;
            }
        }

        public bool IsRefreshingLayout { get; private set; }

        protected virtual void RefreshLayout()
        {
            if (this.IsRefreshingLayout)
            {
                return;
            }
            this.IsRefreshingLayout = true;
            try
            {
                if (this.HasComponent1 && this.HasComponent2)
                {
                    this.Component1.Disconnect();
                    this.Component2.Disconnect();
                    this.Component1.SetValue(Grid.RowProperty, 0);
                    this.Component1.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 4));
                    this.Component2.SetValue(Grid.RowProperty, 1);
                    this.Content = this.CreateSplitPanel();
                    this.Visibility = Visibility.Visible;
                }
                else if (this.HasComponent1 || this.HasComponent2)
                {
                    var component = this.HasComponent1 ? this.Component1 : this.Component2;
                    component.SetValue(FrameworkElement.MarginProperty, new Thickness());
                    component.Disconnect();
                    this.Content = component;
                    this.Visibility = Visibility.Visible;
                }
                else
                {
                    this.Visibility = Visibility.Collapsed;
                }
            }
            finally
            {
                this.IsRefreshingLayout = false;
            }
        }

        protected virtual object CreateSplitPanel()
        {
            return new Grid().With(grid =>
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition().With(rowDefinition =>
                {
                    rowDefinition.SetBinding(
                        RowDefinition.HeightProperty,
                        new Binding("SplitterHeight")
                        {
                            Source = this,
                            Mode = BindingMode.TwoWay
                        }
                    );
                }));
                grid.Children.Add(this.Component1);
                grid.Children.Add(new GridSplitter()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Height = 4
                });
                grid.Children.Add(this.Component2);
            });
        }

        protected virtual void OnIsComponentEnabledChanged(object sender, EventArgs e)
        {
            this.RefreshLayout();
        }

        protected virtual void OnIsComponentValidChanged(object sender, EventArgs e)
        {
            this.RefreshLayout();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Component1 != null)
            {
                this.Component1.DataContext = this.DataContext;
            }
            if (this.Component2 != null)
            {
                this.Component2.DataContext = this.DataContext;
            }
        }
    }
}
