using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public abstract class ViewModelBase : Freezable, INotifyPropertyChanged
    {
        public static readonly DependencyProperty CoreProperty = DependencyProperty.Register(
            "Core",
            typeof(ICore),
            typeof(ViewModelBase),
            new PropertyMetadata(new PropertyChangedCallback(OnCoreChanged))
        );

        public static ICore GetCore(ViewModelBase source)
        {
            return (ICore)source.GetValue(CoreProperty);
        }

        public static void SetCore(ViewModelBase source, ICore value)
        {
            source.SetValue(CoreProperty, value);
        }

        public static void OnCoreChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = sender as ViewModelBase;
            if (viewModel == null)
            {
                return;
            }
            viewModel.OnCoreChanged();
        }

        public ICore Core
        {
            get
            {
                return this.GetValue(CoreProperty) as ICore;
            }
            set
            {
                this.SetValue(CoreProperty, value);
            }
        }

        protected virtual void OnCoreChanged()
        {
            if (this.CoreChanged != null)
            {
                this.CoreChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Core");
        }

        public event EventHandler CoreChanged = delegate { };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
