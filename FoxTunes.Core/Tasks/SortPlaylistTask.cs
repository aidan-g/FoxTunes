using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class SortPlaylistTask : PlaylistTaskBase
    {
        public SortPlaylistTask(Playlist playlist, PlaylistColumn playlistColumn) : base(playlist)
        {
            this.PlaylistColumn = playlistColumn;
        }

        public PlaylistColumn PlaylistColumn { get; private set; }

        protected override Task OnRun()
        {
            return this.SortItems(this.PlaylistColumn);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
        }
    }
}
