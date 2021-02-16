using FoxTunes;
using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace FoxTunes
{
    public class TagLibMetaDataSource : BaseComponent, IMetaDataSource
    {
        public static ArtworkType ArtworkTypes = ArtworkType.FrontCover;

        public static readonly KeyLock<string> KeyLock = new KeyLock<string>(StringComparer.OrdinalIgnoreCase);

        //10MB
        public static int MAX_TAG_SIZE = 10240000;

        public TagLibMetaDataSource()
        {
            this.Warnings = new ConcurrentDictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public ConcurrentDictionary<string, IList<string>> Warnings { get; private set; }

        public IEnumerable<string> GetWarnings(string fileName)
        {
            var warnings = default(IList<string>);
            if (!this.Warnings.TryGetValue(fileName, out warnings))
            {
                return Enumerable.Empty<string>();
            }
            return warnings;
        }

        public void AddWarning(string fileName, string warning)
        {
            this.Warnings.GetOrAdd(fileName, key => new List<string>()).Add(warning);
        }

        public void AddWarnings(string fileName, IEnumerable<string> warnings)
        {
            this.Warnings.GetOrAdd(fileName, key => new List<string>()).AddRange(warnings);
        }

        public TagLibFileFactory FileFactory { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement EmbeddedImages { get; private set; }

        public BooleanConfigurationElement LooseImages { get; private set; }

        public SelectionConfigurationElement ImagesPreference { get; private set; }

        public BooleanConfigurationElement CopyImages { get; private set; }

        public IntegerConfigurationElement MaxImageSize { get; private set; }

        public BooleanConfigurationElement Extended { get; private set; }

        public BooleanConfigurationElement MusicBrainz { get; private set; }

        public BooleanConfigurationElement Lyrics { get; private set; }

        public BooleanConfigurationElement ReplayGain { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

        public SelectionConfigurationElement Write { get; private set; }

        public IArtworkProvider ArtworkProvider { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.FileFactory = ComponentRegistry.Instance.GetComponent<TagLibFileFactory>();
            this.Configuration = core.Components.Configuration;
            this.EmbeddedImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_EMBEDDED_IMAGES
            );
            this.LooseImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LOOSE_IMAGES
            );
            this.ImagesPreference = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.IMAGES_PREFERENCE
            );
            this.CopyImages = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.COPY_IMAGES_ELEMENT
            );
            this.MaxImageSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.MAX_IMAGE_SIZE
            );
            this.Extended = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_EXTENDED_TAGS
            );
            this.MusicBrainz = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_MUSICBRAINZ_TAGS
            );
            this.Lyrics = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS
            );
            this.ReplayGain = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_REPLAY_GAIN_TAGS
            );
            this.Popularimeter = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS
            );
            this.Write = this.Configuration.GetElement<SelectionConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.WRITE_ELEMENT
            );
            this.ArtworkProvider = core.Components.ArtworkProvider;
            base.InitializeComponent(core);
        }

        public async Task<IEnumerable<MetaDataItem>> GetMetaData(string fileName)
        {
            if (!this.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
                this.AddWarning(fileName, "Unsupported file format.");
                return Enumerable.Empty<MetaDataItem>();
            }
            var collect = default(bool);
            var metaData = new List<MetaDataItem>();
            Logger.Write(this, LogLevel.Trace, "Reading meta data for file: {0}", fileName);
            try
            {
                using (var file = this.FileFactory.Create(fileName))
                {
                    if (file.PossiblyCorrupt)
                    {
                        this.AddWarnings(fileName, file.CorruptionReasons);
                    }
                    if (file.InvariantStartPosition > MAX_TAG_SIZE)
                    {
                        collect = true;
                    }
                    if (file.Tag != null)
                    {
                        this.AddTags(metaData, file.Tag);
                    }
                    if (file.Properties != null)
                    {
                        this.AddProperties(metaData, file.Properties);
                    }
                    if (this.Popularimeter.Value)
                    {
                        this.Try(() => PopularimeterManager.Read(this, metaData, file), this.ErrorHandler);
                    }
                    if (this.ReplayGain.Value)
                    {
                        this.Try(() => ReplayGainManager.Read(this, metaData, file), this.ErrorHandler);
                    }
                    if (file is IMetaDataSource metaDataSource)
                    {
                        await this.AddAdditional(metaData, file, metaDataSource).ConfigureAwait(false);
                    }
                    await ImageManager.Read(this, metaData, file).ConfigureAwait(false);
                }
            }
            catch (UnsupportedFormatException)
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
                this.AddWarning(fileName, "Unsupported file format.");
            }
            catch (OutOfMemoryException)
            {
                //This can happen with really big embedded images.
                //It's tricky to avoid because we can't check InvariantStartPosition without parsing.
                Logger.Write(this, LogLevel.Warn, "Out of memory: {0}", fileName);
                this.AddWarning(fileName, "Out of memory.");
                collect = true;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to read meta data: {0} => {1}", fileName, e.Message);
                this.AddWarning(fileName, string.Format("Failed to read meta data: {0}", e.Message));
            }
            finally
            {
                if (collect)
                {
                    //If we encountered a large meta data section (>10MB) then we need to try to reclaim the memory.
                    GC.Collect();
                }
            }
            return metaData;
        }

        public async Task SetMetaData(string fileName, IEnumerable<MetaDataItem> metaData, Func<MetaDataItem, bool> predicate)
        {
            if (MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value) == WriteBehaviour.None)
            {
                Logger.Write(this, LogLevel.Warn, "Writing is disabled: {0}", fileName);
                return;
            }
            if (!this.IsSupported(fileName))
            {
                Logger.Write(this, LogLevel.Warn, "Unsupported file format: {0}", fileName);
                return;
            }
            var metaDataItems = default(IEnumerable<MetaDataItem>);
            lock (metaData)
            {
                if (predicate != null)
                {
                    metaDataItems = metaData.Where(predicate).ToArray();
                }
                else
                {
                    metaDataItems = metaData.ToArray();
                }
                if (!metaDataItems.Any())
                {
                    //Nothing to update.
                    return;
                }
            }
            var collect = default(bool);
            using (var file = this.FileFactory.Create(fileName))
            {
                if (file.PossiblyCorrupt)
                {
                    this.AddWarnings(fileName, file.CorruptionReasons);
                }
                foreach (var metaDataItem in metaDataItems)
                {
                    switch (metaDataItem.Type)
                    {
                        case MetaDataItemType.Tag:
                            this.SetTag(metaDataItem, file, file.Tag);
                            break;
                        case MetaDataItemType.Image:
                            if (file.InvariantStartPosition > MAX_TAG_SIZE)
                            {
                                collect = true;
                            }
                            await this.SetImage(metaDataItem, file, file.Tag).ConfigureAwait(false);
                            break;
                    }
                }
                if (file is IMetaDataSource metaDataSource)
                {
                    await this.SetAdditional(metaData, file, metaDataSource).ConfigureAwait(false);
                }
                file.Save();
            }
            if (collect)
            {
                //If we encountered a large meta data section (>10MB) then we need to try to reclaim the memory.
                GC.Collect();
            }
        }

        protected virtual bool IsSupported(string fileName)
        {
            var mimeType = string.Format("taglib/{0}", fileName.GetExtension());
            return FileTypes.AvailableTypes.ContainsKey(mimeType);
        }

        private void AddTags(IList<MetaDataItem> metaData, Tag tag)
        {
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Album, tag.Album), this.ErrorHandler);
            (this).Try(() =>
            {
                if (!string.IsNullOrEmpty(tag.FirstAlbumArtist))
                {
                    this.AddTag(metaData, CommonMetaData.Artist, tag.FirstAlbumArtist);
                }
                else if (!string.IsNullOrEmpty(tag.FirstPerformer))
                {
                    this.AddTag(metaData, CommonMetaData.Artist, tag.FirstPerformer);
                }
            }, this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Composer, tag.FirstComposer), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Conductor, tag.Conductor), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Disc, tag.Disc.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.DiscCount, tag.DiscCount.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Genre, tag.FirstGenre), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Performer, tag.FirstPerformer), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Title, tag.Title), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Track, tag.Track.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.TrackCount, tag.TrackCount.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.Year, tag.Year.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.BeatsPerMinute, tag.BeatsPerMinute.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddTag(metaData, CommonMetaData.InitialKey, tag.InitialKey), this.ErrorHandler);

            if (this.Extended.Value)
            {
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicIpId, tag.MusicIpId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.AmazonId, tag.AmazonId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.Comment, tag.Comment), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.Copyright, tag.Copyright), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.Grouping, tag.Grouping), this.ErrorHandler);
            }

            if (this.MusicBrainz.Value)
            {
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzArtistId, tag.MusicBrainzArtistId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzDiscId, tag.MusicBrainzDiscId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseArtistId, tag.MusicBrainzReleaseArtistId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseCountry, tag.MusicBrainzReleaseCountry), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseId, tag.MusicBrainzReleaseId), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseStatus, tag.MusicBrainzReleaseStatus), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzReleaseType, tag.MusicBrainzReleaseType), this.ErrorHandler);
                (this).Try(() => this.AddTag(metaData, CommonMetaData.MusicBrainzTrackId, tag.MusicBrainzTrackId), this.ErrorHandler);
            }

            if (this.Lyrics.Value)
            {
                (this).Try(() => this.AddTag(metaData, CommonMetaData.Lyrics, tag.Lyrics), this.ErrorHandler);
            }
        }

        private bool HasValue(string value)
        {
            return !string.IsNullOrEmpty(value) && !string.Equals(value, 0.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private void AddTag(IList<MetaDataItem> metaData, string name, string value)
        {
            if (!this.HasValue(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Tag) { Value = value.Trim() });
        }

        private void AddProperties(IList<MetaDataItem> metaData, Properties properties)
        {
            (this).Try(() => this.AddProperty(metaData, CommonProperties.Duration, properties.Duration.TotalMilliseconds.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddProperty(metaData, CommonProperties.AudioBitrate, properties.AudioBitrate.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddProperty(metaData, CommonProperties.AudioChannels, properties.AudioChannels.ToString()), this.ErrorHandler);
            (this).Try(() => this.AddProperty(metaData, CommonProperties.AudioSampleRate, properties.AudioSampleRate.ToString()), this.ErrorHandler);
            (this).Try(() =>
            {
                if (properties.BitsPerSample != 0)
                {
                    this.AddProperty(metaData, CommonProperties.BitsPerSample, properties.BitsPerSample.ToString());
                }
                else
                {
                    //This is special case just for MPEG-4.
                    foreach (var codec in properties.Codecs.OfType<TagLib.Mpeg4.IsoAudioSampleEntry>())
                    {
                        if (codec.AudioSampleSize != 0)
                        {
                            this.AddProperty(metaData, CommonProperties.BitsPerSample, codec.AudioSampleSize.ToString());
                            break;
                        }
                    }
                }
            }, this.ErrorHandler);
        }

        private void AddProperty(IList<MetaDataItem> metaData, string name, string value)
        {
            if (!this.HasValue(value))
            {
                return;
            }
            metaData.Add(new MetaDataItem(name, MetaDataItemType.Property) { Value = value.Trim() });
        }

        private async Task AddAdditional(IList<MetaDataItem> metaData, File file, IMetaDataSource metaDataSource)
        {
            try
            {
                metaData.AddRange(await metaDataSource.GetMetaData(file.Name).ConfigureAwait(false));
            }
            catch (Exception e)
            {
                this.ErrorHandler(e);
            }
        }

        private async Task<bool> AddImages(IList<MetaDataItem> metaData, string fileName)
        {
            var types = ArtworkType.None;
            foreach (var type in new[] { ArtworkType.FrontCover, ArtworkType.BackCover })
            {
                if (!ArtworkTypes.HasFlag(type))
                {
                    continue;
                }
                var value = this.ArtworkProvider.Find(fileName, type);
                if (!string.IsNullOrEmpty(value) && global::System.IO.File.Exists(value))
                {
                    if (this.CopyImages.Value)
                    {
                        value = await this.ImportImage(value, value, false).ConfigureAwait(false);
                    }
                    metaData.Add(new MetaDataItem()
                    {
                        Name = Enum.GetName(typeof(ArtworkType), type),
                        Value = value,
                        Type = MetaDataItemType.Image
                    });
                    if (ArtworkTypes.HasFlag(types |= type))
                    {
                        //We have everything we need.
                        return true;
                    }
                }
            }
            return types != ArtworkType.None;
        }

        private async Task<bool> AddImages(IList<MetaDataItem> metaData, string name, File file, Tag tag, IPicture[] pictures)
        {
            var types = ArtworkType.None;
            try
            {
                foreach (var picture in pictures.OrderBy(picture => GetPicturePriority(picture)))
                {
                    var type = GetArtworkType(picture.Type);
                    if (!ArtworkTypes.HasFlag(type) || types.HasFlag(type))
                    {
                        continue;
                    }
                    if (picture.Data.Count > this.MaxImageSize.Value * 1024000)
                    {
                        Logger.Write(this, LogLevel.Warn, "Not importing image from file \"{0}\" due to size.");
                        this.AddWarning(file.Name, string.Format("Not importing image from file \"{0}\" due to size: {1} > {2}", file.Name, picture.Data.Count, this.MaxImageSize.Value * 1024000));
                        continue;
                    }
                    metaData.Add(new MetaDataItem(Enum.GetName(typeof(ArtworkType), type), MetaDataItemType.Image)
                    {
                        Value = await this.ImportImage(file, tag, picture, type, false).ConfigureAwait(false)
                    });
                    if (ArtworkTypes.HasFlag(types |= type))
                    {
                        //We have everything we need.
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to read pictures: {0} => {1}", file.Name, e.Message);
            }
            return types != ArtworkType.None;
        }

        private async Task<string> ImportImage(File file, Tag tag, IPicture picture, ArtworkType type, bool overwrite)
        {
            var id = this.GetPictureId(file, tag, picture, type);
            return await this.ImportImage(picture, id, overwrite).ConfigureAwait(false);
        }

        private async Task<string> ImportImage(IPicture value, string id, bool overwrite)
        {
            var prefix = this.GetType().Name;
            var result = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
            {
                using (await KeyLock.LockAsync(id).ConfigureAwait(false))
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, value.Data.Data).ConfigureAwait(false);
                    }
                }
            }
            return result;
        }

        private async Task<string> ImportImage(string fileName, string id, bool overwrite)
        {
            if (FileMetaDataStore.Contains(fileName))
            {
                return fileName;
            }
            var prefix = this.GetType().Name;
            var result = default(string);
            if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
            {
                using (await KeyLock.LockAsync(id).ConfigureAwait(false))
                {
                    if (overwrite || !FileMetaDataStore.Exists(prefix, id, out result))
                    {
                        return await FileMetaDataStore.WriteAsync(prefix, id, fileName).ConfigureAwait(false);
                    }
                }
            }
            return result;
        }

        private void SetTag(MetaDataItem metaDataItem, File file, Tag tag)
        {
            if (string.Equals(metaDataItem.Name, CommonMetaData.Album, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Album = metaDataItem.Value, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Artist, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.AlbumArtists = new[] { metaDataItem.Value }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Composer, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Composers = new[] { metaDataItem.Value }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Conductor, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Conductor = metaDataItem.Value, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Disc, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() =>
                {
                    var disc = default(uint);
                    if (uint.TryParse(metaDataItem.Value, out disc))
                    {
                        tag.Disc = disc;
                    }
                    else
                    {
                        tag.Disc = 0;
                    }
                }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.DiscCount, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() =>
                {
                    var discCount = default(uint);
                    if (uint.TryParse(metaDataItem.Value, out discCount))
                    {
                        tag.DiscCount = discCount;
                    }
                    else
                    {
                        tag.DiscCount = 0;
                    }
                }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Genre, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Genres = new[] { metaDataItem.Value }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonStatistics.LastPlayed, StringComparison.OrdinalIgnoreCase))
            {
                if (this.Popularimeter.Value && MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value).HasFlag(WriteBehaviour.Statistics))
                {
                    this.Try(() => PopularimeterManager.Write(this, metaDataItem, file), this.ErrorHandler);
                }
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Performer, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Performers = new[] { metaDataItem.Value }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonStatistics.PlayCount, StringComparison.OrdinalIgnoreCase))
            {
                if (this.Popularimeter.Value && MetaDataBehaviourConfiguration.GetWriteBehaviour(this.Write.Value).HasFlag(WriteBehaviour.Statistics))
                {
                    this.Try(() => PopularimeterManager.Write(this, metaDataItem, file), this.ErrorHandler);
                }
            }
            else if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
            {
                if (this.Popularimeter.Value)
                {
                    this.Try(() => PopularimeterManager.Write(this, metaDataItem, file), this.ErrorHandler);
                }
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumGain, StringComparison.OrdinalIgnoreCase) || string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumPeak, StringComparison.OrdinalIgnoreCase) || string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackGain, StringComparison.OrdinalIgnoreCase) || string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackPeak, StringComparison.OrdinalIgnoreCase))
            {
                if (this.ReplayGain.Value)
                {
                    this.Try(() => ReplayGainManager.Write(this, metaDataItem, file), this.ErrorHandler);
                }
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Title, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() => tag.Title = metaDataItem.Value, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Track, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() =>
                {
                    var track = default(uint);
                    if (uint.TryParse(metaDataItem.Value, out track))
                    {
                        tag.Track = track;
                    }
                    else
                    {
                        tag.Track = 0;
                    }
                }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.TrackCount, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() =>
                {
                    var trackCount = default(uint);
                    if (uint.TryParse(metaDataItem.Value, out trackCount))
                    {
                        tag.TrackCount = trackCount;
                    }
                    else
                    {
                        tag.TrackCount = 0;
                    }
                }, this.ErrorHandler);
            }
            else if (string.Equals(metaDataItem.Name, CommonMetaData.Year, StringComparison.OrdinalIgnoreCase))
            {
                this.Try(() =>
                {
                    var year = default(uint);
                    if (uint.TryParse(metaDataItem.Value, out year))
                    {
                        tag.Year = year;
                    }
                    else
                    {
                        tag.Year = 0;
                    }
                }, this.ErrorHandler);
            }
        }

        private Task SetAdditional(IEnumerable<MetaDataItem> metaData, File file, IMetaDataSource metaDataSource)
        {
            return metaDataSource.SetMetaData(file.Name, metaData, metaDataItem => true);
        }

        private async Task SetImage(MetaDataItem metaDataItem, File file, Tag tag)
        {
            var index = default(int);
            var pictures = new List<IPicture>(tag.Pictures);
            if (this.HasImage(metaDataItem.Name, tag, pictures, out index))
            {
                if (!string.IsNullOrEmpty(metaDataItem.Value))
                {
                    await this.ReplaceImage(metaDataItem, file, tag, pictures, index).ConfigureAwait(false);
                }
                else
                {
                    this.RemoveImage(metaDataItem, tag, pictures, index);
                }
            }
            else if (!string.IsNullOrEmpty(metaDataItem.Value))
            {
                await this.AddImage(metaDataItem, file, tag, pictures).ConfigureAwait(false);
            }
            tag.Pictures = pictures.ToArray();
        }

        private bool HasImage(string name, Tag tag, IList<IPicture> pictures, out int index)
        {
            var type = GetArtworkType(name);
            for (var a = 0; a < pictures.Count; a++)
            {
                if (pictures[a] != null && GetArtworkType(pictures[a].Type) == type)
                {
                    index = a;
                    return true;
                }
            }
            index = default(int);
            return false;
        }

        private async Task AddImage(MetaDataItem metaDataItem, File file, Tag tag, IList<IPicture> pictures)
        {
            pictures.Add(await this.CreateImage(metaDataItem, file, tag).ConfigureAwait(false));
        }

        private async Task ReplaceImage(MetaDataItem metaDataItem, File file, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures[index] = await this.CreateImage(metaDataItem, file, tag).ConfigureAwait(false);
        }

        private void RemoveImage(MetaDataItem metaDataItem, Tag tag, IList<IPicture> pictures, int index)
        {
            pictures.RemoveAt(index);
        }

        private async Task<IPicture> CreateImage(MetaDataItem metaDataItem, File file, Tag tag)
        {
            var type = GetArtworkType(metaDataItem.Name);
            var picture = new Picture(metaDataItem.Value)
            {
                Type = GetPictureType(type),
                MimeType = MimeMapping.Instance.GetMimeType(metaDataItem.Value)
            };
            metaDataItem.Value = await this.ImportImage(file, tag, picture, type, true).ConfigureAwait(false);
            return picture;
        }

        private void ErrorHandler(Exception e)
        {
            Logger.Write(this, LogLevel.Error, "Failed to read meta data: {0}", e.Message);
        }

        private string GetPictureId(File file, Tag tag, IPicture picture, ArtworkType type)
        {
            //Year + (Album | Checksum) + Type
            var hashCode = default(long);
            unchecked
            {
                if (tag.Year != 0)
                {
                    hashCode = (hashCode * 29) + tag.Year.GetHashCode();
                }
                if (!string.IsNullOrEmpty(tag.Album))
                {
                    hashCode += tag.Album.ToLower().GetHashCode();
                }
                else
                {
                    hashCode += picture.Data.Checksum;
                }
                hashCode = (hashCode * 29) + type.GetHashCode();
            }
            return Math.Abs(hashCode).ToString();
        }

        public static readonly IDictionary<ArtworkType, PictureType> ArtworkTypeMapping = new Dictionary<ArtworkType, PictureType>()
        {
            { ArtworkType.FrontCover, PictureType.FrontCover },
            { ArtworkType.BackCover, PictureType.BackCover },
        };

        public static PictureType GetPictureType(ArtworkType artworkType)
        {
            var pictureType = default(PictureType);
            if (ArtworkTypeMapping.TryGetValue(artworkType, out pictureType))
            {
                return pictureType;
            }
            return PictureType.NotAPicture;
        }

        private byte GetPicturePriority(IPicture picture)
        {
            switch (picture.Type)
            {
                case PictureType.FrontCover:
                case PictureType.BackCover:
                    return 0;
            }
            return 255;
        }

        public static readonly IDictionary<PictureType, ArtworkType> PictureTypeMapping = new Dictionary<PictureType, ArtworkType>()
        {
            { PictureType.FrontCover, ArtworkType.FrontCover },
            { PictureType.BackCover, ArtworkType.BackCover },
            { PictureType.NotAPicture, ArtworkType.FrontCover } //This seems to be pretty common...
        };

        public static ArtworkType GetArtworkType(string name)
        {
            var artworkType = default(ArtworkType);
            if (Enum.TryParse<ArtworkType>(name, out artworkType))
            {
                return artworkType;
            }
            return ArtworkType.Unknown;
        }

        public static ArtworkType GetArtworkType(PictureType pictureType)
        {
            var artworkType = default(ArtworkType);
            if (PictureTypeMapping.TryGetValue(pictureType, out artworkType))
            {
                return artworkType;
            }
            return ArtworkType.Unknown;
        }

        public static class ImageManager
        {
            public static async Task<bool> Read(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                var embedded = source.EmbeddedImages.Value;
                var loose = source.LooseImages.Value;
                if (embedded && loose)
                {
                    switch (MetaDataBehaviourConfiguration.GetImagesPreference(source.ImagesPreference.Value))
                    {
                        default:
                        case ImagePreference.Embedded:
                            return await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false) || await ReadLoose(source, metaDatas, file).ConfigureAwait(false);
                        case ImagePreference.Loose:
                            return await ReadLoose(source, metaDatas, file).ConfigureAwait(false) || await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false);
                    }
                }
                else if (embedded)
                {
                    return await ReadEmbedded(source, metaDatas, file).ConfigureAwait(false);
                }
                else if (loose)
                {
                    return await ReadLoose(source, metaDatas, file).ConfigureAwait(false);
                }
                return false;
            }

            private static Task<bool> ReadEmbedded(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                if (file.InvariantStartPosition > MAX_TAG_SIZE)
                {
                    Logger.Write(source, LogLevel.Warn, "Not importing images from file \"{0}\" due to size: {1} > {2}", file.Name, file.InvariantStartPosition, MAX_TAG_SIZE);
                }
                else
                {
                    var pictures = file.Tag.Pictures;
                    if (pictures != null)
                    {
                        return source.AddImages(
                            metaDatas,
                            CommonMetaData.Pictures,
                            file,
                            file.Tag,
                            pictures
                        );
                    }
                }
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.FromResult(false);
#endif
            }

            private static Task<bool> ReadLoose(TagLibMetaDataSource source, IList<MetaDataItem> metaDatas, File file)
            {
                return source.AddImages(metaDatas, file.Name);
            }
        }

        public static class ReplayGainManager
        {
            public static void Read(TagLibMetaDataSource source, IList<MetaDataItem> metaData, File file)
            {
                var tag = file.Tag;
                if (tag.ReplayGainAlbumPeak != 0 && !double.IsNaN(tag.ReplayGainAlbumPeak) && !double.IsInfinity(tag.ReplayGainAlbumPeak))
                {
                    source.AddTag(metaData, CommonMetaData.ReplayGainAlbumPeak, tag.ReplayGainAlbumPeak.ToString());
                }
                if (tag.ReplayGainAlbumGain != 0 && !double.IsNaN(tag.ReplayGainAlbumGain) && !double.IsInfinity(tag.ReplayGainAlbumGain))
                {
                    source.AddTag(metaData, CommonMetaData.ReplayGainAlbumGain, tag.ReplayGainAlbumGain.ToString());
                }
                if (tag.ReplayGainTrackPeak != 0 && !double.IsNaN(tag.ReplayGainTrackPeak) && !double.IsInfinity(tag.ReplayGainTrackPeak))
                {
                    source.AddTag(metaData, CommonMetaData.ReplayGainTrackPeak, tag.ReplayGainTrackPeak.ToString());
                }
                if (tag.ReplayGainTrackGain != 0 && !double.IsNaN(tag.ReplayGainTrackGain) && !double.IsInfinity(tag.ReplayGainTrackGain))
                {
                    source.AddTag(metaData, CommonMetaData.ReplayGainTrackGain, tag.ReplayGainTrackGain.ToString());
                }
            }

            public static void Write(TagLibMetaDataSource source, MetaDataItem metaDataItem, File file)
            {
                var tag = file.Tag;
                var value = default(double);
                if (string.IsNullOrEmpty(metaDataItem.Value))
                {
                    value = double.NaN;
                }
                else if (!double.TryParse(metaDataItem.Value, out value))
                {
                    value = double.NaN;
                }
                if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumGain, StringComparison.OrdinalIgnoreCase))
                {
                    tag.ReplayGainAlbumGain = value;
                }
                else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainAlbumPeak, StringComparison.OrdinalIgnoreCase))
                {
                    tag.ReplayGainAlbumPeak = value;
                }
                else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackGain, StringComparison.OrdinalIgnoreCase))
                {
                    tag.ReplayGainTrackGain = value;
                }
                else if (string.Equals(metaDataItem.Name, CommonMetaData.ReplayGainTrackPeak, StringComparison.OrdinalIgnoreCase))
                {
                    tag.ReplayGainTrackPeak = value;
                }
            }
        }

        public static class PopularimeterManager
        {
            public static void Read(TagLibMetaDataSource source, IList<MetaDataItem> metaData, File file)
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                //If it's an Id3v2 tag then try to read the popularimeter frame.
                //It can contain a rating and a play counter.
                if (file.TagTypes.HasFlag(TagTypes.Id3v2))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    if (tag == null)
                    {
                        return;
                    }
                    foreach (var frame in tag.GetFrames<global::TagLib.Id3v2.PopularimeterFrame>())
                    {
                        ReadPopularimeterFrame(frame, result);
                    }
                }
                //If we didn't find a popularimeter frame then attempt to read the rating from a custom tag.
                if (!result.ContainsKey(CommonStatistics.Rating))
                {
                    var rating = ReadCustomTag(CommonStatistics.Rating, file);
                    if (!string.IsNullOrEmpty(rating))
                    {
                        result.Add(CommonStatistics.Rating, Convert.ToString(GetRatingStars(rating)));
                    }
                    else
                    {
                        result.Add(CommonStatistics.Rating, string.Empty);
                    }
                }
                //If we didn't find a popularimeter frame then attempt to read the play count from a custom tag.
                if (!result.ContainsKey(CommonStatistics.PlayCount))
                {
                    var playCount = ReadCustomTag(CommonStatistics.PlayCount, file);
                    if (!string.IsNullOrEmpty(playCount))
                    {
                        result.Add(CommonStatistics.PlayCount, playCount);
                    }
                    else
                    {
                        result.Add(CommonStatistics.PlayCount, "0");
                    }
                }
                //Popularimeter frame does not support last played, attempt to read the play count from a custom tag.
                //if (!result.ContainsKey(CommonMetaData.LastPlayed))
                {
                    var lastPlayed = ReadCustomTag(CommonStatistics.LastPlayed, file);
                    if (!string.IsNullOrEmpty(lastPlayed))
                    {
                        result.Add(CommonStatistics.LastPlayed, lastPlayed);
                    }
                    else
                    {
                        result.Add(CommonStatistics.LastPlayed, string.Empty);
                    }
                }
                //Copy our informations back to the meta data collection.
                foreach (var key in result.Keys)
                {
                    var value = result[key];
                    source.AddTag(metaData, key, value);
                }
            }

            private static void ReadPopularimeterFrame(global::TagLib.Id3v2.PopularimeterFrame frame, IDictionary<string, string> result)
            {
                if (frame.Rating > 0)
                {
                    result.Add(CommonStatistics.Rating, Convert.ToString(GetRatingStars(frame.Rating)));
                }
                if (frame.PlayCount > 0)
                {
                    result.Add(CommonStatistics.PlayCount, Convert.ToString(frame.PlayCount));
                }
            }

            public static void Write(TagLibMetaDataSource source, MetaDataItem metaDataItem, File file)
            {
                if (file.TagTypes.HasFlag(TagTypes.Id3v2) && (new[] { CommonStatistics.Rating, CommonStatistics.PlayCount }).Contains(metaDataItem.Name, true))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    var frames = tag.GetFrames<global::TagLib.Id3v2.PopularimeterFrame>();
                    if (frames != null && frames.Any())
                    {
                        foreach (var frame in frames)
                        {
                            WritePopularimeterFrame(frame, metaDataItem);
                        }
                    }
                    else
                    {
                        var frame = new global::TagLib.Id3v2.PopularimeterFrame(string.Empty);
                        WritePopularimeterFrame(frame, metaDataItem);
                        tag.AddFrame(frame);
                    }
                }
                else if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
                {
                    WriteCustomTag(metaDataItem.Name, Convert.ToString(GetRatingMask(metaDataItem.Value)), file);
                }
                else
                {
                    WriteCustomTag(metaDataItem.Name, metaDataItem.Value, file);
                }
            }

            private static void WritePopularimeterFrame(global::TagLib.Id3v2.PopularimeterFrame frame, MetaDataItem metaDataItem)
            {
                if (string.Equals(metaDataItem.Name, CommonStatistics.Rating, StringComparison.OrdinalIgnoreCase))
                {
                    frame.Rating = GetRatingMask(metaDataItem.Value);
                }
                else if (string.Equals(metaDataItem.Name, CommonStatistics.PlayCount, StringComparison.OrdinalIgnoreCase))
                {
                    frame.PlayCount = Convert.ToUInt64(metaDataItem.Value);
                }
            }

            private static T GetTag<T>(File file, TagTypes tagTypes) where T : Tag
            {
                return file.GetTag(tagTypes) as T;
            }

            private static string ReadCustomTag(string name, File file)
            {
                var key = GetCustomTagName(name);
                if (file.TagTypes.HasFlag(TagTypes.Id3v2))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    if (tag != null)
                    {
                        var frame = global::TagLib.Id3v2.PrivateFrame.Get(tag, key, false);
                        if (frame != null && frame.PrivateData != null && frame.PrivateData.Count > 0)
                        {
                            try
                            {
                                //We're sort of sure that it's UTF-16.
                                return Encoding.Unicode.GetString(frame.PrivateData.Data);
                            }
                            catch
                            {
                                //Nothing can be done, wasn't in the expected format.
                            }
                        }
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Apple))
                {
                    var tag = GetTag<global::TagLib.Mpeg4.AppleTag>(file, TagTypes.Apple);
                    if (tag != null)
                    {
                        return tag.GetDashBox("com.apple.iTunes", key);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Xiph))
                {
                    var tag = GetTag<global::TagLib.Ogg.XiphComment>(file, TagTypes.Xiph);
                    if (tag != null)
                    {
                        return tag.GetFirstField(key);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Ape))
                {
                    var tag = GetTag<global::TagLib.Ape.Tag>(file, TagTypes.Ape);
                    if (tag != null)
                    {
                        var item = tag.GetItem(key);
                        if (item != null)
                        {
                            return item.ToStringArray().FirstOrDefault();
                        }
                    }
                }
                //Not implemented.
                return null;
            }

            private static void WriteCustomTag(string name, string value, File file)
            {
                var key = GetCustomTagName(name);
                if (file.TagTypes.HasFlag(TagTypes.Id3v2))
                {
                    var tag = GetTag<global::TagLib.Id3v2.Tag>(file, TagTypes.Id3v2);
                    if (tag != null)
                    {
                        var frame = global::TagLib.Id3v2.PrivateFrame.Get(tag, key, true);
                        //We're sort of sure that it's UTF-16.
                        frame.PrivateData = Encoding.Unicode.GetBytes(value);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Apple))
                {
                    const string PREFIX = "\0\0\0\0";
                    const string MEAN = "com.apple.iTunes";
                    var apple = GetTag<global::TagLib.Mpeg4.AppleTag>(file, TagTypes.Apple);
                    if (apple != null)
                    {
                        while (apple.GetDashBox(MEAN, key) != null)
                        {
                            apple.SetDashBox(MEAN, key, string.Empty);
                        }
                        apple.SetDashBox(PREFIX + MEAN, PREFIX + key, value);
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Xiph))
                {
                    var xiph = GetTag<global::TagLib.Ogg.XiphComment>(file, TagTypes.Xiph);
                    if (xiph != null)
                    {
                        xiph.SetField(key, new[] { value });
                    }
                }
                else if (file.TagTypes.HasFlag(TagTypes.Ape))
                {
                    var ape = GetTag<global::TagLib.Ape.Tag>(file, TagTypes.Ape);
                    if (ape != null)
                    {
                        ape.SetValue(key, value);
                    }
                }
                //Not implemented.
            }
        }

        private static string GetCustomTagName(string name)
        {
            if (string.Equals(name, CommonStatistics.LastPlayed, StringComparison.OrdinalIgnoreCase))
            {
                //TODO: I can't work out what the standard is for this value.
                return "last_played_timestamp";
            }
            else if (string.Equals(name, CommonStatistics.PlayCount, StringComparison.OrdinalIgnoreCase))
            {
                return "play_count";
            }
            return name.ToLower();
        }

        private static byte GetRatingMask(string rating)
        {
            var temp = default(byte);
            if (!byte.TryParse(rating, out temp))
            {
                return 0;
            }
            return GetRatingMask(temp);
        }

        private static byte GetRatingMask(byte rating)
        {
            //TODO: We only write WMP style ratings but this should be configurable.
            const byte RATING_0 = 0;
            const byte RATING_1 = 1;
            const byte RATING_2 = 64;
            const byte RATING_3 = 128;
            const byte RATING_4 = 196;
            const byte RATING_5 = 255;

            switch (rating)
            {
                default:
                case 0:
                    return RATING_0;
                case 1:
                    return RATING_1;
                case 2:
                    return RATING_2;
                case 3:
                    return RATING_3;
                case 4:
                    return RATING_4;
                case 5:
                    return RATING_5;
            }
        }

        private static byte GetRatingStars(string rating)
        {
            var temp = default(byte);
            if (!byte.TryParse(rating, out temp))
            {
                return 0;
            }
            return GetRatingStars(temp);
        }

        private static byte GetRatingStars(byte rating)
        {
            //There are no solid definitions of what 0-255 rating maps to 0-5 stars.
            //This logic was adapted various sources.
            //iTunes ratings are 0-100 but there's no good way to identify them as such.

            const byte RATING_0 = 0;
            const byte RATING_1 = 1;
            const byte RATING_2 = 2;
            const byte RATING_3 = 3;
            const byte RATING_4 = 4;
            const byte RATING_5 = 5;

            //First try the simple approach.
            switch (rating)
            {
                case 0:
                    return RATING_0;
                case 1:
                    return RATING_1;
                case 64:
                    return RATING_2;
                case 128:
                    return RATING_3;
                case 196:
                    return RATING_4;
                case 255:
                    return RATING_5;
            }
            //Then try other things.
            if (rating >= 2 && rating <= 8)
            {
                return RATING_0;
            }
            else if (rating >= 9 && rating <= 18)
            {
                //0.5 rounded up.
                return RATING_1;
            }
            else if (rating >= 19 && rating <= 28)
            {
                return RATING_1;
            }
            else if (rating == 29)
            {
                //1.5 rounded up.
                return RATING_2;
            }
            else if (rating >= 30 && rating <= 39)
            {
                //0.5 rounded up.
                return RATING_1;
            }
            else if (rating >= 40 && rating <= 49)
            {
                return RATING_1;
            }
            else if (rating >= 50 && rating <= 59)
            {
                //1.5 rounded up.
                return RATING_2;
            }
            else if (rating >= 60 && rating <= 69)
            {
                return RATING_2;
            }
            else if (rating >= 70 && rating <= 90)
            {
                //1.5 rounded up.
                return RATING_2;
            }
            else if (rating >= 91 && rating <= 113)
            {
                return RATING_2;
            }
            else if (rating >= 114 && rating <= 123)
            {
                //2.5 rounded up.
                return RATING_3;
            }
            else if (rating >= 124 && rating <= 133)
            {
                return RATING_3;
            }
            else if (rating >= 134 && rating <= 141)
            {
                //2.5 rounded up.
                return RATING_3;
            }
            else if (rating >= 142 && rating <= 167)
            {
                return RATING_3;
            }
            else if (rating >= 168 && rating <= 191)
            {
                //3.5 rounded up.
                return RATING_4;
            }
            else if (rating >= 192 && rating <= 218)
            {
                return RATING_4;
            }
            else if (rating >= 219 && rating <= 247)
            {
                //4.5 rounded up.
                return RATING_5;
            }

            return RATING_5;
        }
    }
}
