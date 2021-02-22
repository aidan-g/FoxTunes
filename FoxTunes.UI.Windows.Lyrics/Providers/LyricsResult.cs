namespace FoxTunes
{
    public class LyricsResult
    {
        private LyricsResult()
        {
            this.Success = false;
        }

        public LyricsResult(string lyrics) : this()
        {
            this.Lyrics = lyrics;
            this.Success = true;
        }

        public string Lyrics { get; private set; }

        public bool Success { get; private set; }

        public static readonly LyricsResult Fail = new LyricsResult();
    }
}
