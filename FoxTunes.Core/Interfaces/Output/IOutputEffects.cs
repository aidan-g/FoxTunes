namespace FoxTunes.Interfaces
{
    public interface IOutputEffects : IStandardComponent
    {
        IOutputVolume Volume { get; }

        IOutputEqualizer Equalizer { get; }

        IOutputTempo Tempo { get; }
    }
}
