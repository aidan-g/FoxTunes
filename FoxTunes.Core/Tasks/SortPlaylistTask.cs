using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class SortPlaylistTask : PlaylistTaskBase
    {
        public SortPlaylistTask(Playlist playlist, PlaylistColumn playlistColumn, bool descending) : base(playlist)
        {
            this.PlaylistColumn = playlistColumn;
            this.Descending = descending;
        }

        public PlaylistColumn PlaylistColumn { get; private set; }

        public bool Descending { get; private set; }

        protected override Task OnRun()
        {
            return this.SortItems(this.PlaylistColumn, this.Descending);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
        }
    }
}
