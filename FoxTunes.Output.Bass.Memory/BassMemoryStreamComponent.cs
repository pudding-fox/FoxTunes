using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Memory;

namespace FoxTunes
{
    public class BassMemoryStreamComponent : BassStreamComponent
    {
        public BassMemoryStreamComponent(BassMemoryBehaviour behaviour, BassOutputStream stream, IBassStreamPipelineQueryResult query)
        {
            this.Behaviour = behaviour;
            this.Query = query;
            this.Rate = behaviour.Output.Rate;
            this.Channels = stream.Channels;
            this.Flags = BassFlags.Decode;
            if (this.Behaviour.Output.Float)
            {
                this.Flags |= BassFlags.Float;
            }
        }

        public override string Name
        {
            get
            {
                return Strings.BassMemoryStreamComponent_Name;
            }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    "{0} ({1} MB)",
                    this.Name,
                    BassMemory.Usage() / 1000000
                );
            }
        }

        public BassMemoryBehaviour Behaviour { get; private set; }

        public IBassStreamPipelineQueryResult Query { get; private set; }

        public override int Rate { get; protected set; }

        public override int Channels { get; protected set; }

        public override BassFlags Flags { get; protected set; }

        public override int ChannelHandle { get; protected set; }

        public override bool IsActive
        {
            get
            {
                return BassMemory.Usage() > 0;
            }
        }

        public override void Connect(IBassStreamComponent previous)
        {
            this.ChannelHandle = previous.ChannelHandle;
        }

        protected override void OnDisposing()
        {
            //Nothing to do.
        }

        public static bool ShouldCreate(BassMemoryBehaviour behaviour, BassOutputStream stream, IBassStreamPipelineQueryResult query)
        {
            if (BassUtils.GetChannelDsdRaw(stream.ChannelHandle))
            {
                //Only PCM.
                return false;
            }
            return true;
        }
    }
}
