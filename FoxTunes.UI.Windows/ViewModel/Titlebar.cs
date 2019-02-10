using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Titlebar : ViewModelBase
    {
        public static readonly DependencyProperty WindowProperty = DependencyProperty.Register(
            "Window",
            typeof(Window),
            typeof(Titlebar),
            new PropertyMetadata(new PropertyChangedCallback(OnWindowChanged))
        );

        public static Window GetWindow(Titlebar source)
        {
            return (Window)source.GetValue(WindowProperty);
        }

        public static void SetWindow(Titlebar source, Window value)
        {
            source.SetValue(WindowProperty, value);
        }

        public static void OnWindowChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var titlebar = sender as Titlebar;
            if (titlebar == null)
            {
                return;
            }
            titlebar.OnWindowChanged();
        }

        public Window Window
        {
            get
            {
                return this.GetValue(WindowProperty) as Window;
            }
            set
            {
                this.SetValue(WindowProperty, value);
            }
        }

        protected virtual void OnWindowChanged()
        {
            this.OnTitleChanged();
            this.OnWindowStateChanged();
            if (this.WindowChanged != null)
            {
                this.WindowChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Window");
        }

        public event EventHandler WindowChanged;

        public Stream Icon
        {
            get
            {
                return typeof(Titlebar).Assembly.GetManifestResourceStream("FoxTunes.UI.Windows.Images.Fox.ico");
            }
        }

        public string Title
        {
            get
            {
                if (this.Window != null)
                {
                    return this.Window.Title;
                }
                return null;
            }
            set
            {
                if (this.Window != null)
                {
                    this.Window.Title = value;
                }
                this.OnTitleChanged();
            }
        }

        protected virtual void OnTitleChanged()
        {
            if (this.TitleChanged != null)
            {
                this.TitleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Title");
        }

        public event EventHandler TitleChanged;

        public WindowState WindowState
        {
            get
            {
                if (this.Window != null)
                {
                    return this.Window.WindowState;
                }
                return WindowState.Normal;
            }
            set
            {
                if (this.Window != null)
                {
                    this.Window.WindowState = value;
                }
                this.OnWindowStateChanged();
            }
        }

        protected virtual void OnWindowStateChanged()
        {
            if (this.WindowStateChanged != null)
            {
                this.WindowStateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("WindowState");
        }

        public event EventHandler WindowStateChanged;

        public ICommand MinimizeCommand
        {
            get
            {
                return new Command(this.Minimize);
            }
        }

        public void Minimize()
        {
            this.WindowState = WindowState.Minimized;
        }

        public ICommand MaximizeRestoreCommand
        {
            get
            {
                return new Command(this.MaximizeRestore);
            }
        }

        public void MaximizeRestore()
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    this.WindowState = WindowState.Normal;
                    break;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new Command(this.Close);
            }
        }

        public void Close()
        {
            if (this.Window != null)
            {
                this.Window.Close();
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Titlebar();
        }
    }
}
