using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IMetaDataSourceFactory : IStandardFactory
    {
        bool Enabled { get; }

        IEnumerable<KeyValuePair<string, MetaDataItemType>> Supported { get; }

        IMetaDataSource Create();
    }
}
