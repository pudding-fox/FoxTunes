using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Tool : ViewModelBase
    {
        public string Title
        {
            get
            {
                if (this.Configuration == null)
                {
                    return null;
                }
                return this.Configuration.Title;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Title = value;
            }
        }

        protected virtual void OnTitleChanged(object sender, EventArgs e)
        {
            if (this.TitleChanged != null)
            {
                this.TitleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Title");
        }

        public event EventHandler TitleChanged;

        public double Left
        {
            get
            {
                if (this.Configuration == null || this.Configuration.Left == 0)
                {
                    return double.NaN;
                }
                return this.Configuration.Left;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Left = Convert.ToInt32(value);
            }
        }

        protected virtual void OnLeftChanged(object sender, EventArgs e)
        {
            if (this.LeftChanged != null)
            {
                this.LeftChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Left");
        }

        public event EventHandler LeftChanged;

        public double Top
        {
            get
            {
                if (this.Configuration == null || this.Configuration.Top == 0)
                {
                    return double.NaN;
                }
                return this.Configuration.Top;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Top = Convert.ToInt32(value);
            }
        }

        protected virtual void OnTopChanged(object sender, EventArgs e)
        {
            if (this.TopChanged != null)
            {
                this.TopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Top");
        }

        public event EventHandler TopChanged;

        public double Width
        {
            get
            {
                if (this.Configuration == null || this.Configuration.Width <= 0)
                {
                    return double.NaN;
                }
                return this.Configuration.Width;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Width = Convert.ToInt32(value);
            }
        }

        protected virtual void OnWidthChanged(object sender, EventArgs e)
        {
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public double Height
        {
            get
            {
                if (this.Configuration == null || this.Configuration.Height <= 0)
                {
                    return double.NaN;
                }
                return this.Configuration.Height;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Height = Convert.ToInt32(value);
            }
        }

        protected virtual void OnHeightChanged(object sender, EventArgs e)
        {
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        public string Content
        {
            get
            {
                if (this.Configuration == null)
                {
                    return null;
                }
                return this.Configuration.Content;
            }
            set
            {
                if (this.Configuration == null)
                {
                    return;
                }
                this.Configuration.Content = value;
            }
        }

        protected virtual void OnContentChanged(object sender, EventArgs e)
        {
            if (this.ContentChanged != null)
            {
                this.ContentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Content");
        }

        public event EventHandler ContentChanged;

        private ToolWindowConfiguration _Configuration { get; set; }

        public ToolWindowConfiguration Configuration
        {
            get
            {
                return this._Configuration;
            }
            set
            {
                this._Configuration = value;
                this.OnConfigurationChanged();
            }
        }

        protected virtual void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Configuration.TitleChanged += this.OnTitleChanged;
                this.Configuration.TopChanged += this.OnTopChanged;
                this.Configuration.LeftChanged += this.OnLeftChanged;
                this.Configuration.WidthChanged += this.OnWidthChanged;
                this.Configuration.HeightChanged += this.OnHeightChanged;
                this.Configuration.ContentChanged += this.OnContentChanged;

                this.OnTitleChanged(this, EventArgs.Empty);
                this.OnTopChanged(this, EventArgs.Empty);
                this.OnLeftChanged(this, EventArgs.Empty);
                this.OnWidthChanged(this, EventArgs.Empty);
                this.OnHeightChanged(this, EventArgs.Empty);
                this.OnContentChanged(this, EventArgs.Empty);
            }
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Configuration");
        }

        public event EventHandler ConfigurationChanged;

        private DoubleConfigurationElement _ScalingFactor { get; set; }

        public DoubleConfigurationElement ScalingFactor
        {
            get
            {
                return this._ScalingFactor;
            }
            set
            {
                this._ScalingFactor = value;
                this.OnScalingFactorChanged();
            }
        }

        protected virtual void OnScalingFactorChanged()
        {
            if (this.ScalingFactorChanged != null)
            {
                this.ScalingFactorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ScalingFactor");
        }

        public event EventHandler ScalingFactorChanged;

        public override void InitializeComponent(ICore core)
        {
            this.ScalingFactor = core.Components.Configuration.GetElement<DoubleConfigurationElement>(
              WindowsUserInterfaceConfiguration.SECTION,
              WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Tool();
        }

        protected override void OnDisposing()
        {
            if (this.Configuration != null)
            {
                this.Configuration.TitleChanged -= this.OnTitleChanged;
                this.Configuration.TopChanged -= this.OnTopChanged;
                this.Configuration.LeftChanged -= this.OnLeftChanged;
                this.Configuration.WidthChanged -= this.OnWidthChanged;
                this.Configuration.HeightChanged -= this.OnHeightChanged;
                this.Configuration.ContentChanged -= this.OnContentChanged;
            }
            base.OnDisposing();
        }
    }
}
