using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class BassStreamAdvice : BaseComponent, IBassStreamAdvice
    {
        public abstract string FileName { get; protected set; }

        public abstract TimeSpan Offset { get; protected set; }

        public abstract TimeSpan Length { get; protected set; }
    }
}
