using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes
{
    public abstract class UIComponentPanel : UIComponentBase, IUIComponentPanel
    {
        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component",
            typeof(UIComponentConfiguration),
            typeof(UIComponentPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnComponentChanged))
        );

        public static UIComponentConfiguration GetComponent(UIComponentPanel source)
        {
            return (UIComponentConfiguration)source.GetValue(ComponentProperty);
        }

        public static void SetComponent(UIComponentPanel source, UIComponentConfiguration value)
        {
            source.SetValue(ComponentProperty, value);
        }

        public static void OnComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentPanel;
            if (container == null)
            {
                return;
            }
            container.OnComponentChanged();
        }

        public UIComponentConfiguration Component
        {
            get
            {
                return this.GetValue(ComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(ComponentProperty, value);
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
    }
}
