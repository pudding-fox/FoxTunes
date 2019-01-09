using FoxTunes.Interfaces;
using ManagedBass.Gapless;
using System;

namespace FoxTunes
{
    public class BassGaplessStreamInputBehaviour : StandardBehaviour
    {
        public IBassOutput Output { get; private set; }

        new public bool IsInitialized { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>().CreatingPipeline += this.OnCreatingPipeline;
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            BassUtils.OK(BassGapless.Init());
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS GAPLESS Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            Logger.Write(this, LogLevel.Debug, "Releasing BASS GAPLESS.");
            BassGapless.Free();
            this.IsInitialized = false;
        }

        protected virtual void OnCreatingPipeline(object sender, CreatingPipelineEventArgs e)
        {
            e.Input = new BassGaplessStreamInput(this, e.Stream);
        }
    }

    public delegate void BassGaplessEventHandler(object sender, BassGaplessEventArgs e);
}