using FoxTunes.Interfaces;
using System;
using System.ComponentModel;

namespace FoxTunes
{
    [Serializable]
    public abstract class BaseComponent : IBaseComponent
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public virtual void InitializeComponent(ICore core)
        {
            //Nothing to do.
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
