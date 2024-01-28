using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Main : ViewModelBase
    {
        public IConfiguration Configuration { get; private set; }

        private BooleanConfigurationElement _ShowLibrary { get; set; }

        public BooleanConfigurationElement ShowLibrary
        {
            get
            {
                return this._ShowLibrary;
            }
            set
            {
                this._ShowLibrary = value;
                this.OnShowLibraryChanged();
            }
        }

        protected virtual void OnShowLibraryChanged()
        {
            if (this.ShowLibraryChanged != null)
            {
                this.ShowLibraryChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowLibrary");
        }

        public event EventHandler ShowLibraryChanged = delegate { };

        private BooleanConfigurationElement _ShowArtwork { get; set; }

        public BooleanConfigurationElement ShowArtwork
        {
            get
            {
                return this._ShowArtwork;
            }
            set
            {
                this._ShowArtwork = value;
                this.OnShowArtworkChanged();
            }
        }

        protected virtual void OnShowArtworkChanged()
        {
            if (this.ShowArtworkChanged != null)
            {
                this.ShowArtworkChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowArtwork");
        }

        public event EventHandler ShowArtworkChanged = delegate { };

        protected override void OnCoreChanged()
        {
            this.Configuration = this.Core.Components.Configuration;
            this.ShowLibrary = this.Configuration.GetElement<BooleanConfigurationElement>(
               WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
               WindowsUserInterfaceConfiguration.SHOW_LIBRARY
            );
            this.ShowArtwork = this.Configuration.GetElement<BooleanConfigurationElement>(
              WindowsUserInterfaceConfiguration.APPEARANCE_SECTION,
              WindowsUserInterfaceConfiguration.SHOW_ARTWORK_ELEMENT
           );
            base.OnCoreChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Main();
        }
    }
}
