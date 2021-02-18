using FoxTunes.Interfaces;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class LyricsProvider : StandardComponent
    {
        protected LyricsProvider(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public LyricsBehaviour Behaviour { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Behaviour = ComponentRegistry.Instance.GetComponent<LyricsBehaviour>();
            if (this.Behaviour != null)
            {
                this.Behaviour.Register(this);
            }
            base.InitializeComponent(core);
        }

        public abstract Task<LyricsResult> Lookup(IFileData fileData);
    }
}
