using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Data;

namespace FoxTunes
{
    public class UIComponentRoot : UIComponentPanel, IDisposable
    {
        static UIComponentRoot()
        {
            Instances = new List<WeakReference<UIComponentRoot>>();
        }

        private static IList<WeakReference<UIComponentRoot>> Instances { get; set; }

        public static IEnumerable<UIComponentRoot> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(UIComponentRoot sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public UIComponentRoot()
        {
            var container = new UIComponentContainer();
            //TODO: Should we create this binding now or on Loaded?
            container.SetBinding(
                UIComponentContainer.ComponentProperty,
                new Binding()
                {
                    Source = this,
                    Path = new PropertyPath(nameof(this.Component))
                }
            );
            this.Content = container;
            lock (Instances)
            {
                Instances.Add(new WeakReference<UIComponentRoot>(this));
            }
            OnActiveChanged(this);
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
            lock (Instances)
            {
                for (var a = Instances.Count - 1; a >= 0; a--)
                {
                    var instance = Instances[a];
                    if (instance == null || !instance.IsAlive)
                    {
                        Instances.RemoveAt(a);
                    }
                    else if (object.ReferenceEquals(this, instance.Target))
                    {
                        Instances.RemoveAt(a);
                    }
                }
            }
            OnActiveChanged(this);
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
