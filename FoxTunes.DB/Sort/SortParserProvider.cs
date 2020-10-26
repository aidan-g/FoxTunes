using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;

namespace FoxTunes.DB.Sort
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public abstract class SortParserProvider : StandardComponent, ISortParserProvider
    {
        private static readonly IDictionary<string, IDictionary<string, string>> Lookups = new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(CommonMetaData), CommonMetaData.Lookup },
            { nameof(CustomMetaData), CustomMetaData.Lookup },
            { nameof(CommonProperties), CommonProperties.Lookup }
        };

        public ISortParser SortParser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.SortParser = core.Components.SortParser;
            this.SortParser.Register(this);
            base.InitializeComponent(core);
        }

        public abstract bool TryParse(string sort, out ISortParserResultExpression expression);

        protected virtual bool TryGetName(string expression, out string name)
        {
            if (string.IsNullOrEmpty(expression))
            {
                name = default(string);
                return false;
            }
            var parts = expression.Split('.');
            if (parts.Length != 2)
            {
                name = default(string);
                return false;
            }
            var lookup = default(IDictionary<string, string>);
            if (!Lookups.TryGetValue(parts[0], out lookup))
            {
                name = default(string);
                return false;
            }
            return lookup.TryGetValue(parts[1], out name);
        }
    }
}
