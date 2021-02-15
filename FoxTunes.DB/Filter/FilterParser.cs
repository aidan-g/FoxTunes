using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class FilterParser : StandardComponent, IFilterParser, IConfigurableComponent
    {
        public FilterParser()
        {
            this.Providers = new List<IFilterParserProvider>();
        }

        public IList<IFilterParserProvider> Providers { get; private set; }

        public void Register(IFilterParserProvider provider)
        {
            this.Providers.Add(provider);
        }

        public bool TryParse(string filter, out IFilterParserResult result)
        {
            var groups = new List<IFilterParserResultGroup>();
            while (!string.IsNullOrEmpty(filter))
            {
                var success = default(bool);
                foreach (var provider in this.Providers)
                {
                    var group = default(IFilterParserResultGroup);
                    if (provider.TryParse(ref filter, out group))
                    {
                        groups.Add(group);
                        success = true;
                        break;
                    }
                }
                if (!success)
                {
                    break;
                }
            }
            result = new FilterParserResult(groups);
            return string.IsNullOrEmpty(filter);
        }

        public bool AppliesTo(string filter, IEnumerable<string> names)
        {
            var result = default(IFilterParserResult);
            if (!this.TryParse(filter, out result))
            {
                return false;
            }
            foreach (var group in result.Groups)
            {
                foreach (var entry in group.Entries)
                {
                    if (names.Contains(entry.Name, true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return FilterParserConfiguration.GetConfigurationSections();
        }

        [Component("219DC49B-0916-4820-BDE2-9354A9586753", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
        [ComponentDependency(Slot = ComponentSlots.Database)]
        public class DefaultFilterParserProvider : FilterParserProvider
        {
            public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

            public IConfiguration Configuration { get; private set; }

            public IEnumerable<string> SearchNames { get; private set; }

            public override void InitializeComponent(ICore core)
            {
                this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
                this.Configuration = core.Components.Configuration;
                this.Configuration.GetElement<TextConfigurationElement>(
                    FilterParserConfiguration.SECTION,
                    FilterParserConfiguration.SEARCH_NAMES
                ).ConnectValue(value =>
                {
                    var reset = this.SearchNames != null;
                    this.SearchNames = FilterParserConfiguration.GetSearchNames(value);
                    if (reset)
                    {
                        //As the results of searches are now different we should clear the cache.
                        this.LibraryHierarchyCache.Reset();
                    }
                });
                base.InitializeComponent(core);
            }

            public override bool TryParse(ref string filter, out IFilterParserResultGroup result)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    result = default(IFilterParserResultGroup);
                    return false;
                }
                result = this.Parse(filter);
                this.OnParsed(ref filter, 0, filter.Length);
                return true;
            }

            protected virtual IFilterParserResultGroup Parse(string filter)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    filter = FilterParserResultEntry.UNBOUNDED_WILDCARD;
                }
                else
                {
                    filter = string.Format(
                        "{0}{1}{0}",
                        FilterParserResultEntry.UNBOUNDED_WILDCARD,
                        filter.Trim()
                    );
                }
                var entries = this.SearchNames.Select(
                    name => new FilterParserResultEntry(name, FilterParserEntryOperator.Match, filter)
                ).ToArray();
                return new FilterParserResultGroup(entries, FilterParserGroupOperator.Or);
            }
        }

        [Component("E49288CB-3FDA-4AE3-862C-F2E0911EE1DD", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_NORMAL)]
        [ComponentDependency(Slot = ComponentSlots.Database)]
        public class KeyValueFilterParserProvider : FilterParserProvider
        {
            const string ENTRY = "ENTRY";

            const string NAME = "NAME";

            const string OPERATOR = "OPERATOR";

            const string VALUE = "VALUE";

            public KeyValueFilterParserProvider()
            {
                var operators = string.Join("|", new[]
                {
                    FilterParserResultEntry.GREATER_EQUAL,
                    FilterParserResultEntry.GREATER,
                    FilterParserResultEntry.LESS_EQUAL,
                    FilterParserResultEntry.LESS,
                    FilterParserResultEntry.EQUAL
                }.Select(element => "(?:" + Regex.Escape(element) + ")"));
                this.Regex = new Regex(
                    "^(?:(?<" + ENTRY + ">(?<" + NAME + ">[a-z]+)\\s*(?<" + OPERATOR + ">" + operators + ")\\s*(?<" + VALUE + ">.+?)))+$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                );
            }

            public Regex Regex { get; private set; }

            public override bool TryParse(ref string filter, out IFilterParserResultGroup result)
            {
                var match = this.Regex.Match(filter);
                if (!match.Success)
                {
                    result = default(IFilterParserResultGroup);
                    return false;
                }
                result = this.Parse(match);
                this.OnParsed(ref filter, match.Index, match.Length);
                return true;
            }

            protected virtual IFilterParserResultGroup Parse(Match match)
            {
                var entries = new List<IFilterParserResultEntry>();
                for (int a = 0, b = match.Groups[ENTRY].Captures.Count; a < b; a++)
                {
                    var name = match.Groups[NAME].Captures[a].Value.Trim();
                    var @operator = FilterParserResultEntry.GetOperator(
                        match.Groups[OPERATOR].Captures[a].Value.Trim()
                    );
                    var value = match.Groups[VALUE].Captures[a].Value.Trim();
                    switch (@operator)
                    {
                        case FilterParserEntryOperator.Match:
                            value = string.Format(
                                "{0}{1}{0}",
                                FilterParserResultEntry.UNBOUNDED_WILDCARD,
                                value
                            );
                            break;
                    }
                    entries.Add(new FilterParserResultEntry(name, @operator, value));
                }
                return new FilterParserResultGroup(entries, FilterParserGroupOperator.And);
            }
        }

        [Component("A14FFBBF-1985-45BA-8B9D-472B875A17FC", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
        [ComponentDependency(Slot = ComponentSlots.Database)]
        public class RatingFilterParserProvider : FilterParserProvider
        {
            const string RATING = "RATING";

            public RatingFilterParserProvider()
            {
                this.Regex = new Regex(
                    "(?:(?<" + RATING + ">[0-5])\\*)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                );
            }

            public Regex Regex { get; private set; }

            public override bool TryParse(ref string filter, out IFilterParserResultGroup result)
            {
                var match = this.Regex.Match(filter);
                if (!match.Success)
                {
                    result = default(IFilterParserResultGroup);
                    return false;
                }
                result = this.Parse(match);
                this.OnParsed(ref filter, match.Index, match.Length);
                return true;
            }

            protected virtual IFilterParserResultGroup Parse(Match match)
            {
                var entries = new List<IFilterParserResultEntry>();
                var rating = match.Groups[RATING];
                if (rating != null)
                {
                    entries.Add(new FilterParserResultEntry(CommonStatistics.Rating, FilterParserEntryOperator.Equal, rating.Value));
                }
                return new FilterParserResultGroup(entries, FilterParserGroupOperator.And);
            }
        }
    }

    public class FilterParserResult : IFilterParserResult
    {
        public FilterParserResult(IEnumerable<IFilterParserResultGroup> groups)
        {
            this.Groups = groups;
        }

        public IEnumerable<IFilterParserResultGroup> Groups { get; private set; }

        public virtual bool Equals(IFilterParserResult other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!Enumerable.SequenceEqual(this.Groups, other.Groups))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IFilterParserResult);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                foreach (var group in this.Groups)
                {
                    hashCode += group.GetHashCode();
                }
            }
            return hashCode;
        }

        public static bool operator ==(FilterParserResult a, FilterParserResult b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(FilterParserResult a, FilterParserResult b)
        {
            return !(a == b);
        }
    }

    public class FilterParserResultGroup : IFilterParserResultGroup
    {
        public FilterParserResultGroup(IEnumerable<IFilterParserResultEntry> entries, FilterParserGroupOperator @operator)
        {
            this.Entries = entries;
            this.Operator = @operator;
        }

        public IEnumerable<IFilterParserResultEntry> Entries { get; private set; }

        public FilterParserGroupOperator Operator { get; private set; }

        public virtual bool Equals(IFilterParserResultGroup other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!Enumerable.SequenceEqual(this.Entries, other.Entries))
            {
                return false;
            }
            if (this.Operator != other.Operator)
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IFilterParserResultGroup);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                foreach (var entry in this.Entries)
                {
                    hashCode += entry.GetHashCode();
                }
                hashCode += this.Operator.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(FilterParserResultGroup a, FilterParserResultGroup b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(FilterParserResultGroup a, FilterParserResultGroup b)
        {
            return !(a == b);
        }
    }

    public class FilterParserResultEntry : IFilterParserResultEntry
    {
        public const string BOUNDED_WILDCARD = "?";

        public const string UNBOUNDED_WILDCARD = "*";

        public const string EQUAL = ":";

        public const string GREATER = ">";

        public const string GREATER_EQUAL = ">:";

        public const string LESS = "<";

        public const string LESS_EQUAL = "<:";

        public static FilterParserEntryOperator GetOperator(string @operator)
        {
            switch (@operator)
            {
                default:
                case EQUAL:
                    return FilterParserEntryOperator.Match;
                case GREATER:
                    return FilterParserEntryOperator.Greater;
                case GREATER_EQUAL:
                    return FilterParserEntryOperator.GreaterEqual;
                case LESS:
                    return FilterParserEntryOperator.Less;
                case LESS_EQUAL:
                    return FilterParserEntryOperator.LessEqual;
            }
        }

        public FilterParserResultEntry(string name, FilterParserEntryOperator @operator, string value)
        {
            this.Name = name;
            this.Operator = @operator;
            this.Value = value;
        }

        public string Name { get; private set; }

        public FilterParserEntryOperator Operator { get; private set; }

        public string Value { get; private set; }

        public virtual bool Equals(IFilterParserResultEntry other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (this.Operator != other.Operator)
            {
                return false;
            }
            if (!string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as IFilterParserResultEntry);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    hashCode += this.Name.ToLower().GetHashCode();
                }
                hashCode += this.Operator.GetHashCode();
                if (!string.IsNullOrEmpty(this.Value))
                {
                    hashCode += this.Value.ToLower().GetHashCode();
                }
            }
            return hashCode;
        }

        public static bool operator ==(FilterParserResultEntry a, FilterParserResultEntry b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            if (object.ReferenceEquals((object)a, (object)b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(FilterParserResultEntry a, FilterParserResultEntry b)
        {
            return !(a == b);
        }
    }
}
