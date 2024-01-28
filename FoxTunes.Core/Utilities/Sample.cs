using System;
using System.Linq;

namespace FoxTunes
{
    public class Metric
    {
        public Metric(int capacity, long? @default = null)
        {
            this.Measures = new long?[capacity];
            for (var a = 0; a < this.Measures.Length; a++)
            {
                this.Measures[a] = @default;
            }
        }

        private long?[] Measures { get; set; }

        private int Position { get; set; }

        public long Average(int value)
        {
            this.Measures[this.Position++] = value;
            if (this.Position >= this.Measures.Length)
            {
                this.Position = 0;
            }
            return this.Average();
        }

        private long Average()
        {
            return Convert.ToInt64(Math.Ceiling(this.Measures.Where(measure => measure.HasValue).Average(measure => measure.Value)));
        }
    }
}
