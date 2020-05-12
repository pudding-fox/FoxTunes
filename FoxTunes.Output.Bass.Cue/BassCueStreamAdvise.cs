using System;

namespace FoxTunes
{
    public class BassCueStreamAdvice : BassStreamAdvice
    {
        public BassCueStreamAdvice(string fileName, string offset, string length)
        {
            this.FileName = fileName;
            this.Offset = CueSheetIndex.ToTimeSpan(offset);
            this.Length = CueSheetIndex.ToTimeSpan(length);
        }

        public override string FileName { get; protected set; }

        public override TimeSpan Offset { get; protected set; }

        public override TimeSpan Length { get; protected set; }
    }
}
