using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class Info : ViewModelBase
    {
        const string ARTIST = "No Artist";

        const string PERFORMER = "No Performer";

        const string ALBUM = "No Album";

        const string GENRE = "No Genre";

        const string YEAR = "No Year";

        const int SAMPLE_RATE = 44100;

        const int BITS_PER_SAMPLE = 16;

        const int CHANNELS = 2;

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public ILibraryBrowser LibraryBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        private bool _HasData { get; set; }

        public bool HasData
        {
            get
            {
                return this._HasData;
            }
            set
            {
                this._HasData = value;
                this.OnHasDataChanged();
            }
        }

        protected virtual void OnHasDataChanged()
        {
            if (this.HasDataChanged != null)
            {
                this.HasDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("HasData");
        }

        public event EventHandler HasDataChanged;

        private string _FileName { get; set; }

        public string FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                this._FileName = value;
                this.OnFileNameChanged();
            }
        }

        protected virtual void OnFileNameChanged()
        {
            if (this.FileNameChanged != null)
            {
                this.FileNameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FileName");
        }

        public event EventHandler FileNameChanged;

        private IFileData _FileData { get; set; }

        public IFileData FileData
        {
            get
            {
                return this._FileData;
            }
            set
            {
                this._FileData = value;
                this.OnFileDataChanged();
            }
        }

        protected virtual void OnFileDataChanged()
        {
            if (this.FileDataChanged != null)
            {
                this.FileDataChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("FileData");
        }

        public event EventHandler FileDataChanged;

        private string _Artist { get; set; }

        public string Artist
        {
            get
            {
                if (string.IsNullOrEmpty(this._Artist))
                {
                    return ARTIST;
                }
                return this._Artist;
            }
            set
            {
                this._Artist = value;
                this.OnArtistChanged();
            }
        }

        protected virtual void OnArtistChanged()
        {
            if (this.ArtistChanged != null)
            {
                this.ArtistChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Artist");
        }

        public event EventHandler ArtistChanged;

        private string _Performer { get; set; }

        public string Performer
        {
            get
            {
                if (string.IsNullOrEmpty(this._Performer))
                {
                    return PERFORMER;
                }
                return this._Performer;
            }
            set
            {
                this._Performer = value;
                this.OnPerformerChanged();
            }
        }

        protected virtual void OnPerformerChanged()
        {
            if (this.PerformerChanged != null)
            {
                this.PerformerChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Performer");
        }

        public event EventHandler PerformerChanged;

        private string _Album { get; set; }

        public string Album
        {
            get
            {
                if (string.IsNullOrEmpty(this._Album))
                {
                    return ALBUM;
                }
                return this._Album;
            }
            set
            {
                this._Album = value;
                this.OnAlbumChanged();
            }
        }

        protected virtual void OnAlbumChanged()
        {
            if (this.AlbumChanged != null)
            {
                this.AlbumChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Album");
        }

        public event EventHandler AlbumChanged;

        private string _Title { get; set; }

        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(this._Title))
                {
                    if (!string.IsNullOrEmpty(this._FileName))
                    {
                        return Path.GetFileNameWithoutExtension(this._FileName);
                    }
                }
                return this._Title;
            }
            set
            {
                this._Title = value;
                this.OnTitleChanged();
            }
        }

        protected virtual void OnTitleChanged()
        {
            if (this.TitleChanged != null)
            {
                this.TitleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Title");
        }

        public event EventHandler TitleChanged;

        private string _Genre { get; set; }

        public string Genre
        {
            get
            {
                if (string.IsNullOrEmpty(this._Genre))
                {
                    return GENRE;
                }
                return this._Genre;
            }
            set
            {
                this._Genre = value;
                this.OnGenreChanged();
            }
        }

        protected virtual void OnGenreChanged()
        {
            if (this.GenreChanged != null)
            {
                this.GenreChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Genre");
        }

        public event EventHandler GenreChanged;

        private string _Year { get; set; }

        public string Year
        {
            get
            {
                if (string.IsNullOrEmpty(this._Year))
                {
                    return YEAR;
                }
                return this._Year;
            }
            set
            {
                this._Year = value;
                this.OnYearChanged();
            }
        }

        protected virtual void OnYearChanged()
        {
            if (this.YearChanged != null)
            {
                this.YearChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Year");
        }

        public event EventHandler YearChanged;

        private string _Channels { get; set; }

        public string Channels
        {
            get
            {
                var channels = default(int);
                if (!string.IsNullOrEmpty(this._Channels) && int.TryParse(this._Channels, out channels))
                {
                    return MetaDataInfo.ChannelDescription(channels);
                }
                return MetaDataInfo.ChannelDescription(CHANNELS);
            }
            set
            {
                this._Channels = value;
                this.OnChannelsChanged();
            }
        }

        protected virtual void OnChannelsChanged()
        {
            if (this.ChannelsChanged != null)
            {
                this.ChannelsChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Channels");
        }

        public event EventHandler ChannelsChanged;

        private string _SampleRate { get; set; }

        public string SampleRate
        {
            get
            {
                var sampleRate = default(int);
                if (!string.IsNullOrEmpty(this._SampleRate) && int.TryParse(this._SampleRate, out sampleRate))
                {
                    return MetaDataInfo.SampleRateDescription(sampleRate);
                }
                return MetaDataInfo.SampleRateDescription(SAMPLE_RATE);
            }
            set
            {
                this._SampleRate = value;
                this.OnSampleRateChanged();
            }
        }

        protected virtual void OnSampleRateChanged()
        {
            if (this.SampleRateChanged != null)
            {
                this.SampleRateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SampleRate");
        }

        public event EventHandler SampleRateChanged;

        private string _BitsPerSample { get; set; }

        public string BitsPerSample
        {
            get
            {
                var bitsPerSample = default(int);
                if (!string.IsNullOrEmpty(this._BitsPerSample) && int.TryParse(this._BitsPerSample, out bitsPerSample))
                {
                    return MetaDataInfo.SampleDescription(bitsPerSample);
                }
                return MetaDataInfo.SampleDescription(BITS_PER_SAMPLE);
            }
            set
            {
                this._BitsPerSample = value;
                this.OnBitsPerSampleChanged();
            }
        }

        protected virtual void OnBitsPerSampleChanged()
        {
            if (this.BitsPerSampleChanged != null)
            {
                this.BitsPerSampleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("BitsPerSample");
        }

        public event EventHandler BitsPerSampleChanged;

        private string _Bitrate { get; set; }

        public string Bitrate
        {
            get
            {
                var bitrate = default(int);
                if (!string.IsNullOrEmpty(this._Bitrate) && int.TryParse(this._Bitrate, out bitrate))
                {
                    return MetaDataInfo.BitRateDescription(bitrate);
                }
                else
                {
                    return MetaDataInfo.BitRateDescription(CalculateBitrate(this._SampleRate, this._BitsPerSample, this._Channels));
                }
            }
            set
            {
                this._Bitrate = value;
                this.OnBitrateChanged();
            }
        }

        private static int CalculateBitrate(string sampleRate, string bitsPerSample, string channels)
        {
            var _sampleRate = default(int);
            var _bitsPerSample = default(int);
            var _channels = default(int);
            if (!int.TryParse(sampleRate, out _sampleRate))
            {
                _sampleRate = SAMPLE_RATE;
            }
            if (!int.TryParse(bitsPerSample, out _bitsPerSample))
            {
                _bitsPerSample = BITS_PER_SAMPLE;
            }
            if (!int.TryParse(channels, out _channels))
            {
                _channels = CHANNELS;
            }
            return (_sampleRate * _bitsPerSample * _channels) / 1000;
        }

        protected virtual void OnBitrateChanged()
        {
            if (this.BitrateChanged != null)
            {
                this.BitrateChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bitrate");
        }

        public event EventHandler BitrateChanged;

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = this.Core.Managers.Library;
            this.PlaylistManager = this.Core.Managers.Playlist;
            this.PlaybackManager = this.Core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.LibraryBrowser = this.Core.Components.LibraryBrowser;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Dispatch(this.Refresh);
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            //Critical: Don't block in this event handler, it causes a deadlock.
            this.Dispatch(this.Refresh);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    return this.Refresh(names);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual Task Refresh(IEnumerable<string> names)
        {
            if (names != null && names.Any())
            {
                if (!names.Intersect(new[]
                {
                    CommonMetaData.Artist,
                    CommonMetaData.Performer,
                    CommonMetaData.Album,
                    CommonMetaData.Title,
                    CommonMetaData.Genre,
                    CommonMetaData.Year,
                    CommonStatistics.Rating,
                    CommonImageTypes.FrontCover
                }).Any())
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.Refresh();
        }

        protected virtual Task Refresh()
        {
            var fileName = default(string);
            var fileData = default(IFileData);
            var metaData = default(IDictionary<string, string>);
            var outputStream = this.PlaybackManager.CurrentStream;
            if (outputStream != null)
            {
                fileName = outputStream.FileName;
                if (outputStream.PlaylistItem.LibraryItem_Id.HasValue)
                {
                    fileData = this.LibraryBrowser.Get(outputStream.PlaylistItem.LibraryItem_Id.Value);
                }
                else
                {
                    fileData = outputStream.PlaylistItem;
                }
                lock (fileData.MetaDatas)
                {
                    metaData = fileData.MetaDatas.ToDictionary(
                        metaDataItem => metaDataItem.Name,
                        metaDataItem => metaDataItem.Value,
                        StringComparer.OrdinalIgnoreCase
                    );
                }
            }
            return Windows.Invoke(() =>
            {
                if (metaData != null)
                {
                    this.FileName = fileName;
                    this.FileData = fileData;
                    this.Artist = metaData.GetValueOrDefault(CommonMetaData.Artist);
                    this.Performer = metaData.GetValueOrDefault(CommonMetaData.Performer);
                    this.Album = metaData.GetValueOrDefault(CommonMetaData.Album);
                    this.Title = metaData.GetValueOrDefault(CommonMetaData.Title);
                    this.Genre = metaData.GetValueOrDefault(CommonMetaData.Genre);
                    this.Year = metaData.GetValueOrDefault(CommonMetaData.Year);
                    this.Channels = metaData.GetValueOrDefault(CommonProperties.AudioChannels);
                    this.SampleRate = metaData.GetValueOrDefault(CommonProperties.AudioSampleRate);
                    this.BitsPerSample = metaData.GetValueOrDefault(CommonProperties.BitsPerSample);
                    this.Bitrate = metaData.GetValueOrDefault(CommonProperties.AudioBitrate);
                    this.HasData = true;
                }
                else
                {
                    this.HasData = false;
                    this.FileName = fileName;
                    this.FileData = fileData;
                    this.Artist = null;
                    this.Performer = null;
                    this.Album = null;
                    this.Title = null;
                    this.Genre = null;
                    this.Year = null;
                    this.Channels = null;
                    this.SampleRate = null;
                    this.BitsPerSample = null;
                    this.Bitrate = null;
                }
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Info();
        }

        protected override void OnDisposing()
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            base.OnDisposing();
        }
    }
}
