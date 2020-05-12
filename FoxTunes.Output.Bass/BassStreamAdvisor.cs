using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class BassStreamAdvisor : StandardComponent, IBassStreamAdvisor
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public virtual byte Priority
        {
            get
            {
                return PRIORITY_NORMAL;
            }
        }

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

        public abstract bool Advice(PlaylistItem playlistItem, out IBassStreamAdvice Advice);
    }
}
