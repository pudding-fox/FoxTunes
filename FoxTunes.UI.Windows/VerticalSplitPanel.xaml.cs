using System;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for VerticalSplitPanel.xaml
    /// </summary>
    public partial class VerticalSplitPanel : UserControl
    {
        public static readonly DependencyPropertyKey ControlTypePropertyKey = DependencyProperty.RegisterReadOnly(
            "ControlType",
            typeof(Type),
            typeof(VerticalSplitPanel),
            new FrameworkPropertyMetadata(typeof(object), FrameworkPropertyMetadataOptions.None)
        );

        public static readonly DependencyProperty ControlTypeProperty = ControlTypePropertyKey.DependencyProperty;

        public static Type GetControlType(VerticalSplitPanel source)
        {
            return (Type)source.GetValue(ControlTypeProperty);
        }

        protected static void SetControlType(VerticalSplitPanel source, Type value)
        {
            source.SetValue(ControlTypePropertyKey, value);
        }

        public static readonly DependencyProperty ControlType1Property = DependencyProperty.Register(
            "ControlType1",
            typeof(Type),
            typeof(VerticalSplitPanel),
            new FrameworkPropertyMetadata(typeof(object), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnControlType1Changed))
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
            new FrameworkPropertyMetadata(typeof(object), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnControlType2Changed))
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
            this.RefreshLayout();
        }

        public Type ControlType
        {
            get
            {
                return GetControlType(this);
            }
            protected set
            {
                SetControlType(this, value);
            }
        }

        protected virtual void OnControlTypeChanged()
        {

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
            this.RefreshLayout();
        }

        public bool HasControlType1
        {
            get
            {
                return this.ControlType1 != null && this.ControlType1 != typeof(object);
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
            this.RefreshLayout();
        }

        public bool HasControlType2
        {
            get
            {
                return this.ControlType2 != null && this.ControlType2 != typeof(object);
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

        protected virtual void RefreshLayout()
        {
            if (this.HasControlType1 && this.HasControlType2)
            {
                this.Style = this.FindResource<Style>("SplitStyle");
                this.ControlType = typeof(object);
                this.Visibility = Visibility.Visible;
            }
            else if (this.HasControlType1)
            {
                this.Style = this.FindResource<Style>("FillStyle");
                this.ControlType = this.ControlType1;
                this.Visibility = Visibility.Visible;
                //TODO: For some stupid fucking reason the binding does not work sometimes.
                //TODO: I have no idea why but spent hours trying to fix it.
                ((ControlSlot)this.Content).ControlType = this.ControlType1;
            }
            else if (this.HasControlType2)
            {
                this.Style = this.FindResource<Style>("FillStyle");
                this.ControlType = this.ControlType2;
                this.Visibility = Visibility.Visible;
                //TODO: For some stupid fucking reason the binding does not work sometimes.
                //TODO: I have no idea why but spent hours trying to fix it.
                ((ControlSlot)this.Content).ControlType = this.ControlType2;
            }
            else
            {
                this.Style = null;
                this.ControlType = typeof(object);
                this.Visibility = Visibility.Collapsed;
            }
        }
    }
}
