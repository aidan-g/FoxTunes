using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes.ViewModel
{
    public static class MetaDataSelector
    {
        public static readonly IEnumerable<MetaDataElement> Elements = GetElements();

        private static IEnumerable<MetaDataElement> GetElements()
        {
            var elements = Enumerable.Empty<MetaDataElement>();
            elements = elements.Concat(CommonMetaData.Lookup.Keys.Select(name => new MetaDataElement(name, MetaDataItemType.Tag)));
            elements = elements.Concat(CommonProperties.Lookup.Keys.Select(name => new MetaDataElement(name, MetaDataItemType.Property)));
            elements = elements.Concat(CommonStatistics.Lookup.Keys.Select(name => new MetaDataElement(name, MetaDataItemType.Statistic)));
            return elements.ToArray();
        }

        public static readonly IEnumerable<string> Formats = GetFormats();

        private static IEnumerable<string> GetFormats()
        {
            return new[]
            {
                CommonFormats.TimeSpan
            };
        }

        public class MetaDataElement
        {
            public MetaDataElement(string name, MetaDataItemType type)
            {
                this.Name = name;
                this.Type = Enum.GetName(typeof(MetaDataItemType), type);
            }

            public string Name { get; private set; }

            public string Type { get; private set; }
        }
    }
}
