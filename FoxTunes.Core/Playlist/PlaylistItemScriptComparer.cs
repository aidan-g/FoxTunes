using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public class PlaylistItemScriptComparer : BaseComponent, IComparer<PlaylistItem>, IDisposable
    {
        public PlaylistItemScriptComparer(string script)
        {
            this.Script = script;
        }

        public string Script { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public IScriptingContext ScriptingContext { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.ScriptingContext = this.ScriptingRuntime.CreateContext();
            base.InitializeComponent(core);
        }

        public int Compare(PlaylistItem playlistItem1, PlaylistItem playlistItem2)
        {
            var value1 = this.GetValue(playlistItem1);
            var value2 = this.GetValue(playlistItem2);
            return this.Compare(value1, value2);
        }

        protected virtual int Compare(string value1, string value2)
        {
            var numeric1 = default(float);
            var numeric2 = default(float);
            if (float.TryParse(value1, out numeric1) && float.TryParse(value2, out numeric2))
            {
                return numeric1.CompareTo(numeric2);
            }
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
        }

        protected virtual string GetValue(PlaylistItem playlistItem)
        {
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, this.Script);
            runner.Prepare();
            return Convert.ToString(runner.Run());
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
            if (this.ScriptingContext != null)
            {
                this.ScriptingContext.Dispose();
            }
        }

        ~PlaylistItemScriptComparer()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
