using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MetaDataSynchronizer : StandardComponent, IMetaDataSynchronizer, IDisposable
    {
        const int UPDATE_INTERVAL = 60000;

        public static readonly object SyncRoot = new object();

        public IMetaDataManager MetaDataManager { get; private set; }

        public global::System.Timers.Timer Timer { get; private set; }

        public BooleanConfigurationElement MetaDataEnabled { get; private set; }

        public BooleanConfigurationElement BackgroundEnabled { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MetaDataManager = core.Managers.MetaData;
            this.MetaDataEnabled = core.Components.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.ENABLE_ELEMENT
            );
            this.BackgroundEnabled = core.Components.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.BACKGROUND_WRITE_ELEMENT
            );
            this.Write = core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            this.MetaDataEnabled.ValueChanged += this.OnValueChanged;
            this.BackgroundEnabled.ValueChanged += this.OnValueChanged;
            this.Write.ValueChanged += this.OnValueChanged;
            this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            if (this.MetaDataEnabled.Value && this.BackgroundEnabled.Value && MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value) != WriteBehaviour.None)
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
        }

        private void Enable()
        {
            lock (SyncRoot)
            {
                if (this.Timer == null)
                {
                    this.Timer = new global::System.Timers.Timer();
                    this.Timer.Interval = UPDATE_INTERVAL;
                    this.Timer.AutoReset = false;
                    this.Timer.Elapsed += this.OnElapsed;
                    this.Timer.Start();
                    Logger.Write(this, LogLevel.Debug, "Background meta data synchronization enabled.");
                }
            }
        }

        private void Disable()
        {
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                    Logger.Write(this, LogLevel.Debug, "Background meta data synchronization disabled.");
                }
            }
        }

        protected virtual async void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await this.MetaDataManager.Synchronize().ConfigureAwait(false);
                lock (SyncRoot)
                {
                    if (this.Timer == null)
                    {
                        return;
                    }
                    this.Timer.Start();
                }
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        public Task Synchronize(params LibraryItem[] libraryItem)
        {
            return this.MetaDataManager.Synchronize(libraryItem);
        }

        public Task Synchronize(params PlaylistItem[] playlistItem)
        {
            return this.MetaDataManager.Synchronize(playlistItem);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Disable();
        }

        ~MetaDataSynchronizer()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}
