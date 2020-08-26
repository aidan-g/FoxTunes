using FoxDb;
using FoxTunes.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class HierarchyManager : StandardManager, IHierarchyManager
    {
        public HierarchyManagerState State
        {
            get
            {
                if (global::FoxTunes.BackgroundTask.Active.Any(backgroundTask =>
                {
                    var type = backgroundTask.GetType();
                    return type == typeof(BuildLibraryHierarchiesTask) || type == typeof(ClearLibraryHierarchiesTask);
                }))
                {
                    return HierarchyManagerState.Updating;
                }
                return HierarchyManagerState.None;
            }
        }

        public ICore Core { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IDatabaseFactory DatabaseFactory { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.LibraryManager = core.Managers.Library;
            this.DatabaseFactory = core.Factories.Database;
            base.InitializeComponent(core);
        }

        public async Task Build(LibraryItemStatus? status)
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new BuildLibraryHierarchiesTask(status))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        public async Task Clear(LibraryItemStatus? status, bool signal)
        {
            if (this.LibraryManager.State.HasFlag(LibraryManagerState.Updating))
            {
                return;
            }
            using (var task = new ClearLibraryHierarchiesTask(status, signal))
            {
                task.InitializeComponent(this.Core);
                await this.OnBackgroundTask(task).ConfigureAwait(false);
                await task.Run().ConfigureAwait(false);
            }
        }

        protected virtual Task OnBackgroundTask(IBackgroundTask backgroundTask)
        {
            if (this.BackgroundTask == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            var e = new BackgroundTaskEventArgs(backgroundTask);
            this.BackgroundTask(this, e);
            return e.Complete();
        }

        public event BackgroundTaskEventHandler BackgroundTask;

        public string Checksum
        {
            get
            {
                return "C1399216-D828-4EB8-9249-70DEA89EADFA";
            }
        }

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            //IMPORTANT: When editing this function remember to change the checksum.
            if (!type.HasFlag(DatabaseInitializeType.Library))
            {
                return;
            }
            var scriptingRuntime = ComponentRegistry.Instance.GetComponent<IScriptingRuntime>();
            if (scriptingRuntime == null)
            {
                return;
            }
            using (var transaction = database.BeginTransaction(database.PreferredIsolationLevel))
            {
                var set = database.Set<LibraryHierarchy>(transaction);
                foreach (var libraryHierarchy in set)
                {
                    //TODO: Bad .Wait()
                    LibraryTaskBase.RemoveHierarchies(database, libraryHierarchy, null, transaction).Wait();
                }
                set.Clear();
                set.Add(new LibraryHierarchy()
                {
                    Name = "Artist/Album/Title",
                    Type = LibraryHierarchyType.Script,
                    Sequence = 0,
                    Enabled = true,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Sequence = 0, Script = scriptingRuntime.CoreScripts.Artist },
                        new LibraryHierarchyLevel() { Sequence = 1, Script = scriptingRuntime.CoreScripts.Year_Album },
                        new LibraryHierarchyLevel() { Sequence = 2, Script = scriptingRuntime.CoreScripts.Disk_Track_Title }
                    }
                });
                set.Add(new LibraryHierarchy()
                {
                    Name = "Genre/Album/Title",
                    Type = LibraryHierarchyType.Script,
                    Sequence = 1,
                    Enabled = false,
                    Levels = new ObservableCollection<LibraryHierarchyLevel>()
                    {
                        new LibraryHierarchyLevel() { Sequence = 0, Script = scriptingRuntime.CoreScripts.Genre },
                        new LibraryHierarchyLevel() { Sequence = 1, Script = scriptingRuntime.CoreScripts.Year_Album },
                        new LibraryHierarchyLevel() { Sequence = 2, Script = scriptingRuntime.CoreScripts.Disk_Track_Title }
                    }
                });
                set.Add(new LibraryHierarchy()
                {
                    Name = "Folder Structure",
                    Type = LibraryHierarchyType.FileSystem,
                    Sequence = 2,
                    Enabled = false
                });
                transaction.Commit();
            }
        }
    }
}
