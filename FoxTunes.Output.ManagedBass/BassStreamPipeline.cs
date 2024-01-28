using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class BassStreamPipeline : BaseComponent, IBassStreamPipeline
    {
        public static readonly object SyncRoot = new object();

        public BassStreamPipeline(IBassStreamInput input, IEnumerable<IBassStreamComponent> components, IBassStreamOutput output)
        {
            this.Input = input;
            this.Components = components;
            this.Output = output;
        }

        public IBassStreamInput Input { get; private set; }

        public IEnumerable<IBassStreamComponent> Components { get; private set; }

        public IBassStreamOutput Output { get; private set; }

        public IEnumerable<IBassStreamComponent> All
        {
            get
            {
                yield return this.Input;
                foreach (var component in this.Components)
                {
                    yield return component;
                }
                yield return this.Output;
            }
        }

        public IEnumerable<IBassStreamControllable> Controllable
        {
            get
            {
                return this.All.OfType<IBassStreamControllable>();
            }
        }

        public long BufferLength
        {
            get
            {
                return this.All.Sum(component => component.BufferLength);
            }
        }

        public void Connect()
        {
            var previous = (IBassStreamComponent)this.Input;
            this.Input.Connect(previous);
            foreach (var component in this.Components)
            {
                component.Connect(previous);
                previous = component;
            }
            this.Output.Connect(previous);
        }

        public void ClearBuffer()
        {
            this.All.ForEach(component => component.ClearBuffer());
        }

        public void Play()
        {
            this.Controllable.ForEach(component => component.Play());
        }

        public void Pause()
        {
            this.Controllable.Reverse().ForEach(component => component.Pause());
        }

        public void Resume()
        {
            this.Controllable.ForEach(component => component.Resume());
        }

        public void Stop()
        {
            this.Controllable.Reverse().ForEach(component => component.Stop());
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
            this.All.ForEach(component =>
            {
                try
                {
                    component.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Component \"{0}\" could not be disposed: {1}", component.GetType().Name, e.Message);
                }
            });
            this.IsDisposed = true;
        }

        ~BassStreamPipeline()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
