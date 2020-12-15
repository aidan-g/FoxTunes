using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BassDeviceMonitorBehaviour : StandardBehaviour, IDisposable
    {
        public static readonly TimeSpan TIMEOUT = TimeSpan.FromMilliseconds(100);

        private BassDeviceMonitorBehaviour()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
        }

        protected BassDeviceMonitorBehaviour(string id) : this()
        {
            this.Id = id;
        }

        public Debouncer Debouncer { get; private set; }

        public string Id { get; private set; }

        public IBassOutput Output { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement EnabledElement { get; private set; }

        public SelectionConfigurationElement OutputElement { get; private set; }

        public NotificationClient NotificationClient { get; private set; }

        new public bool IsInitialized { get; private set; }

        public bool IsEnabled
        {
            get
            {
                return this.EnabledElement.Value && string.Equals(this.OutputElement.Value.Id, this.Id, StringComparison.OrdinalIgnoreCase);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.Output = core.Components.Output as IBassOutput;
            this.Output.Init += this.OnInit;
            this.Output.Free += this.OnFree;
            this.PlaylistManager = core.Managers.Playlist;
            this.PlaybackManager = core.Managers.Playback;
            this.Configuration = core.Components.Configuration;
            this.EnabledElement = this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.DEVICE_MONITOR_ELEMENT
            );
            this.OutputElement = this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.OUTPUT_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected virtual void OnInit(object sender, EventArgs e)
        {
            if (!this.IsEnabled || this.IsInitialized)
            {
                return;
            }
            this.NotificationClient = new NotificationClient();
            this.NotificationClient.DeviceAdded += this.OnDeviceAdded;
            this.NotificationClient.DeviceRemoved += this.OnDeviceRemoved;
            this.NotificationClient.DeviceStateChanged += this.OnDeviceStateChanged;
            this.NotificationClient.DefaultDeviceChanged += this.OnDefaultDeviceChanged;
            this.NotificationClient.PropertyValueChanged += this.OnPropertyValueChanged;
            this.IsInitialized = true;
            Logger.Write(this, LogLevel.Debug, "BASS Device Monitor Initialized.");
        }

        protected virtual void OnFree(object sender, EventArgs e)
        {
            if (!this.IsInitialized)
            {
                return;
            }
            if (this.NotificationClient != null)
            {
                this.NotificationClient.DeviceAdded -= this.OnDeviceAdded;
                this.NotificationClient.DeviceRemoved -= this.OnDeviceRemoved;
                this.NotificationClient.DeviceStateChanged -= this.OnDeviceStateChanged;
                this.NotificationClient.DefaultDeviceChanged -= this.OnDefaultDeviceChanged;
                this.NotificationClient.PropertyValueChanged -= this.OnPropertyValueChanged;
                this.NotificationClient.Dispose();
                this.NotificationClient = null;
            }
            this.IsInitialized = false;
        }

        protected virtual void OnDeviceAdded(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDeviceRemoved(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDeviceStateChanged(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual void OnDefaultDeviceChanged(object sender, NotificationClientEventArgs e)
        {
            this.Debouncer.Exec(() =>
            {
                if (!this.RestartRequired(e.Flow, e.Role))
                {
                    return;
                }
                Logger.Write(this, LogLevel.Debug, "The default playback device was changed: {0} => {1} => {2}", e.Flow.Value, e.Role.Value, e.Device);
                Logger.Write(this, LogLevel.Debug, "Restarting the output.");
                var task = this.Restart();
            });
        }

        protected virtual void OnPropertyValueChanged(object sender, NotificationClientEventArgs e)
        {
            //Nothing to do.
        }

        protected virtual bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!this.Output.IsStarted)
            {
                return false;
            }
            if (flow == null || flow.Value != DataFlow.Render)
            {
                return false;
            }
            if (role == null || role.Value != Role.Multimedia)
            {
                return false;
            }
            return true;
        }

        public async Task Restart()
        {
            if (!this.Output.IsStarted)
            {
                return;
            }
            var position = default(long);
            var paused = default(bool);
            var playlistItem = default(PlaylistItem);
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                position = outputStream.Position;
                paused = outputStream.IsPaused;
                playlistItem = outputStream.PlaylistItem;
            }
            try
            {
                await this.Output.Shutdown().ConfigureAwait(false);
                await this.Output.Start().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.OnError(e).ConfigureAwait(false);
                return;
            }
            if (playlistItem != null)
            {
                try
                {
                    await this.PlaylistManager.Play(playlistItem).ConfigureAwait(false);
                    if (this.PlaybackManager.CurrentStream != null)
                    {
                        if (position > 0)
                        {
                            this.PlaybackManager.CurrentStream.Position = position;
                        }
                        if (paused)
                        {
                            await this.PlaybackManager.CurrentStream.Pause().ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    await this.OnError(e).ConfigureAwait(false);
                }
            }
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
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
            }
            if (this.Output != null)
            {
                this.Output.Init -= this.OnInit;
                this.Output.Free -= this.OnFree;
            }
            this.OnFree(this, EventArgs.Empty);
        }

        ~BassDeviceMonitorBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }
}