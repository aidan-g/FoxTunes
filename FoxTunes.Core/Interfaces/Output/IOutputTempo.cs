using System;

namespace FoxTunes.Interfaces
{
    public interface IOutputTempo : IOutputEffect
    {
        int MinValue { get; }

        int MaxValue { get; }

        int Value { get; set; }

        event EventHandler ValueChanged;

        int MinPitch { get; }

        int MaxPitch { get; }

        int Pitch { get; set; }

        event EventHandler PitchChanged;

        int MinRate { get; }

        int MaxRate { get; }

        int Rate { get; set; }

        event EventHandler RateChanged;
    }
}
