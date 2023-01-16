using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LayoutTree : ViewModelBase
    {
        private ObservableCollection<UIComponentConfiguration> _Components { get; set; }

        public ObservableCollection<UIComponentConfiguration> Components
        {
            get
            {
                return this._Components;
            }
            set
            {
                if (object.ReferenceEquals(this.Components, value))
                {
                    return;
                }
                this._Components = value;
                this.OnComponentsChanged();
            }
        }

        protected virtual void OnComponentsChanged()
        {
            if (this.ComponentsChanged != null)
            {
                this.ComponentsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Components");
        }

        public event EventHandler ComponentsChanged;

        protected override Freezable CreateInstanceCore()
        {
            return new LayoutTree();
        }
    }
}
