using FoxDb;
using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace FoxTunes
{
    [Component("C0B2450C-DEDA-4D8B-8A32-5EA733F1FD45", null, priority: ComponentAttribute.PRIORITY_LOW)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaybackStateBehaviour : StandardBehaviour, IUIPlaylistColumnProvider, IDatabaseInitializer
    {
        public string Id
        {
            get
            {
                return typeof(PlaybackStateBehaviour).AssemblyQualifiedName;
            }
        }

        public string Name
        {
            get
            {
                return "Playback State";
            }
        }

        public string Description
        {
            get
            {
                return null;
            }
        }

        public DataTemplate CellTemplate
        {
            get
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Resources.PlaybackState)))
                {
                    var template = (DataTemplate)XamlReader.Load(stream);
                    template.Seal();
                    return template;
                }
            }
        }

        public IEnumerable<string> MetaData
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public string Checksum
        {
            get
            {
                return "5815AE1E-A96C-4DAF-BB78-9461B00C94A2";
            }
        }

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            //IMPORTANT: When editing this function remember to change the checksum.
            if (!type.HasFlag(DatabaseInitializeType.Playlist))
            {
                return;
            }
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                var set = database.Set<PlaylistColumn>(transaction);
                set.Add(new PlaylistColumn()
                {
                    Name = "Playing",
                    Type = PlaylistColumnType.Plugin,
                    Sequence = 0,
                    Plugin = this.Id,
                    Enabled = true
                });
                transaction.Commit();
            }
        }
    }
}
