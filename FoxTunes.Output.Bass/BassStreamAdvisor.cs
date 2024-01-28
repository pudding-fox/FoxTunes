using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class BassStreamAdvisor : StandardComponent, IBassStreamAdvisor
    {
        public IBassStreamFactory StreamFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.StreamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            if (this.StreamFactory != null)
            {
                this.StreamFactory.Register(this);
            }
            base.InitializeComponent(core);
        }

        public abstract void Advise(IBassStreamProvider provider, PlaylistItem playlistItem, IList<IBassStreamAdvice> advice, BassStreamUsageType type);
    }
}
