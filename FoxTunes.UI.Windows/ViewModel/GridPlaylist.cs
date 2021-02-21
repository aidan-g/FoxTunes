#define FILE_ACTION_MANAGER_DROP_ACTION
using FoxDb;
using FoxTunes.Integration;
using FoxTunes.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class GridPlaylist : PlaylistBase
    {
        public Playlist CurrentPlaylist { get; private set; }

        public PlaylistSortingBehaviour PlaylistSortingBehaviour { get; private set; }

        public PlaylistGridViewColumnFactory GridViewColumnFactory { get; private set; }

        public IFileActionHandlerManager FileActionHandlerManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private bool _GroupingEnabled { get; set; }

        public bool GroupingEnabled
        {
            get
            {
                return this._GroupingEnabled;
            }
            set
            {
                this._GroupingEnabled = value;
                this.OnGroupingEnabledChanged();
            }
        }

        protected virtual void OnGroupingEnabledChanged()
        {
            if (this.GroupingEnabledChanged != null)
            {
                this.GroupingEnabledChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GroupingEnabled");
        }

        public event EventHandler GroupingEnabledChanged;

        private string _GroupingScript { get; set; }

        public string GroupingScript
        {
            get
            {
                return this._GroupingScript;
            }
            set
            {
                this._GroupingScript = value;
                this.OnGroupingScriptChanged();
            }
        }

        protected virtual void OnGroupingScriptChanged()
        {
            if (this.GroupingScriptChanged != null)
            {
                this.GroupingScriptChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GroupingScript");
        }

        public event EventHandler GroupingScriptChanged;

        public IList SelectedItems
        {
            get
            {
                if (this.PlaylistManager == null)
                {
                    return null;
                }
                return this.PlaylistManager.SelectedItems;
            }
            set
            {
                if (this.PlaylistManager == null)
                {
                    return;
                }
                this.PlaylistManager.SelectedItems = value.OfType<PlaylistItem>().ToArray();
            }
        }

        protected virtual void OnSelectedItemsChanged()
        {
            if (this.SelectedItemsChanged != null)
            {
                this.SelectedItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItems");
        }

        public event EventHandler SelectedItemsChanged;

        private bool _InsertActive { get; set; }

        public bool InsertActive
        {
            get
            {
                return this._InsertActive;
            }
            set
            {
                this._InsertActive = value;
                this.OnInsertActiveChanged();
            }
        }

        protected virtual void OnInsertActiveChanged()
        {
            if (this.InsertActiveChanged != null)
            {
                this.InsertActiveChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertActive");
        }

        public event EventHandler InsertActiveChanged;

        private PlaylistItem _InsertItem { get; set; }

        public PlaylistItem InsertItem
        {
            get
            {
                return this._InsertItem;
            }
            set
            {
                this._InsertItem = value;
                this.OnInsertItemChanged();
            }
        }

        protected virtual void OnInsertItemChanged()
        {
            if (this.InsertItemChanged != null)
            {
                this.InsertItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("InsertItem");
        }

        public event EventHandler InsertItemChanged;

        private ObservableCollection<PlaylistGridViewColumn> _GridColumns { get; set; }

        public ObservableCollection<PlaylistGridViewColumn> GridColumns
        {
            get
            {
                return this._GridColumns;
            }
            set
            {
                this._GridColumns = value;
                this.OnGridColumnsChanged();
            }
        }

        protected virtual void OnGridColumnsChanged()
        {
            if (this.GridColumnsChanged != null)
            {
                this.GridColumnsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("GridColumns");
        }

        public event EventHandler GridColumnsChanged;

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.PlaylistManager.SelectedItemsChanged += this.OnSelectedItemsChanged;
            this.PlaylistSortingBehaviour = ComponentRegistry.Instance.GetComponent<PlaylistSortingBehaviour>();
            this.GridViewColumnFactory = ComponentRegistry.Instance.GetComponent<PlaylistGridViewColumnFactory>();
            this.FileActionHandlerManager = core.Managers.FileActionHandler;
            this.Configuration = core.Components.Configuration;
#if NET40
            //ListView grouping is too slow under net40 due to lack of virtualization.
#else
            this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistGroupingBehaviourConfiguration.GROUP_ENABLED_ELEMENT
            ).ConnectValue(value => this.GroupingEnabled = value);
            this.Configuration.GetElement<TextConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistGroupingBehaviourConfiguration.GROUP_SCRIPT_ELEMENT
            ).ConnectValue(value => this.GroupingScript = value);
#endif
        }

        protected virtual void OnSelectedItemsChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(this.OnSelectedItemsChanged);
        }

        protected override async Task OnSignal(object sender, ISignal signal)
        {
            await base.OnSignal(sender, signal).ConfigureAwait(false);
            switch (signal.Name)
            {
                case CommonSignals.PlaylistUpdated:
                    var playlists = signal.State as IEnumerable<Playlist>;
                    if (playlists != null && playlists.Any())
                    {
                        var playlist = await this.GetPlaylist().ConfigureAwait(false);
                        if (playlist != null && playlists.Contains(playlist))
                        {
                            await this.ResizeColumns().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await this.ResizeColumns().ConfigureAwait(false);
                    }
                    break;
                case CommonSignals.PlaylistColumnsUpdated:
                    var columns = signal.State as IEnumerable<PlaylistColumn>;
                    if (columns != null && columns.Any())
                    {
                        await this.RefreshColumns().ConfigureAwait(false);
                    }
                    else
                    {
                        await this.ReloadColumns().ConfigureAwait(false);
                    }
                    break;
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    await this.RefreshColumns(names).ConfigureAwait(false);
                    break;
            }
        }

        public ICommand RemovePlaylistItemsCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    new Func<Task>(this.RemovePlaylistItems)
                );
            }
        }

        protected virtual async Task RemovePlaylistItems()
        {
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            if (playlist == null)
            {
                return;
            }
            await this.PlaylistManager.Remove(playlist, this.SelectedItems.OfType<PlaylistItem>()).ConfigureAwait(false);
        }

        protected virtual async Task CropPlaylistItems()
        {
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            if (playlist == null)
            {
                return;
            }
            await this.PlaylistManager.Crop(playlist, this.SelectedItems.OfType<PlaylistItem>()).ConfigureAwait(false);
        }

        protected virtual Task LocatePlaylistItems()
        {
            foreach (var item in this.SelectedItems.OfType<PlaylistItem>())
            {
                Explorer.Select(item.FileName);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ICommand PlaySelectedItemCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    () =>
                    {
                        var playlistItem = this.SelectedItems[0] as PlaylistItem;
                        if (playlistItem == null)
                        {
#if NET40
                            return TaskEx.FromResult(false);
#else
                            return Task.CompletedTask;
#endif
                        }
                        return this.PlaylistManager.Play(playlistItem);
                    },
                    () => this.PlaylistManager != null && this.SelectedItems.Count > 0
                );
            }
        }

        public ICommand DragEnterCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragEnter);
            }
        }

        protected virtual void OnDragEnter(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        public ICommand DragOverCommand
        {
            get
            {
                return new Command<DragEventArgs>(this.OnDragOver);
            }
        }

        protected virtual void OnDragOver(DragEventArgs e)
        {
            this.UpdateDragDropEffects(e);
        }

        protected virtual void UpdateDragDropEffects(DragEventArgs e)
        {
            var effects = DragDropEffects.None;
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    effects = DragDropEffects.Copy;
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>())
                {
                    effects = DragDropEffects.Copy;
                }
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    effects = DragDropEffects.Copy;
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to query clipboard contents: {0}", exception.Message);
            }
            e.Effects = effects;
        }

        public ICommand DropCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<DragEventArgs>(
                    new Func<DragEventArgs, Task>(this.OnDrop)
                );
            }
        }

        protected virtual Task OnDrop(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>;
                    return this.FileActionHandlerManager.RunPaths(paths);
                }
                if (e.Data.GetDataPresent(typeof(LibraryHierarchyNode)))
                {
                    var libraryHierarchyNode = e.Data.GetData(typeof(LibraryHierarchyNode)) as LibraryHierarchyNode;
                    return this.AddToPlaylist(libraryHierarchyNode);
                }
                if (e.Data.GetDataPresent<IEnumerable<PlaylistItem>>())
                {
                    var playlistItems = e.Data
                        .GetData<IEnumerable<PlaylistItem>>()
                        .OrderBy(playlistItem => playlistItem.Sequence);
                    return this.AddToPlaylist(playlistItems);
                }
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    var paths = ShellIDListHelper.GetData(e.Data);
                    return this.FileActionHandlerManager.RunPaths(paths);
                }
            }
            catch (Exception exception)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to process clipboard contents: {0}", exception.Message);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        private async Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode)
        {
            var sequence = default(int);
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            if (playlist == null)
            {
                return;
            }
            if (this.TryGetInsertSequence(out sequence))
            {
                await this.PlaylistManager.Insert(playlist, sequence, libraryHierarchyNode, false).ConfigureAwait(false);
            }
            else
            {
                await this.PlaylistManager.Add(playlist, libraryHierarchyNode, false).ConfigureAwait(false);
            }
        }

        private async Task AddToPlaylist(IEnumerable<PlaylistItem> playlistItems)
        {
            var sequence = default(int);
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            if (playlist == null)
            {
                return;
            }
            if (this.TryGetInsertSequence(out sequence))
            {
                //TODO: This was really confusing, there is some disconnection between what the UI suggests will happen (where the items will be moved to) and what happens.
                //TODO: I don't really understand it but if the current sequence is less than the target sequence (moving down the list) then we have to decrement the target sequence.
                //TODO: The unit tests don't have this problem for some reason.
                var playlistItem = playlistItems.FirstOrDefault();
                if (playlistItem != null && playlistItem.Sequence < sequence)
                {
                    sequence--;
                }
                await this.PlaylistManager.Move(playlist, sequence, playlistItems).ConfigureAwait(false);
            }
            else
            {
                await this.PlaylistManager.Move(playlist, playlistItems).ConfigureAwait(false);
            }
        }

        protected virtual bool TryGetInsertSequence(out int sequence)
        {
            if (!this.InsertActive || this.InsertItem == null)
            {
                sequence = 0;
                return false;
            }
            sequence = this.InsertItem.Sequence;
            return true;
        }

        protected virtual IEnumerable<PlaylistGridViewColumn> GetGridColumns()
        {
            if (this.PlaylistBrowser != null)
            {
                return this.PlaylistBrowser.GetColumns()
                    .Where(column => column.Enabled)
                    .Select(column => this.GridViewColumnFactory.Create(column))
                    .ToArray();
            }
            return new PlaylistGridViewColumn[] { };
        }

        public virtual async Task Refresh()
        {
            this.CurrentPlaylist = await this.GetPlaylist().ConfigureAwait(false);
            await this.RefreshItems().ConfigureAwait(false);
            await this.RefreshSelectedItems().ConfigureAwait(false);
            await this.RefreshColumns(null).ConfigureAwait(false);
            await this.ResizeColumns().ConfigureAwait(false);
        }

        public virtual Task RefreshSelectedItems()
        {
            return Windows.Invoke(new Action(this.OnSelectedItemsChanged));
        }

        public virtual async Task RefreshColumns(IEnumerable<string> names)
        {
            if (this.GridColumns == null || this.GridColumns.Count == 0)
            {
                await this.ReloadColumns().ConfigureAwait(false);
            }
            if (this.GridColumns != null)
            {
                foreach (var column in this.GridColumns)
                {
                    if (!this.GridViewColumnFactory.ShouldRefreshColumn(column, names))
                    {
                        continue;
                    }
                    await this.RefreshColumn(column).ConfigureAwait(false);
                }
            }
        }

        protected virtual Task RefreshColumn(PlaylistGridViewColumn column)
        {
            return Windows.Invoke(() => this.GridViewColumnFactory.Refresh(column));
        }

        protected virtual async Task ResizeColumns()
        {
            if (this.GridColumns == null || this.GridColumns.Count == 0)
            {
                await this.ReloadColumns().ConfigureAwait(false);
            }
            if (this.GridColumns != null)
            {
                foreach (var column in this.GridColumns)
                {
                    await this.ResizeColumn(column).ConfigureAwait(false);
                }
            }
        }

        protected virtual Task ResizeColumn(GridViewColumn column)
        {
            return Windows.Invoke(() => this.GridViewColumnFactory.Resize(column));
        }

        protected virtual Task RefreshColumns()
        {
            if (this.GridColumns != null)
            {
                var count = this.PlaylistBrowser.GetColumns()
                    .Where(column => column.Enabled)
                    .Count();
                if (this.GridColumns.Count == count)
                {
                    //Nothing to do, we only handle add/remove columns.
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.ReloadColumns();
        }

        protected virtual Task ReloadColumns()
        {
            return Windows.Invoke(
                () => this.GridColumns = new ObservableCollection<PlaylistGridViewColumn>(this.GetGridColumns())
            );
        }

        public async Task Sort(PlaylistColumn playlistColumn)
        {
            var playlist = await this.GetPlaylist().ConfigureAwait(false);
            await this.PlaylistSortingBehaviour.Sort(playlist, playlistColumn).ConfigureAwait(false);
        }
    }
}
