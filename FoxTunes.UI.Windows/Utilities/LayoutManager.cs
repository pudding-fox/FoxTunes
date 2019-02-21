using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class LayoutManager : StandardComponent
    {
        public IConfiguration Configuration { get; private set; }

        private SelectionConfigurationElement _TopLeft { get; set; }

        public SelectionConfigurationElement TopLeft
        {
            get
            {
                return this._TopLeft;
            }
            set
            {
                this._TopLeft = value;
                this.OnTopLeftChanged();
            }
        }

        protected virtual void OnTopLeftChanged()
        {
            if (this.TopLeftChanged != null)
            {
                this.TopLeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopLeft");
        }

        public event EventHandler TopLeftChanged;

        private SelectionConfigurationElement _BottomLeft { get; set; }

        public SelectionConfigurationElement BottomLeft
        {
            get
            {
                return this._BottomLeft;
            }
            set
            {
                this._BottomLeft = value;
                this.OnBottomLeftChanged();
            }
        }

        protected virtual void OnBottomLeftChanged()
        {
            if (this.BottomLeftChanged != null)
            {
                this.BottomLeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomLeft");
        }

        public event EventHandler BottomLeftChanged;

        private SelectionConfigurationElement _TopCenter { get; set; }

        public SelectionConfigurationElement TopCenter
        {
            get
            {
                return this._TopCenter;
            }
            set
            {
                this._TopCenter = value;
                this.OnTopCenterChanged();
            }
        }

        protected virtual void OnTopCenterChanged()
        {
            if (this.TopCenterChanged != null)
            {
                this.TopCenterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopCenter");
        }

        public event EventHandler TopCenterChanged;

        private SelectionConfigurationElement _BottomCenter { get; set; }

        public SelectionConfigurationElement BottomCenter
        {
            get
            {
                return this._BottomCenter;
            }
            set
            {
                this._BottomCenter = value;
                this.OnBottomCenterChanged();
            }
        }

        protected virtual void OnBottomCenterChanged()
        {
            if (this.BottomCenterChanged != null)
            {
                this.BottomCenterChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomCenter");
        }

        public event EventHandler BottomCenterChanged;

        private SelectionConfigurationElement _TopRight { get; set; }

        public SelectionConfigurationElement TopRight
        {
            get
            {
                return this._TopRight;
            }
            set
            {
                this._TopRight = value;
                this.OnTopRightChanged();
            }
        }

        protected virtual void OnTopRightChanged()
        {
            if (this.TopRightChanged != null)
            {
                this.TopRightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("TopRight");
        }

        public event EventHandler TopRightChanged;

        private SelectionConfigurationElement _BottomRight { get; set; }

        public SelectionConfigurationElement BottomRight
        {
            get
            {
                return this._BottomRight;
            }
            set
            {
                this._BottomRight = value;
                this.OnBottomRightChanged();
            }
        }

        protected virtual void OnBottomRightChanged()
        {
            if (this.BottomRightChanged != null)
            {
                this.BottomRightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BottomRight");
        }

        public event EventHandler BottomRightChanged;

        public IEnumerable<Type> ActiveControls
        {
            get
            {
                var elements = new[]
                {
                    this.TopLeft,
                    this.BottomLeft,
                    this.TopCenter,
                    this.BottomCenter,
                    this.TopRight,
                    this.BottomRight
                };
                foreach (var element in elements)
                {
                    var control = WindowsUserInterfaceConfiguration.GetControl(element.Value);
                    if (control != null)
                    {
                        yield return control;
                    }
                }
            }
        }

        protected virtual void OnActiveControlsChanged()
        {
            if (this.ActiveControlsChanged != null)
            {
                this.ActiveControlsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ActiveControls");
        }

        public event EventHandler ActiveControlsChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.TopLeft = this.Configuration.GetElement<SelectionConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.TOP_LEFT_ELEMENT
            );
            this.BottomLeft = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_LEFT_ELEMENT
            );
            this.TopCenter = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.TOP_CENTER_ELEMENT
            );
            this.BottomCenter = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_CENTER_ELEMENT
            );
            this.TopRight = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.TOP_RIGHT_ELEMENT
            );
            this.BottomRight = this.Configuration.GetElement<SelectionConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.BOTTOM_RIGHT_ELEMENT
            );
            this.TopLeft.ValueChanged += this.OnLayoutUpdated;
            this.BottomLeft.ValueChanged += this.OnLayoutUpdated;
            this.TopCenter.ValueChanged += this.OnLayoutUpdated;
            this.BottomCenter.ValueChanged += this.OnLayoutUpdated;
            this.TopRight.ValueChanged += this.OnLayoutUpdated;
            this.BottomRight.ValueChanged += this.OnLayoutUpdated;
            base.InitializeComponent(core);
        }

        protected virtual void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.OnActiveControlsChanged();
        }
    }
}
