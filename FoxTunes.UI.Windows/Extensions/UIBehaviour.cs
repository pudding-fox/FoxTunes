using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class UIBehaviour : IDisposable
    {
        static UIBehaviour()
        {
            Instances = new List<WeakReference<UIBehaviour>>();
        }

        private static IList<WeakReference<UIBehaviour>> Instances { get; set; }

        public static IEnumerable<UIBehaviour> Active
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

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        protected UIBehaviour()
        {
            lock (Instances)
            {
                Instances.Add(new WeakReference<UIBehaviour>(this));
            }
        }

        protected virtual void Dispatch(Func<Task> function)
        {
#if NET40
            var task = TaskEx.Run(function);
#else
            var task = Task.Run(function);
#endif
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
        }

        ~UIBehaviour()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        public static void Shutdown()
        {
            Logger.Write(typeof(UIBehaviour), LogLevel.Debug, "Shutting down..");
            foreach (var instance in Active)
            {
                try
                {
                    instance.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(UIBehaviour), LogLevel.Warn, "Instance failed to dispose: {0}", e.Message);
                }
            }
            if (Active.Any())
            {
                Logger.Write(typeof(UIBehaviour), LogLevel.Warn, "Some instances failed to disposed.");
            }
        }
    }
}
