using FoxDb.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class AddLibraryHierarchyNodesToPlaylistTask : PlaylistTaskBase
    {
        public AddLibraryHierarchyNodesToPlaylistTask(Playlist playlist, int sequence, IEnumerable<LibraryHierarchyNode> libraryHierarchyNodes, string filter, bool clear, bool visible = true)
            : base(playlist, sequence)
        {
            this.LibraryHierarchyNodes = libraryHierarchyNodes;
            this.Filter = filter;
            this.Clear = clear;
            this._Visible = visible;
        }

        public override bool Visible
        {
            get
            {
                return this._Visible && this.LibraryHierarchyNodes.Count() > 1;
            }
        }

        public override bool Cancellable
        {
            get
            {
                return this._Visible && this.LibraryHierarchyNodes.Count() > 1;
            }
        }

        public IEnumerable<LibraryHierarchyNode> LibraryHierarchyNodes { get; private set; }

        public string Filter { get; private set; }

        public bool Clear { get; private set; }

        private bool _Visible { get; set; }

        protected override async Task OnRun()
        {
            if (this.Clear)
            {
                await this.RemoveItems(PlaylistItemStatus.None).ConfigureAwait(false);
            }
            await this.AddPlaylistItems().ConfigureAwait(false);
            if (!this.Clear)
            {
                await this.ShiftItems(QueryOperator.GreaterOrEqual, this.Sequence, this.Offset).ConfigureAwait(false);
            }
            await this.SetPlaylistItemsStatus(PlaylistItemStatus.None).ConfigureAwait(false);
        }

        protected override async Task OnCompleted()
        {
            await base.OnCompleted().ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated, new[] { this.Playlist })).ConfigureAwait(false);
        }

        private async Task AddPlaylistItems()
        {
            this.Name = "Creating playlist";
            this.Count = this.LibraryHierarchyNodes.Count();
            using (var task = new SingletonReentrantTask(this, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, async cancellationToken =>
            {
                using (var transaction = this.Database.BeginTransaction(this.Database.PreferredIsolationLevel))
                {
                    var position = 0;
                    foreach (var libraryHierarchyNode in this.LibraryHierarchyNodes)
                    {
                        if (this.IsCancellationRequested)
                        {
                            break;
                        }
                        this.Description = libraryHierarchyNode.Value;
                        await this.AddPlaylistItems(this.Database.Queries.AddLibraryHierarchyNodeToPlaylist(this.Filter, this.Sort.Value), libraryHierarchyNode, transaction).ConfigureAwait(false);
                        this.Position = ++position;
                    }
                    if (transaction.HasTransaction)
                    {
                        transaction.Commit();
                    }
                }
            }))
            {
                await task.Run().ConfigureAwait(false);
            }
        }

        private async Task AddPlaylistItems(IDatabaseQuery query, LibraryHierarchyNode libraryHierarchyNode, ITransactionSource transaction)
        {
            var count = await this.Database.ExecuteScalarAsync<int>(query, (parameters, phase) =>
            {
                switch (phase)
                {
                    case DatabaseParameterPhase.Fetch:
                        parameters["playlistId"] = this.Playlist.Id;
                        parameters["libraryHierarchyId"] = libraryHierarchyNode.LibraryHierarchyId;
                        parameters["libraryHierarchyItemId"] = libraryHierarchyNode.Id;
                        parameters["sequence"] = this.Sequence;
                        parameters["status"] = PlaylistItemStatus.Import;
                        break;
                }
            }, transaction).ConfigureAwait(false);
            this.Sequence += count;
            this.Offset += count;
        }
    }
}
