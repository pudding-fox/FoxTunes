using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class BassStreamInput : BassStreamComponent, IBassStreamInput
    {
        protected BassStreamInput(BassFlags flags) : base(flags)
        {

        }

        public abstract IEnumerable<int> Queue { get; }

        public virtual bool PreserveBuffer
        {
            get
            {
                return false;
            }
        }

        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        public abstract bool CheckFormat(BassOutputStream stream);

        public override void Connect(IBassStreamComponent previous)
        {
            throw new NotImplementedException();
        }

        public abstract void Connect(BassOutputStream stream);

        public abstract bool Contains(BassOutputStream stream);

        public abstract int Position(BassOutputStream stream);

        public abstract bool Add(BassOutputStream stream);

        public abstract bool Remove(BassOutputStream stream, Action<BassOutputStream> callBack);

        public abstract void Reset();
    }
}
