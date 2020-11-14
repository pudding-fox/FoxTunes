using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputTempo : IOutputEffect
    {
        float Tempo { get; set; }

        event EventHandler TempoChanged;

        float Pitch { get; set; }

        event EventHandler PitchChanged;

        float Rate { get; set; }

        event EventHandler RateChanged;

        bool AAFilter { get; set; }

        event EventHandler AAFilterChanged;

        byte AAFilterLength { get; set; }

        event EventHandler AAFilterLengthChanged;
    }
}
