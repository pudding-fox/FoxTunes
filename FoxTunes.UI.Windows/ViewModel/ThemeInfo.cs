using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class ThemeInfo : ViewModelBase
    {
        public CornerRadius CornerRadius
        {
            get
            {
                if (this.ThemeLoader == null || this.ThemeLoader.Theme == null)
                {
                    return default(CornerRadius);
                }
                return new CornerRadius(this.ThemeLoader.Theme.CornerRadius);
            }
        }

        protected virtual void OnCornerRadiusChanged()
        {
            if (this.CornerRadiusChanged != null)
            {
                this.CornerRadiusChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("CornerRadius");
        }

        public event EventHandler CornerRadiusChanged;

        public ThemeLoader ThemeLoader { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ThemeLoader.ThemeChanged += this.OnThemeChanged;
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnThemeChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected virtual void Refresh()
        {
            this.OnCornerRadiusChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ThemeInfo();
        }

        protected override void OnDisposing()
        {
            if (this.ThemeLoader != null)
            {
                this.ThemeLoader.ThemeChanged -= this.OnThemeChanged;
            }
            base.OnDisposing();
        }
    }
}
