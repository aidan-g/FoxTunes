using FoxTunes.Interfaces;
using System;
using FoxDb;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistColumnsBehaviour : StandardBehaviour, IInvocableComponent
    {
        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.DatabaseFactory = core.Factories.Database;
            this.SignalEmitter = core.Components.SignalEmitter;
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                foreach (var playlistColumn in this.PlaylistBrowser.GetColumns())
                {
                    yield return new InvocationComponent(
                        InvocationComponent.CATEGORY_PLAYLIST_HEADER,
                        Convert.ToString(playlistColumn.Id),
                        playlistColumn.Name,
                        path: "Columns",
                        attributes: playlistColumn.Enabled ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
            }
        }

        public async Task InvokeAsync(IInvocationComponent component)
        {
            var id = default(int);
            if (!int.TryParse(component.Id, out id))
            {
                return;
            }
            var column = this.PlaylistBrowser.GetColumns().FirstOrDefault(
                _column => _column.Id == id
            );
            if (column == null)
            {
                return;
            }
            column.Enabled = !column.Enabled;
            using (var database = this.DatabaseFactory.Create())
            {
                using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                {
                    var set = database.Set<PlaylistColumn>(transaction);
                    await set.AddOrUpdateAsync(column).ConfigureAwait(false);
                    transaction.Commit();
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated)).ConfigureAwait(false);
        }
    }
}
