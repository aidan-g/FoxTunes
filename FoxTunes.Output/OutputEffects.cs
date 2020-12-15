using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class OutputEffects : StandardComponent, IOutputEffects
    {
        public IOutputVolume Volume { get; private set; }

        public IOutputEqualizer Equalizer { get; private set; }

        public IOutputTempo Tempo { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Volume = ComponentRegistry.Instance.GetComponent<IOutputVolume>();
            this.Equalizer = ComponentRegistry.Instance.GetComponent<IOutputEqualizer>();
            this.Tempo = ComponentRegistry.Instance.GetComponent<IOutputTempo>();
            base.InitializeComponent(core);
        }
    }
}
