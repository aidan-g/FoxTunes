using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class FileActionHandlerManager : StandardManager, IFileActionHandlerManager
    {
        public FileActionHandlerManager()
        {
            this.OpenMode = CommandLineParser.OpenMode.None;
            this.Queue = new PendingQueue<string>(TimeSpan.FromSeconds(1));
            this.Queue.Complete += this.OnComplete;
        }

        public CommandLineParser.OpenMode OpenMode { get; private set; }

        public PendingQueue<string> Queue { get; private set; }

        public IPlaylistBrowser PlaylistBrowser { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IEnumerable<IFileActionHandler> FileActionHandlers { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistBrowser = core.Components.PlaylistBrowser;
            this.PlaylistManager = core.Managers.Playlist;
            this.FileActionHandlers = ComponentRegistry.Instance.GetComponents<IFileActionHandler>();
            base.InitializeComponent(core);
        }

        protected virtual void OnComplete(object sender, PendingQueueEventArgs<string> e)
        {
#if NET40            
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                try
                {
                    var playlist = default(Playlist);
                    var index = default(int);
                    if (this.OpenMode == CommandLineParser.OpenMode.Play)
                    {
                        playlist = this.PlaylistManager.SelectedPlaylist;
                        index = this.PlaylistBrowser.GetInsertIndex(playlist);
                    }
                    await this.RunPaths(e.Sequence).ConfigureAwait(false);
                    if (this.OpenMode == CommandLineParser.OpenMode.Play)
                    {
                        await this.PlaylistManager.Play(playlist, index).ConfigureAwait(false);
                    }
                }
                finally
                {
                    this.OpenMode = CommandLineParser.OpenMode.Default;
                }
            });
        }

        public virtual void RunCommand(string command)
        {
            var mode = default(CommandLineParser.OpenMode);
            var paths = default(IEnumerable<string>);
            if (!CommandLineParser.TryParse(command, out paths, out mode))
            {
                return;
            }
            this.OpenMode = mode;
            this.Queue.EnqueueRange(paths);
        }

        public virtual async Task RunPaths(IEnumerable<string> paths)
        {
            var handlers = new Dictionary<IFileActionHandler, IList<string>>();
            foreach (var path in paths)
            {
                foreach (var handler in this.FileActionHandlers)
                {
                    if (!handler.CanHandle(path))
                    {
                        continue;
                    }
                    handlers.GetOrAdd(handler, key => new List<string>()).Add(path);
                }
            }
            foreach (var pair in handlers)
            {
                await pair.Key.Handle(pair.Value).ConfigureAwait(false);
            }
        }

        protected override void OnDisposing()
        {
            if (this.Queue != null)
            {
                this.Queue.Complete -= this.OnComplete;
                this.Queue.Dispose();
            }
            base.OnDisposing();
        }
    }
}
