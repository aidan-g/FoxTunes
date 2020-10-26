using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class PlaylistSortBuilder
    {
        public PlaylistSortBuilder(IDatabase database, ISortParserResult sort)
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

        public string GetColumn(string name)
        {
            var index = this.Names.IndexOf(name, StringComparer.OrdinalIgnoreCase);
            if (index == -1)
            {
                throw new NotImplementedException();
            }
            return string.Format("Value_{0}_Value", index);
        }
    }
}
