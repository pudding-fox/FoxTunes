using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes.ViewModel
{
    public class Titlebar : ViewModelBase
    {
        public Titlebar()
        {
            Windows.MainWindowCreated += this.OnMainWindowCreated;
        }

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
                if (Windows.IsMainWindowCreated)
                {
                    return Windows.MainWindow.Title;
                }
                return null;
            }
            set
            {
                if (Windows.IsMainWindowCreated)
                {
                    Windows.MainWindow.Title = value;
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
                if (Windows.IsMainWindowCreated)
                {
                    return Windows.MainWindow.WindowState;
                }
                return WindowState.Normal;
            }
            set
            {
                if (Windows.IsMainWindowCreated)
                {
                    Windows.MainWindow.WindowState = value;
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
            if (Windows.IsMainWindowCreated)
            {
                Windows.MainWindow.Close();
            }
        }

        protected virtual void OnMainWindowCreated(object sender, EventArgs e)
        {
            this.OnTitleChanged();
            this.OnWindowStateChanged();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Titlebar();
        }
    }
}
