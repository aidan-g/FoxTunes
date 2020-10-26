using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class AddLibraryHierarchyNodeToPlaylist
    {
        public AddLibraryHierarchyNodeToPlaylist(IDatabase database, IFilterParserResult filter, ISortParserResult sort)
        {
            this.Database = database;
            this.Filter = filter;
            this.Sort = sort;
        }

        public IDatabase Database { get; private set; }

        public IFilterParserResult Filter { get; private set; }

        public ISortParserResult Sort { get; private set; }

        public IEnumerable<string> Names
        {
            get
            {
                foreach (var expression in this.Sort.Expressions)
                {
                    yield return expression.Name;
                    var child = expression.Child;
                    while (child != null)
                    {
                        yield return child.Name;
                        child = child.Child;
                    }
                }
            }
        }
    }
}
