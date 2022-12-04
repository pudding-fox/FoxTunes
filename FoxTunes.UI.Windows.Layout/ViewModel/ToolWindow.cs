using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class ToolWindow : ViewModelBase
    {
        public string Title
        {
            get
            {
                if (this.Configuration == null)
                {
                    return string.Empty;
                }
                return ToolWindowConfiguration.GetTitle(this.Configuration);
            }
            set
            {
                if (this.Configuration == null || string.Equals(this.Configuration.Title, value))
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
                if (this.Configuration == null || this.Configuration.Left == Convert.ToInt32(value))
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
                if (this.Configuration == null || this.Configuration.Top == Convert.ToInt32(value))
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
                if (this.Configuration == null || this.Configuration.Width == Convert.ToInt32(value))
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
                if (this.Configuration == null || this.Configuration.Height == Convert.ToInt32(value))
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

        public UIComponentConfiguration Component
        {
            get
            {
                if (this.Configuration == null)
                {
                    return null;
                }
                return this.Configuration.Component;
            }
            set
            {
                if (this.Configuration == null || object.ReferenceEquals(this.Configuration.Component, value))
                {
                    return;
                }
                this.Configuration.Component = value;
            }
        }

        protected virtual void OnComponentChanged(object sender, EventArgs e)
        {
            this.OnTitleChanged(this, EventArgs.Empty);
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        public bool ShowWithMainWindow
        {
            get
            {
                if (this.Configuration == null)
                {
                    return false;
                }
                return this.Configuration.ShowWithMainWindow;
            }
            set
            {
                if (this.Configuration == null || this.Configuration.ShowWithMainWindow == value)
                {
                    return;
                }
                this.Configuration.ShowWithMainWindow = value;
            }
        }

        protected virtual void OnShowWithMainWindowChanged(object sender, EventArgs e)
        {
            if (this.ShowWithMainWindowChanged != null)
            {
                this.ShowWithMainWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMainWindow");
        }

        public event EventHandler ShowWithMainWindowChanged;

        public bool ShowWithMiniWindow
        {
            get
            {
                if (this.Configuration == null)
                {
                    return false;
                }
                return this.Configuration.ShowWithMiniWindow;
            }
            set
            {
                if (this.Configuration == null || this.Configuration.ShowWithMiniWindow == value)
                {
                    return;
                }
                this.Configuration.ShowWithMiniWindow = value;
            }
        }

        protected virtual void OnShowWithMiniWindowChanged(object sender, EventArgs e)
        {
            if (this.ShowWithMiniWindowChanged != null)
            {
                this.ShowWithMiniWindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowWithMiniWindow");
        }

        public event EventHandler ShowWithMiniWindowChanged;

        public bool AlwaysOnTop
        {
            get
            {
                if (this.Configuration == null)
                {
                    return false;
                }
                return this.Configuration.AlwaysOnTop;
            }
            set
            {
                if (this.Configuration == null || this.Configuration.AlwaysOnTop == value)
                {
                    return;
                }
                this.Configuration.AlwaysOnTop = value;
            }
        }

        protected virtual void OnAlwaysOnTopChanged(object sender, EventArgs e)
        {
            if (this.AlwaysOnTopChanged != null)
            {
                this.AlwaysOnTopChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("AlwaysOnTop");
        }

        public event EventHandler AlwaysOnTopChanged;

        private ToolWindowConfiguration _Configuration { get; set; }

        public ToolWindowConfiguration Configuration
        {
            get
            {
                return this._Configuration;
            }
            set
            {
                if (object.ReferenceEquals(this.Configuration, value))
                {
                    return;
                }
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
                this.Configuration.ComponentChanged += this.OnComponentChanged;
                this.Configuration.ShowWithMainWindowChanged += this.OnShowWithMainWindowChanged;
                this.Configuration.ShowWithMiniWindowChanged += this.OnShowWithMiniWindowChanged;
                this.Configuration.AlwaysOnTopChanged += this.OnAlwaysOnTopChanged;

                this.OnTitleChanged(this, EventArgs.Empty);
                this.OnTopChanged(this, EventArgs.Empty);
                this.OnLeftChanged(this, EventArgs.Empty);
                this.OnWidthChanged(this, EventArgs.Empty);
                this.OnHeightChanged(this, EventArgs.Empty);
                this.OnComponentChanged(this, EventArgs.Empty);
                this.OnShowWithMainWindowChanged(this, EventArgs.Empty);
                this.OnShowWithMiniWindowChanged(this, EventArgs.Empty);
                this.OnAlwaysOnTopChanged(this, EventArgs.Empty);
            }
            if (this.ConfigurationChanged != null)
            {
                this.ConfigurationChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Configuration");
        }

        public event EventHandler ConfigurationChanged;

        protected override Freezable CreateInstanceCore()
        {
            return new ToolWindow();
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
                this.Configuration.ComponentChanged -= this.OnComponentChanged;
                this.Configuration.ShowWithMainWindowChanged -= this.OnShowWithMainWindowChanged;
                this.Configuration.ShowWithMiniWindowChanged -= this.OnShowWithMiniWindowChanged;
                this.Configuration.AlwaysOnTopChanged -= this.OnAlwaysOnTopChanged;
            }
            base.OnDisposing();
        }
    }
}
