using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Wrapper : ViewModelBase
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(Wrapper),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged))
        );

        public static object GetValue(Wrapper source)
        {
            return source.GetValue(ValueProperty);
        }

        public static void SetValue(Wrapper source, object value)
        {
            source.SetValue(ValueProperty, value);
        }

        public static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var wrapper = sender as Wrapper;
            if (wrapper == null)
            {
                return;
            }
            wrapper.OnValueChanged();
        }

        public object Value
        {
            get
            {
                return this.GetValue(ValueProperty);
            }
            set
            {
                this.SetValue(ValueProperty, value);
            }
        }

        protected virtual void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Value");
        }

        public event EventHandler ValueChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new Wrapper();
        }
    }
}
