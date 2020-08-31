using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;
using System.Windows.Threading;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class TaskbarButtonsBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 1000;

        private static readonly int WM_TASKBARCREATED;

        private static readonly object SyncRoot = new object();

        static TaskbarButtonsBehaviour()
        {
            try
            {
                WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
            }
            catch
            {
                Logger.Write(typeof(TaskbarButtonsBehaviour), LogLevel.Warn, "Failed to register window message: TaskbarCreated");
            }
        }

        const int BUTTON_PREVIOUS = 0;

        const int BUTTON_PLAY_PAUSE = 1;

        const int BUTTON_NEXT = 2;

        public TaskbarButtonsBehaviour()
        {
            this.Callback = new HwndSourceHook(this.OnCallback);
            this.Windows = new Dictionary<IntPtr, TaskbarButtonsWindowFlags>();
            this.ImageLists = new Dictionary<IntPtr, IntPtr>();
        }

        public Timer Timer { get; private set; }

        public HwndSourceHook Callback { get; private set; }

        public IDictionary<IntPtr, TaskbarButtonsWindowFlags> Windows { get; private set; }

        public IDictionary<IntPtr, IntPtr> ImageLists { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            if (TaskbarButtonsBehaviourConfiguration.IsPlatformSupported)
            {
                this.PlaylistManager = core.Managers.Playlist;
                this.PlaybackManager = core.Managers.Playback;
                this.UserInterface = core.Components.UserInterface;
                this.UserInterface.WindowCreated += this.OnWindowCreated;
                this.UserInterface.WindowDestroyed += this.OnWindowDestroyed;
                this.Configuration = core.Components.Configuration;
                this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                    TaskbarButtonsBehaviourConfiguration.SECTION,
                    TaskbarButtonsBehaviourConfiguration.ENABLED_ELEMENT
                );
                this.Enabled.ConnectValue(value =>
                {
                    if (value)
                    {
                        this.Enable();
                    }
                    else
                    {
                        this.Disable();
                    }
                    this.Update();
                });
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Platform is not supported.");
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnWindowCreated(object sender, UserInterfaceWindowEventArgs e)
        {
            if (e.Window.Role != UserInterfaceWindowRole.Main)
            {
                //Only create taskbar buttons for main windows.
                return;
            }
            lock (SyncRoot)
            {
                if (this.Windows.ContainsKey(e.Window.Handle))
                {
                    //Uh.. Why was a window with the same handle "created" twice?
                    return;
                }
                this.Windows.Add(e.Window.Handle, TaskbarButtonsWindowFlags.None);
                this.Update();
            }
        }

        protected virtual void OnWindowDestroyed(object sender, UserInterfaceWindowEventArgs e)
        {
            //Nothing to do.
        }

        public void Enable()
        {
            lock (SyncRoot)
            {
                if (this.Timer == null)
                {
                    this.Timer = new Timer();
                    this.Timer.Interval = UPDATE_INTERVAL;
                    this.Timer.Elapsed += this.OnElapsed;
                    this.Timer.AutoReset = false;
                    this.Timer.Start();
                    Logger.Write(this, LogLevel.Debug, "Updater enabled.");
                }
            }
        }

        public void Disable()
        {
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                    Logger.Write(this, LogLevel.Debug, "Updater disabled.");
                }
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.Update();
                lock (SyncRoot)
                {
                    if (this.Timer != null)
                    {
                        this.Timer.Start();
                    }
                }
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        protected virtual void Update()
        {
            lock (SyncRoot)
            {
                foreach (var handle in this.Windows.Keys.ToArray())
                {
                    var flags = this.Windows[handle];
                    var task = this.Update(handle, flags);
                }
            }
        }

        protected virtual async Task Update(IntPtr handle, TaskbarButtonsWindowFlags flags)
        {
            if (flags.HasFlag(TaskbarButtonsWindowFlags.Error))
            {
                return;
            }
            if (this.Enabled.Value)
            {
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.Registered))
                {
                    this.AddHook(handle);
                }
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.ImagesCreated))
                {
                    if (!await this.CreateImages(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                if (!flags.HasFlag(TaskbarButtonsWindowFlags.ButtonsCreated))
                {
                    if (!await this.CreateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
                else
                {
                    if (!await this.UpdateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }
            else
            {
                if (flags.HasFlag(TaskbarButtonsWindowFlags.ButtonsCreated))
                {
                    if (!await this.UpdateButtons(handle).ConfigureAwait(false))
                    {
                        return;
                    }
                }
            }
        }

        protected virtual IntPtr OnCallback(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WindowsTaskbarList.WM_COMMAND)
            {
                var low = (wParam.ToInt64() & 0x0000FFFF);
                var high = (wParam.ToInt64() & 0xFFFF0000) >> 16;
                if (high == WindowsTaskbarList.THBN_CLICKED)
                {
                    this.OnButtonPressed(Convert.ToInt32(low));
                }
            }
            else if (msg == WM_TASKBARCREATED)
            {
                this.OnTaskBarCreated(hwnd);
            }
            return IntPtr.Zero;
        }

        protected virtual void OnTaskBarCreated(IntPtr handle)
        {
            lock (SyncRoot)
            {
                if (!this.Windows.ContainsKey(handle))
                {
                    //Handing an event for an unknown window?
                    return;
                }
                //TODO: Should we be destroying the image list?
                this.Windows[handle] = TaskbarButtonsWindowFlags.Registered;
                var task = this.Update(handle, TaskbarButtonsWindowFlags.Registered);
            }
        }

        protected virtual void AddHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Adding Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return;
            }
            source.AddHook(this.Callback);
            this.Windows[handle] |= TaskbarButtonsWindowFlags.Registered;
        }

        protected virtual void RemoveHook(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Removing Windows event handler.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return;
            }
            source.RemoveHook(this.Callback);
            this.Windows[handle] &= ~TaskbarButtonsWindowFlags.Registered;
        }

        protected virtual async Task<bool> CreateImages(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Creating taskbar button image list.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var width = WindowsImageList.GetSystemMetrics(
                WindowsImageList.SystemMetric.SM_CXSMICON
            );
            var height = WindowsImageList.GetSystemMetrics(
                WindowsImageList.SystemMetric.SM_CYSMICON
            );
            Logger.Write(this, LogLevel.Debug, "Taskbar buttom image dimentions: {0}x{1}", width, height);
            var imageList = WindowsImageList.ImageList_Create(
                width,
                height,
                WindowsImageList.ILC_COLOR32,
                4,
                0
            );
            if (IntPtr.Zero.Equals(imageList))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create button image list.");
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            else
            {
                this.ImageLists[handle] = imageList;
            }
            for (var a = 0; a < 4; a++)
            {
                using (var bitmap = this.GetImage(a, width, height))
                {
                    if (!this.AddImage(handle, imageList, bitmap, width, height))
                    {
                        return false;
                    }
                }
            }
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarSetImageList(handle, imageList)
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create button image list: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar button image list created.");
                this.Windows[handle] |= TaskbarButtonsWindowFlags.ImagesCreated;
                return true;
            }
        }

        protected virtual bool AddImage(IntPtr handle, IntPtr imageList, Bitmap bitmap, int width, int height)
        {
            var bitmapBits = default(IntPtr);
            var bitmapInfo = new WindowsImageList.BITMAPINFO()
            {
                biSize = 40,
                biBitCount = 32,
                biPlanes = 1,
                biWidth = width,
                biHeight = height
            };
            var bitmapSection = WindowsImageList.CreateDIBSection(
                IntPtr.Zero,
                bitmapInfo,
                0,
                out bitmapBits,
                IntPtr.Zero,
                0
            );
            if (IntPtr.Zero.Equals(bitmapSection))
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create DIB.");
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            var bitmapData = bitmap.LockBits(
                new Rectangle(
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height
                ),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            {
                var result = WindowsImageList.RtlMoveMemory(
                    bitmapBits,
                    bitmapData.Scan0,
                    bitmap.Height * bitmapData.Stride
                );
                if (!result)
                {
                    Logger.Write(this, LogLevel.Warn, "Call to RtlMoveMemory reports failure.");
                    this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                    return false;
                }
            }
            bitmap.UnlockBits(bitmapData);
            {
                var result = WindowsImageList.ImageList_Add(
                    imageList,
                    bitmapSection,
                    IntPtr.Zero
                );
                if (result < 0)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to add image to ImageList.");
                    this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                    return false;
                }
            }
            return true;
        }

        protected virtual bool DestroyImages(IntPtr handle)
        {
            var imageList = default(IntPtr);
            if (!this.ImageLists.TryGetValue(handle, out imageList))
            {
                //There was no image list to destroy.
                return true;
            }
            var result = WindowsImageList.ImageList_Destroy(imageList);
            if (!result)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to destroy image list: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar button image list destroyed.");
                this.Windows[handle] &= ~TaskbarButtonsWindowFlags.ImagesCreated;
                return true;
            }
        }

        protected virtual Bitmap GetImage(int index, int width, int height)
        {
            var name = string.Format("FoxTunes.Core.Windows.Images.{0}.png", this.GetImageName(index));
            Logger.Write(this, LogLevel.Debug, "Creating image: {0}", name);
            using (var stream = typeof(TaskbarButtonsBehaviour).Assembly.GetManifestResourceStream(name))
            {
                var image = (Bitmap)Bitmap.FromStream(stream);
                if (image.Width != width || image.Height != height)
                {
                    try
                    {
                        return this.GetImage(image, width, height);
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }
                return image;
            }
        }

        protected virtual Bitmap GetImage(Bitmap sourceImage, int width, int height)
        {
            var resultImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resultImage))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawImage(
                    sourceImage,
                    new Rectangle(
                        0,
                        0,
                        width,
                        height
                    )
                );
            }
            return resultImage;
        }

        protected virtual string GetImageName(int index)
        {
            switch (index)
            {
                case 0:
                    return "prev";
                case 1:
                    return "play";
                case 2:
                    return "pause";
                case 3:
                    return "next";
            }
            throw new NotImplementedException();
        }

        protected virtual async Task<bool> CreateButtons(IntPtr handle)
        {
            Logger.Write(this, LogLevel.Debug, "Creating taskbar buttons.");
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var buttons = this.Buttons.ToArray();
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarAddButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                )
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to create taskbar buttons: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Taskbar buttons created.");
                this.Windows[handle] |= TaskbarButtonsWindowFlags.ButtonsCreated;
                return true;
            }
        }

        protected virtual void UpdateButtons()
        {
            lock (SyncRoot)
            {
                foreach (var handle in this.Windows.Keys.ToArray())
                {
                    var task = this.UpdateButtons(handle);
                }
            }
        }

        protected virtual async Task<bool> UpdateButtons(IntPtr handle)
        {
            var source = HwndSource.FromHwnd(handle);
            if (source == null)
            {
                Logger.Write(this, LogLevel.Warn, "No such window for handle: {0}", handle);
                return false;
            }
            var buttons = this.Buttons.ToArray();
            var result = default(WindowsTaskbarList.HResult);
            await this.Invoke(
                source.Dispatcher,
                () => result = WindowsTaskbarList.Instance.ThumbBarUpdateButtons(
                    handle,
                    Convert.ToUInt32(buttons.Length),
                    buttons
                )
            ).ConfigureAwait(false);
            if (result != WindowsTaskbarList.HResult.Ok)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to update taskbar buttons: {0}", Enum.GetName(typeof(WindowsTaskbarList.HResult), result));
                this.Windows[handle] |= TaskbarButtonsWindowFlags.Error;
                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual IEnumerable<WindowsTaskbarList.ThumbButton> Buttons
        {
            get
            {
                yield return this.PreviousButton;
                yield return this.PlayPauseButton;
                yield return this.NextButton;
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton PreviousButton
        {
            get
            {
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_PREVIOUS,
                    Tip = "Previous",
                    Bitmap = 0,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton PlayPauseButton
        {
            get
            {
                var outputStream = this.PlaybackManager.CurrentStream;
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_PLAY_PAUSE,
                    Tip = "Play or pause",
                    Bitmap = outputStream != null && outputStream.IsPlaying ? 2u : 1u,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButton NextButton
        {
            get
            {
                return new WindowsTaskbarList.ThumbButton()
                {
                    Id = BUTTON_NEXT,
                    Tip = "Next",
                    Bitmap = 3,
                    Mask = this.ButtonMask,
                    Flags = this.ButtonFlags
                };
            }
        }

        protected virtual WindowsTaskbarList.ThumbButtonMask ButtonMask
        {
            get
            {
                return WindowsTaskbarList.ThumbButtonMask.THB_FLAGS | WindowsTaskbarList.ThumbButtonMask.Bitmap;
            }
        }

        protected virtual WindowsTaskbarList.ThumbButtonOptions ButtonFlags
        {
            get
            {
                var flags = default(WindowsTaskbarList.ThumbButtonOptions);
                if (!this.Enabled.Value)
                {
                    flags |= WindowsTaskbarList.ThumbButtonOptions.Hidden;
                }
                return flags;
            }
        }

        protected virtual void OnButtonPressed(int button)
        {
            var task = default(Task);
            switch (button)
            {
                case BUTTON_PREVIOUS:
                    task = this.Previous();
                    break;
                case BUTTON_PLAY_PAUSE:
                    task = this.PlayPause();
                    break;
                case BUTTON_NEXT:
                    task = this.Next();
                    break;
            }
        }

        protected virtual Task Invoke(Dispatcher dispatcher, Action action)
        {
#if NET40
            var source = new TaskCompletionSource<bool>();
            dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    action();
                }
                finally
                {
                    source.SetResult(false);
                }
            }));
            return source.Task;
#else
            return dispatcher.BeginInvoke(action).Task;
#endif
        }

        protected virtual async Task Previous()
        {
            Logger.Write(this, LogLevel.Debug, "Previous button was clicked.");
            await this.PlaylistManager.Previous().ConfigureAwait(false);
            this.UpdateButtons();
        }

        protected virtual async Task PlayPause()
        {
            Logger.Write(this, LogLevel.Debug, "Play/pause button was clicked.");
            var currentStream = this.PlaybackManager.CurrentStream;
            if (currentStream == null)
            {
                await this.PlaylistManager.Next().ConfigureAwait(false);
            }
            else
            {
                if (currentStream.IsPaused)
                {
                    await currentStream.Resume().ConfigureAwait(false);
                }
                else if (currentStream.IsPlaying)
                {
                    await currentStream.Pause().ConfigureAwait(false);
                }
            }
            this.UpdateButtons();
        }

        protected virtual async Task Next()
        {
            Logger.Write(this, LogLevel.Debug, "Next button was clicked.");
            await this.PlaylistManager.Next().ConfigureAwait(false);
            this.UpdateButtons();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return TaskbarButtonsBehaviourConfiguration.GetConfigurationSections();
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
            if (this.UserInterface != null)
            {
                this.UserInterface.WindowCreated -= this.OnWindowCreated;
                this.UserInterface.WindowDestroyed -= this.OnWindowDestroyed;
            }
            this.Disable();
        }

        ~TaskbarButtonsBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string msg);
    }

    [Flags]
    public enum TaskbarButtonsWindowFlags : byte
    {
        None = 0,
        Registered = 1,
        ImagesCreated = 2,
        ButtonsCreated = 4,
        Error = 8
    }
}
