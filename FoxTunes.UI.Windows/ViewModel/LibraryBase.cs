using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public abstract class LibraryBase : ViewModelBase
    {
        protected virtual string LOADING
        {
            get
            {
                return "Loading...";
            }
        }

        protected virtual string UPDATING
        {
            get
            {
                return "Updating...";
            }
        }

        protected virtual string EMPTY
        {
            get
            {
                return Strings.LibraryBase_Add;
            }
        }

        public abstract string UIComponent { get; }

        public bool IsNavigating { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        private IConfiguration Configuration { get; set; }

        private LibraryHierarchyNodeCollection _Items { get; set; }

        public virtual LibraryHierarchyNodeCollection Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.OnItemsChanged();
            }
        }

        protected virtual void OnItemsChanged()
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Items");
        }

        public event EventHandler ItemsChanged;

        public virtual LibraryHierarchyNode SelectedItem
        {
            get
            {
                if (this.LibraryManager == null)
                {
                    return LibraryHierarchyNode.Empty;
                }
                return this.LibraryManager.SelectedItem;
            }
            set
            {
                if (this.LibraryManager == null || value == null)
                {
                    return;
                }
                this.IsNavigating = true;
                try
                {
                    this.LibraryManager.SelectedItem = value;
                }
                finally
                {
                    this.IsNavigating = false;
                }
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (this.SelectedItemChanged != null)
            {
                this.SelectedItemChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedItem");
        }

        public event EventHandler SelectedItemChanged;

        private bool _ShowCursorAdorners { get; set; }

        public virtual bool ShowCursorAdorners
        {
            get
            {
                return this._ShowCursorAdorners;
            }
            set
            {
                this._ShowCursorAdorners = value;
                this.OnShowCursorAdornersChanged();
            }
        }

        protected virtual void OnShowCursorAdornersChanged()
        {
            if (this.ShowCursorAdornersChanged != null)
            {
                this.ShowCursorAdornersChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ShowCursorAdorners");
        }

        public event EventHandler ShowCursorAdornersChanged;

        public string StatusMessage
        {
            get
            {
                if (this.LibraryHierarchyBrowser == null || this.Items == null)
                {
                    return LOADING;
                }
                if (this.Items.Count > 0)
                {
                    return null;
                }
                switch (this.LibraryHierarchyBrowser.State)
                {
                    case LibraryHierarchyBrowserState.Loading:
                        return LOADING;
                }
                var isUpdating = global::FoxTunes.BackgroundTask.Active
                    .OfType<LibraryTaskBase>()
                    .Any();
                if (isUpdating)
                {
                    return UPDATING;
                }
                else
                {
                    return EMPTY;
                }
            }
        }

        protected virtual void OnStatusMessageChanged()
        {
            if (this.StatusMessageChanged != null)
            {
                this.StatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("StatusMessage");
        }

        public event EventHandler StatusMessageChanged;

        public bool HasStatusMessage
        {
            get
            {
                if (this.Items == null || this.Items.Count == 0)
                {
                    return true;
                }
                return false;
            }
        }

        protected virtual void OnHasStatusMessageChanged()
        {
            if (this.HasStatusMessageChanged != null)
            {
                this.HasStatusMessageChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasStatusMessage");
        }

        public event EventHandler HasStatusMessageChanged;

        protected virtual Task RefreshStatus()
        {
            return Windows.Invoke(() =>
            {
                this.OnStatusMessageChanged();
                this.OnHasStatusMessageChanged();
            });
        }

        public virtual async Task Refresh()
        {
            await this.RefreshItems().ConfigureAwait(false);
            await Windows.Invoke(() =>
            {
                this.OnSelectedItemChanged();
            }).ConfigureAwait(false);
        }

        protected virtual async Task RefreshItems()
        {
            var libraryHierarchy = this.LibraryManager.SelectedHierarchy;
            if (libraryHierarchy == null || LibraryHierarchy.Empty.Equals(libraryHierarchy))
            {
                return;
            }
            var items = this.LibraryHierarchyBrowser.GetNodes(libraryHierarchy);
            if (this.Items == null)
            {
                await Windows.Invoke(() => this.Items = new LibraryHierarchyNodeCollection(items)).ConfigureAwait(false);
            }
            else
            {
                await Windows.Invoke(this.Items.Reset(items)).ConfigureAwait(false);
            }
            await this.RefreshStatus().ConfigureAwait(false);
        }

        public virtual Task Reload()
        {
            return this.Refresh();
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryHierarchyBrowser.FilterChanged += this.OnFilterChanged;
            this.LibraryHierarchyBrowser.StateChanged += this.OnStateChanged;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.LibraryManager = this.Core.Managers.Library;
            this.LibraryManager.SelectedHierarchyChanged += this.OnSelectedHierarchyChanged;
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
            this.Configuration = this.Core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.SHOW_CURSOR_ADORNERS
            ).ConnectValue(value => this.ShowCursorAdorners = value);
            //TODO: Bad .Wait().
            this.Refresh().Wait();
            this.RefreshStatus().Wait();
            base.InitializeComponent(core);
        }

        protected virtual void OnActiveChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual void OnFilterChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnStateChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.RefreshStatus);
        }

        protected virtual void OnSelectedHierarchyChanged(object sender, EventArgs e)
        {
            this.Dispatch(this.Refresh);
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
            var task = Windows.Invoke(new Action(this.OnSelectedItemChanged));
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ICommand AddToPlaylistCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand<bool>(
                    force => this.AddToPlaylist(this.SelectedItem, false),
                    force => this.PlaylistManager.SelectedPlaylist != null && this.SelectedItem != null && (force || this.SelectedItem.IsLeaf)
                );
            }
        }

        private Task AddToPlaylist(LibraryHierarchyNode libraryHierarchyNode, bool clear)
        {
            return this.PlaylistManager.Add(
                this.PlaylistManager.SelectedPlaylist,
                libraryHierarchyNode,
                clear
            );
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
                    return this.LibraryManager.Add(paths);
                }
                if (ShellIDListHelper.GetDataPresent(e.Data))
                {
                    var paths = ShellIDListHelper.GetData(e.Data);
                    return this.LibraryManager.Add(paths);
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

        protected override void Dispose(bool disposing)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.LibraryHierarchyBrowser != null)
            {
                this.LibraryHierarchyBrowser.FilterChanged -= this.OnFilterChanged;
            }
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            if (this.LibraryManager != null)
            {
                this.LibraryManager.SelectedHierarchyChanged -= this.OnSelectedHierarchyChanged;
                this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
            }
            base.Dispose(disposing);
        }
    }
}
