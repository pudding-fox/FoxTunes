using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ControlSlot.xaml
    /// </summary>
    public partial class ControlSlot : UserControl
    {
        public static readonly DependencyProperty ControlTypeProperty = DependencyProperty.Register(
            "ControlType",
            typeof(Type),
            typeof(ControlSlot),
            new FrameworkPropertyMetadata(LayoutManager.PLACEHOLDER, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnControlTypeChanged))
        );

        public static Type GetControlType(ControlSlot source)
        {
            return (Type)source.GetValue(ControlTypeProperty);
        }

        public static void SetControlType(ControlSlot source, Type value)
        {
            source.SetValue(ControlTypeProperty, value);
        }

        private static void OnControlTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var controlSlot = sender as ControlSlot;
            if (controlSlot == null)
            {
                return;
            }
            controlSlot.OnControlTypeChanged();
        }

        public ControlSlot()
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
            set
            {
                SetControlType(this, value);
            }
        }

        protected virtual void OnControlTypeChanged()
        {
            this.RefreshLayout();
        }

        public bool HasControlType
        {
            get
            {
                return this.ControlType != null && this.ControlType != LayoutManager.PLACEHOLDER;
            }
        }

        protected virtual void RefreshLayout()
        {
            if (this.HasControlType)
            {
                this.Visibility = Visibility.Visible;
                this.Content = ComponentActivator.Instance.Activate<IUIComponent>(this.ControlType);
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                this.Content = null;
            }
        }
    }
}
