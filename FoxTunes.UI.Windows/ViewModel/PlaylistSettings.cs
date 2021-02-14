using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class PlaylistSettings : ViewModelBase
    {
        public PlaylistColumnProviderManager PlaylistColumnProviderManager { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private CollectionManager<PlaylistColumn> _PlaylistColumns { get; set; }

        public CollectionManager<PlaylistColumn> PlaylistColumns
        {
            get
            {
                return this._PlaylistColumns;
            }
            set
            {
                this._PlaylistColumns = value;
                this.OnPlaylistColumnsChanged();
            }
        }

        protected virtual void OnPlaylistColumnsChanged()
        {
            this.OnPropertyChanged("PlaylistColumns");
        }

        private ObservableCollection<IUIPlaylistColumnProvider> _PlaylistColumnProviders { get; set; }

        public ObservableCollection<IUIPlaylistColumnProvider> PlaylistColumnProviders
        {
            get
            {
                return this._PlaylistColumnProviders;
            }
            set
            {
                this._PlaylistColumnProviders = value;
                this.OnPlaylistColumnProvidersChanged();
            }
        }

        protected virtual void OnPlaylistColumnProvidersChanged()
        {
            this.OnPropertyChanged("PlaylistColumnProviders");
        }

        public bool IsSaving
        {
            get
            {
                return global::FoxTunes.BackgroundTask.Active
                    .OfType<PlaylistTaskBase>()
                    .Any();
            }
        }

        protected virtual void OnIsSavingChanged()
        {
            if (this.IsSavingChanged != null)
            {
                this.IsSavingChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSaving");
        }

        public event EventHandler IsSavingChanged;

        public ICommand SaveCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Save);
            }
        }

        public async Task Save()
        {
            var exception = default(Exception);
            try
            {
                using (var database = this.DatabaseFactory.Create())
                {
                    using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, cancellationToken =>
                    {
                        using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
                        {
                            var playlistColumns = database.Set<PlaylistColumn>(transaction);
                            playlistColumns.Remove(playlistColumns.Except(this.PlaylistColumns.ItemsSource));
                            playlistColumns.AddOrUpdate(this.PlaylistColumns.ItemsSource);
                            transaction.Commit();
                        }
#if NET40
                        return TaskEx.FromResult(false);
#else
                        return Task.CompletedTask;
#endif
                    }))
                    {
                        await task.Run().ConfigureAwait(false);
                    }
                }
                await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated)).ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                exception = e;
            }
            await this.OnError("Save", exception).ConfigureAwait(false);
            throw exception;
        }

        public ICommand CancelCommand
        {
            get
            {
                return new Command(this.Cancel)
                {
                    Tag = CommandHints.DISMISS
                };
            }
        }

        public void Cancel()
        {
            this.Dispatch(this.Refresh);
        }

        public ICommand ResetCommand
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(this.Reset);
            }
        }

        public async Task Reset()
        {
            var core = default(ICore);
            await Windows.Invoke(() => core = this.Core).ConfigureAwait(false);
            using (var database = this.DatabaseFactory.Create())
            {
                using (var task = new SingletonReentrantTask(CancellationToken.None, ComponentSlots.Database, SingletonReentrantTask.PRIORITY_HIGH, cancellationToken =>
                {
                    core.InitializeDatabase(database, DatabaseInitializeType.Playlist);
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }))
                {
                    await task.Run().ConfigureAwait(false);
                }
            }
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistUpdated)).ConfigureAwait(false);
            await this.SignalEmitter.Send(new Signal(this, CommonSignals.PlaylistColumnsUpdated)).ConfigureAwait(false);
        }

        public override void InitializeComponent(ICore core)
        {
            global::FoxTunes.BackgroundTask.ActiveChanged += this.OnActiveChanged;
            this.PlaylistColumnProviderManager = ComponentRegistry.Instance.GetComponent<PlaylistColumnProviderManager>();
            this.PlaylistBrowser = this.Core.Components.PlaylistBrowser;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.DatabaseFactory = this.Core.Factories.Database;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.PlaylistColumns = new CollectionManager<PlaylistColumn>()
            {
                ItemFactory = () =>
                {
                    using (var database = this.DatabaseFactory.Create())
                    {
                        return database.Set<PlaylistColumn>().Create().With(playlistColumn =>
                        {
                            playlistColumn.Name = "New";
                            playlistColumn.Type = PlaylistColumnType.Tag;
                            playlistColumn.Script = "'New'";
                            playlistColumn.Enabled = true;
                        });
                    }
                },
                ExchangeHandler = (item1, item2) =>
                {
                    var temp = item1.Sequence;
                    item1.Sequence = item2.Sequence;
                    item2.Sequence = temp;
                }
            };
            this.PlaylistColumnProviders = new ObservableCollection<IUIPlaylistColumnProvider>(
                this.PlaylistColumnProviderManager.Providers
            );
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual async void OnActiveChanged(object sender, EventArgs e)
        {
            await Windows.Invoke(() => this.OnIsSavingChanged()).ConfigureAwait(false);
        }

        private Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.SettingsUpdated:
                    return this.Refresh();
                case CommonSignals.PlaylistColumnsUpdated:
                    var columns = signal.State as IEnumerable<PlaylistColumn>;
                    if (columns != null && columns.Any())
                    {
                        this.PlaylistColumns.Refresh();
                    }
                    else
                    {
                        return this.Refresh();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                this.PlaylistColumns.ItemsSource = new ObservableCollection<PlaylistColumn>(
                    this.PlaylistBrowser.GetColumns()
                );
            });
        }

        protected override void OnDisposing()
        {
            global::FoxTunes.BackgroundTask.ActiveChanged -= this.OnActiveChanged;
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PlaylistSettings();
        }
    }
}
