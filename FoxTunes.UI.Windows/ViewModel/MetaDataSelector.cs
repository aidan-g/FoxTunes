using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes.ViewModel
{
    public static class MetaDataSelector
    {
        public static readonly IMetaDataSourceFactory Factory = ComponentRegistry.Instance.GetComponent<IMetaDataSourceFactory>();

        public static readonly IEnumerable<MetaDataElement> Elements = GetElements();

        private static IEnumerable<MetaDataElement> GetElements()
        {
            var supported = Factory.Supported.ToArray();
            var elements = Enumerable.Empty<MetaDataElement>();
            elements = elements.Concat(GetElements(CommonMetaData.Lookup.Keys, MetaDataItemType.Tag, supported));
            elements = elements.Concat(GetElements(CommonProperties.Lookup.Keys, MetaDataItemType.Property, supported));
            elements = elements.Concat(GetElements(CommonStatistics.Lookup.Keys, MetaDataItemType.Statistic, supported));
            return elements.ToArray();
        }

        private static IEnumerable<MetaDataElement> GetElements(IEnumerable<string> names, MetaDataItemType type, IEnumerable<KeyValuePair<string, MetaDataItemType>> supported)
        {
            foreach (var name in names)
            {
                if (!supported.Any(element => string.Equals(element.Key, name, StringComparison.OrdinalIgnoreCase) && element.Value == type))
                {
                    continue;
                }
                yield return new MetaDataElement(name, type);
            }
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
