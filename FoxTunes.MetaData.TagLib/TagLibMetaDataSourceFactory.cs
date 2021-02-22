using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    [Component("679D9459-BBCE-4D95-BB65-DD20C335719C", ComponentSlots.MetaData, @default: true)]
    public class TagLibMetaDataSourceFactory : MetaDataSourceFactory
    {
        public BooleanConfigurationElement Extended { get; private set; }

        public BooleanConfigurationElement MusicBrainz { get; private set; }

        public BooleanConfigurationElement Lyrics { get; private set; }

        public BooleanConfigurationElement ReplayGain { get; private set; }

        public BooleanConfigurationElement Popularimeter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
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
        }

        public override IEnumerable<KeyValuePair<string, MetaDataItemType>> Supported
        {
            get
            {
                if (!this.Enabled)
                {
                    yield break;
                }

                //Tags.
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Album, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Artist, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Composer, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Conductor, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Disc, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.DiscCount, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Genre, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Performer, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Title, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Track, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.TrackCount, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Year, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.BeatsPerMinute, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.InitialKey, MetaDataItemType.Tag);
                if (this.Extended.Value)
                {
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicIpId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.AmazonId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Comment, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Copyright, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Grouping, MetaDataItemType.Tag);
                }
                if (this.MusicBrainz.Value)
                {
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzArtistId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzDiscId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzReleaseArtistId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzReleaseCountry, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzReleaseId, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzReleaseStatus, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzReleaseType, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.MusicBrainzTrackId, MetaDataItemType.Tag);
                }
                if (this.Lyrics.Value)
                {
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Lyrics, MetaDataItemType.Tag);
                }
                if (this.ReplayGain.Value)
                {
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.ReplayGainAlbumPeak, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.ReplayGainAlbumGain, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.ReplayGainTrackPeak, MetaDataItemType.Tag);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.ReplayGainTrackGain, MetaDataItemType.Tag);
                }

                //Properties.
                yield return new KeyValuePair<string, MetaDataItemType>(CommonProperties.Duration, MetaDataItemType.Property);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonProperties.AudioBitrate, MetaDataItemType.Property);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonProperties.AudioChannels, MetaDataItemType.Property);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonProperties.AudioSampleRate, MetaDataItemType.Property);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonProperties.BitsPerSample, MetaDataItemType.Property);

                //Statistics.
                if (this.Popularimeter.Value)
                {
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonStatistics.Rating, MetaDataItemType.Statistic);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonStatistics.LastPlayed, MetaDataItemType.Statistic);
                    yield return new KeyValuePair<string, MetaDataItemType>(CommonStatistics.PlayCount, MetaDataItemType.Statistic);
                }
            }
        }

        public override IMetaDataSource Create()
        {
            var source = new TagLibMetaDataSource();
            source.InitializeComponent(this.Core);
            return source;
        }
    }
}
