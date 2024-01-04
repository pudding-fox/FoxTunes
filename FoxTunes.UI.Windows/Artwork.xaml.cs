using FoxTunes.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Artwork.xaml
    /// </summary>
    [UIComponent("66C8A9E7-0891-48DD-8086-E40F72D4D030", role: UIComponentRole.Info)]
    public partial class Artwork : SquareUIComponentBase, IDisposable
    {
        const int TIMEOUT = 100;

        public Artwork()
        {
            this.Debouncer = new AsyncDebouncer(TIMEOUT);
            this.InitializeComponent();
            this.OnFileNameChanged(this, EventArgs.Empty);
        }

        public AsyncDebouncer Debouncer { get; private set; }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Debouncer.Exec(this.Refresh);
        }

        protected virtual void OnFileNameChanged(object sender, EventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
            if (viewModel != null)
            {
                this.IsComponentEnabled = !string.IsNullOrEmpty(viewModel.FileName) && File.Exists(viewModel.FileName);
            }
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
                if (viewModel != null)
                {
                    viewModel.Emit();
                }
            });
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
            }
        }

        ~Artwork()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        protected virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.FindResource<global::FoxTunes.ViewModel.Artwork>("ViewModel");
            if (viewModel != null)
            {
                var task = viewModel.Next();
            }
        }
    }
}
