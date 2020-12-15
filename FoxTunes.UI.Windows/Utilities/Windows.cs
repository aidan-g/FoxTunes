using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FoxTunes
{
    public static class Windows
    {
        static Windows()
        {
            Reset();
        }

        public static Dispatcher Dispatcher
        {
            get
            {
                if (IsMainWindowCreated)
                {
                    return MainWindow.Dispatcher;
                }
                if (IsMiniWindowCreated)
                {
                    return MiniWindow.Dispatcher;
                }
                if (Application.Current != null)
                {
                    return Application.Current.Dispatcher;
                }
                return null;
            }
        }

        private static Lazy<Window> _MainWindow { get; set; }

        public static bool IsMainWindowCreated
        {
            get
            {
                return _MainWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window MainWindow
        {
            get
            {
                var raiseEvent = !IsMainWindowCreated;
                try
                {
                    return _MainWindow.Value;
                }
                finally
                {
                    if (IsMainWindowCreated && raiseEvent)
                    {
                        OnMainWindowCreated();
                    }
                }
            }
        }

        private static void OnMainWindowCreated()
        {
            MainWindow.Closed += OnMainWindowClosed;
            if (MainWindowCreated == null)
            {
                return;
            }
            MainWindowCreated(MainWindow, EventArgs.Empty);
        }

        public static event EventHandler MainWindowCreated;

        private static void OnMainWindowClosed(object sender, EventArgs e)
        {
            if (IsMainWindowCreated)
            {
                UIDisposer.Dispose(MainWindow);
            }
            _MainWindow = new Lazy<Window>(() => new MainWindow());
            if (MainWindowClosed != null)
            {
                MainWindowClosed(typeof(MainWindow), EventArgs.Empty);
            }
            CheckShutdown();
        }

        public static event EventHandler MainWindowClosed;

        private static Lazy<Window> _MiniWindow { get; set; }

        public static bool IsMiniWindowCreated
        {
            get
            {
                return _MiniWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window MiniWindow
        {
            get
            {
                var raiseEvent = !IsMiniWindowCreated;
                try
                {
                    return _MiniWindow.Value;
                }
                finally
                {
                    if (IsMiniWindowCreated && raiseEvent)
                    {
                        OnMiniWindowCreated();
                    }
                }
            }
        }

        private static void OnMiniWindowCreated()
        {
            MiniWindow.Closed += OnMiniWindowClosed;
            if (MiniWindowCreated == null)
            {
                return;
            }
            MiniWindowCreated(MiniWindow, EventArgs.Empty);
        }

        public static event EventHandler MiniWindowCreated;

        private static void OnMiniWindowClosed(object sender, EventArgs e)
        {
            if (IsMiniWindowCreated)
            {
                UIDisposer.Dispose(MiniWindow);
            }
            _MiniWindow = new Lazy<Window>(() => new MiniWindow());
            if (MiniWindowClosed != null)
            {
                MiniWindowClosed(typeof(MiniWindow), EventArgs.Empty);
            }
            CheckShutdown();
        }

        public static event EventHandler MiniWindowClosed;

        private static Lazy<Window> _SettingsWindow { get; set; }

        public static bool IsSettingsWindowCreated
        {
            get
            {
                return _SettingsWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window SettingsWindow
        {
            get
            {
                var raiseEvent = !IsSettingsWindowCreated;
                try
                {
                    return _SettingsWindow.Value;
                }
                finally
                {
                    if (IsSettingsWindowCreated && raiseEvent)
                    {
                        OnSettingsWindowCreated();
                    }
                }
            }
        }

        private static void OnSettingsWindowCreated()
        {
            SettingsWindow.Closed += OnSettingsWindowClosed;
            if (SettingsWindowCreated == null)
            {
                return;
            }
            SettingsWindowCreated(SettingsWindow, EventArgs.Empty);
        }

        public static event EventHandler SettingsWindowCreated;

        private static void OnSettingsWindowClosed(object sender, EventArgs e)
        {
            if (IsSettingsWindowCreated)
            {
                UIDisposer.Dispose(SettingsWindow);
            }
            _SettingsWindow = new Lazy<Window>(() => new SettingsWindow() { Owner = ActiveWindow });
            if (SettingsWindowClosed == null)
            {
                return;
            }
            SettingsWindowClosed(typeof(SettingsWindow), EventArgs.Empty);
        }

        public static event EventHandler SettingsWindowClosed;

        private static Lazy<Window> _EqualizerWindow { get; set; }

        public static bool IsEqualizerWindowCreated
        {
            get
            {
                return _EqualizerWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window EqualizerWindow
        {
            get
            {
                var raiseEvent = !IsEqualizerWindowCreated;
                try
                {
                    return _EqualizerWindow.Value;
                }
                finally
                {
                    if (IsEqualizerWindowCreated && raiseEvent)
                    {
                        OnEqualizerWindowCreated();
                    }
                }
            }
        }

        private static void OnEqualizerWindowCreated()
        {
            EqualizerWindow.Closed += OnEqualizerWindowClosed;
            if (EqualizerWindowCreated == null)
            {
                return;
            }
            EqualizerWindowCreated(EqualizerWindow, EventArgs.Empty);
        }

        public static event EventHandler EqualizerWindowCreated;

        private static void OnEqualizerWindowClosed(object sender, EventArgs e)
        {
            if (IsEqualizerWindowCreated)
            {
                UIDisposer.Dispose(EqualizerWindow);
            }
            _EqualizerWindow = new Lazy<Window>(() => new EqualizerWindow() { Owner = ActiveWindow });
            if (EqualizerWindowClosed == null)
            {
                return;
            }
            EqualizerWindowClosed(typeof(EqualizerWindow), EventArgs.Empty);
        }

        public static event EventHandler EqualizerWindowClosed;

        private static Lazy<Window> _TempoWindow { get; set; }

        public static bool IsTempoWindowCreated
        {
            get
            {
                return _TempoWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window TempoWindow
        {
            get
            {
                var raiseEvent = !IsTempoWindowCreated;
                try
                {
                    return _TempoWindow.Value;
                }
                finally
                {
                    if (IsTempoWindowCreated && raiseEvent)
                    {
                        OnTempoWindowCreated();
                    }
                }
            }
        }

        private static void OnTempoWindowCreated()
        {
            TempoWindow.Closed += OnTempoWindowClosed;
            if (TempoWindowCreated == null)
            {
                return;
            }
            TempoWindowCreated(TempoWindow, EventArgs.Empty);
        }

        public static event EventHandler TempoWindowCreated;

        private static void OnTempoWindowClosed(object sender, EventArgs e)
        {
            if (IsTempoWindowCreated)
            {
                UIDisposer.Dispose(TempoWindow);
            }
            _TempoWindow = new Lazy<Window>(() => new TempoWindow() { Owner = ActiveWindow });
            if (TempoWindowClosed == null)
            {
                return;
            }
            TempoWindowClosed(typeof(TempoWindow), EventArgs.Empty);
        }

        public static event EventHandler TempoWindowClosed;

        private static Lazy<Window> _PlaylistManagerWindow { get; set; }

        public static bool IsPlaylistManagerWindowCreated
        {
            get
            {
                return _PlaylistManagerWindow.IsValueCreated;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Window PlaylistManagerWindow
        {
            get
            {
                var raiseEvent = !IsPlaylistManagerWindowCreated;
                try
                {
                    return _PlaylistManagerWindow.Value;
                }
                finally
                {
                    if (IsPlaylistManagerWindowCreated && raiseEvent)
                    {
                        OnPlaylistManagerWindowCreated();
                    }
                }
            }
        }

        private static void OnPlaylistManagerWindowCreated()
        {
            PlaylistManagerWindow.Closed += OnPlaylistManagerWindowClosed;
            if (PlaylistManagerWindowCreated == null)
            {
                return;
            }
            PlaylistManagerWindowCreated(PlaylistManagerWindow, EventArgs.Empty);
        }

        public static event EventHandler PlaylistManagerWindowCreated;

        private static void OnPlaylistManagerWindowClosed(object sender, EventArgs e)
        {
            if (IsPlaylistManagerWindowCreated)
            {
                UIDisposer.Dispose(PlaylistManagerWindow);
            }
            _PlaylistManagerWindow = new Lazy<Window>(() => new PlaylistManagerWindow() { Owner = ActiveWindow });
            if (PlaylistManagerWindowClosed == null)
            {
                return;
            }
            PlaylistManagerWindowClosed(typeof(PlaylistManagerWindow), EventArgs.Empty);
        }

        public static event EventHandler PlaylistManagerWindowClosed;

        private static Window _ActiveWindow { get; set; }

        public static Window ActiveWindow
        {
            get
            {
                if (_ActiveWindow != null)
                {
                    return _ActiveWindow;
                }
                if (IsMiniWindowCreated && MiniWindow.IsVisible)
                {
                    return MiniWindow;
                }
                if (IsMainWindowCreated)
                {
                    return MainWindow;
                }
                return null;
            }
            set
            {
                OnActiveWindowChanging();
                _ActiveWindow = value;
                OnActiveWindowChanged();
            }
        }

        private static void OnActiveWindowChanging()
        {
            if (ActiveWindowChanging == null)
            {
                return;
            }
            ActiveWindowChanging(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveWindowChanging;

        private static void OnActiveWindowChanged()
        {
            if (ActiveWindowChanged == null)
            {
                return;
            }
            ActiveWindowChanged(null, EventArgs.Empty);
        }

        public static event EventHandler ActiveWindowChanged;

        private static void CheckShutdown()
        {
            if ((IsMainWindowCreated && MainWindow.IsVisible) || (IsMiniWindowCreated && MiniWindow.IsVisible))
            {
                return;
            }
            var task = Shutdown();
        }

        public static Task Shutdown()
        {
            return Invoke(() =>
            {
                ShuttingDown(typeof(Windows), EventArgs.Empty);
                if (IsMiniWindowCreated)
                {
                    MiniWindow.Close();
                }
                if (IsMainWindowCreated)
                {
                    MainWindow.Close();
                }
                if (IsSettingsWindowCreated)
                {
                    SettingsWindow.Close();
                }
                if (IsEqualizerWindowCreated)
                {
                    EqualizerWindow.Close();
                }
                if (IsTempoWindowCreated)
                {
                    TempoWindow.Close();
                }
                if (IsPlaylistManagerWindowCreated)
                {
                    PlaylistManagerWindow.Close();
                }
                UIBehaviour.Shutdown();
                Reset();
            });
        }

        public static event EventHandler ShuttingDown;

        private static void Reset()
        {
            _MainWindow = new Lazy<Window>(() => new MainWindow());
            _MiniWindow = new Lazy<Window>(() => new MiniWindow());
            _SettingsWindow = new Lazy<Window>(() => new SettingsWindow() { Owner = ActiveWindow });
            _EqualizerWindow = new Lazy<Window>(() => new EqualizerWindow() { Owner = ActiveWindow });
            _TempoWindow = new Lazy<Window>(() => new TempoWindow() { Owner = ActiveWindow });
            _PlaylistManagerWindow = new Lazy<Window>(() => new PlaylistManagerWindow() { Owner = ActiveWindow });
        }

        public static Task Invoke(Action action)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.CheckAccess())
                {
                    action();
                }
                else
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
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public static Task Invoke(Func<Task> func)
        {
            var dispatcher = Dispatcher;
            if (dispatcher != null)
            {
                if (dispatcher.CheckAccess())
                {
                    return func();
                }
                else
                {
#if NET40
                    var source = new TaskCompletionSource<bool>();
                    dispatcher.BeginInvoke(new Action(async () =>
                    {
                        try
                        {
                            await func().ConfigureAwait(false);
                        }
                        finally
                        {
                            source.SetResult(false);
                        }
                    }));
                    return source.Task;
#else
                    return dispatcher.BeginInvoke(func).Task;
#endif
                }
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
