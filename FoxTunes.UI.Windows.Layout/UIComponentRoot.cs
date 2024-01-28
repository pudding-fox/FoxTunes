using FoxTunes.Interfaces;
using System;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public class UIComponentRoot : UIComponentPanel, IDisposable
    {
        new public static event EventHandler Loaded;

        new public static event EventHandler Unloaded;

        public UIComponentRoot()
        {
            var container = new UIComponentContainer();
            container.SetBinding(
                UIComponentContainer.ComponentProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(nameof(this.Component))
                }
            );
            this.Content = container;
            base.Loaded += this.OnLoaded;
            base.Unloaded += this.OnUnloaded;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Loaded == null)
            {
                return;
            }
            Loaded(this, EventArgs.Empty);
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Unloaded == null)
            {
                return;
            }
            Unloaded(this, EventArgs.Empty);
        }

        protected override void CreateBindings()
        {
            //Nothing to do.
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
            //TODO: Cannot unsubscribe from GC thread.
            //base.Loaded -= this.OnLoaded;
            //base.Unloaded -= this.OnUnloaded;
        }

        ~UIComponentRoot()
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
    }
}
