using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Titlebar : ViewModelBase
    {
        public WindowState WindowState
        {
            get
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    return Application.Current.MainWindow.WindowState;
                }
                return WindowState.Normal;
            }
            set
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.WindowState = value;
                }
            }
        }

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
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Titlebar();
        }
    }
}
