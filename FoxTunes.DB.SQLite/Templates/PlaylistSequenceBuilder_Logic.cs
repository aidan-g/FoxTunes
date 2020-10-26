using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class PlaylistSequenceBuilder
    {
        public PlaylistSequenceBuilder(IDatabase database, ISortParserResult sort)
        {
            this.Database = database;
            this.Sort = sort;
        }

        public IDatabase Database { get; private set; }

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
