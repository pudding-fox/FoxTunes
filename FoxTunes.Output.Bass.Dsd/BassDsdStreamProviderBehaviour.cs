using System;
using FoxTunes.Interfaces;
using ManagedBass.Dsd;

namespace FoxTunes
{
    public class BassDsdStreamProviderBehaviour : StandardBehaviour
    {
        public IBassOutput Output { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            ComponentRegistry.Instance.GetComponent<IBassStreamFactory>().Register(new BassDsdStreamProvider());
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
    }
}
