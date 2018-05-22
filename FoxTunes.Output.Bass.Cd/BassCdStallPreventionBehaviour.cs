using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoxTunes.Interfaces;
using ManagedBass.Cd;
using ManagedBass.Gapless;

namespace FoxTunes
{
    public class BassCdStallPreventionBehaviour : StandardBehaviour
    {
        const int NO_TRACK = -1;

        public IBassOutput Output { get; private set; }

        public BassCdStreamProviderBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Behaviour = ComponentRegistry.Instance.GetComponent<BassCdStreamProviderBehaviour>();
            //TODO: Assuming we're using gapless input.
            ComponentRegistry.Instance.GetComponent<BassGaplessStreamInputBehaviour>().BassGaplessEvent += this.OnBassGaplessEvent;
            base.InitializeComponent(core);
        }

        protected virtual void OnBassGaplessEvent(object sender, BassGaplessEventArgs e)
        {
            switch (e.EventType)
            {
                case BassGaplessEventType.End:
                    this.OnStall(e.Channel1);
                    break;
            }
        }

        protected virtual void OnStall(int channelHandle)
        {
            var track = BassCd.StreamGetTrack(channelHandle);
            if (track == NO_TRACK)
            {
                return;
            }
            if (track >= BassCd.GetTracks(this.Behaviour.CdDrive))
            {
                return;
            }
            var provider = this.Output.StreamFactory.Providers.OfType<BassCdStreamProvider>().FirstOrDefault();
            if (provider == null)
            {
                return;
            }
            this.Output.FreeStream(channelHandle);
            channelHandle = provider.CreateStream(this.Output, this.Behaviour.CdDrive, track + 1, true);
            if (channelHandle == 0)
            {
                return;
            }
            this.Output.Pipeline.Input.Add(channelHandle);
        }
    }
}
