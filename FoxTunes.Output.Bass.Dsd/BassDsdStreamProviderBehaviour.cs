using FoxTunes.Interfaces;
using ManagedBass.Dsd;
using System;

namespace FoxTunes
{
    public class BassDsdStreamProviderBehaviour : StandardBehaviour, IDisposable
    {
        public IBassOutput Output { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            BassDsd.DefaultFrequency = this.Output.Rate;
            Logger.Write(this, LogLevel.Debug, "BASS DSD Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
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
            if (this.Output != null)
            {
                this.Output.Init -= this.OnInit;
                this.Output.Free -= this.OnFree;
            }
        }

        ~BassDsdStreamProviderBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
