using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [Component("BDAAF3E1-84CC-4D36-A7CB-278663E65844", ComponentSlots.MetaData)]
    public class FileNameMetaDataSourceFactory : MetaDataSourceFactory, IConfigurableComponent
    {
        public IEnumerable<IFileNameMetaDataExtractor> Extractors { get; private set; }

        public string _Patterns { get; private set; }

        public string Patterns
        {
            get
            {
                return this._Patterns;
            }
            set
            {
                this._Patterns = value;
                this.OnPatternsChanged();
            }
        }

        protected virtual void OnPatternsChanged()
        {
            this.Extractors = CreateExtractors(this.Patterns);
        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            this.Configuration.GetElement<TextConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                FileNameMetaDataSourceFactoryConfiguration.PATTERNS_ELEMENT
            ).ConnectValue(value => this.Patterns = value);
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && this.Extractors != null && this.Extractors.Any();
            }
        }

        public override IEnumerable<KeyValuePair<string, MetaDataItemType>> Supported
        {
            get
            {
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Album, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Artist, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Disc, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Genre, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Title, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Track, MetaDataItemType.Tag);
                yield return new KeyValuePair<string, MetaDataItemType>(CommonMetaData.Year, MetaDataItemType.Tag);
            }
        }

        public override IMetaDataSource Create()
        {
            var source = new FileNameMetaDataSource(this.Extractors);
            source.InitializeComponent(this.Core);
            return source;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return FileNameMetaDataSourceFactoryConfiguration.GetConfigurationSections();
        }

        public static IEnumerable<IFileNameMetaDataExtractor> CreateExtractors(string patterns)
        {
            if (!string.IsNullOrEmpty(patterns))
            {
                foreach (var pattern in patterns.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (string.IsNullOrEmpty(pattern) || pattern.StartsWith("//"))
                    {
                        continue;
                    }
                    yield return new FileNameMetaDataExtractor(pattern);
                }
            }
        }
    }
}
