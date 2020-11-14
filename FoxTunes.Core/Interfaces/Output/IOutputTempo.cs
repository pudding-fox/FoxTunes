using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputTempo : IOutputEffect
    {
        int Tempo { get; set; }

        event EventHandler TempoChanged;

        int Pitch { get; set; }

        event EventHandler PitchChanged;

        int Rate { get; set; }

        event EventHandler RateChanged;

        bool AAFilter { get; set; }

        event EventHandler AAFilterChanged;

        byte AAFilterLength { get; set; }

        event EventHandler AAFilterLengthChanged;
    }
}
