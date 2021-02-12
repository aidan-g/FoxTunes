using System;
using System.Globalization;

namespace FoxTunes
{
    public class PlaylistScriptBinding : ScriptBinding
    {
        public PlaylistScriptBinding()
        {

        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlistItem = value as PlaylistItem;
            if (playlistItem == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(this.Script))
            {
                return null;
            }
            return base.Convert(
                this.ExecuteScript(playlistItem, this.Script),
                targetType,
                parameter,
                culture
            );
        }

        private object ExecuteScript(PlaylistItem playlistItem, string script)
        {
            var runner = new PlaylistItemScriptRunner(this.ScriptingContext, playlistItem, script);
            runner.Prepare();
            return runner.Run();
        }
    }
}
