using FoxTunes.DB.Sort;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class SortParser : StandardComponent, ISortParser
    {
        public SortParser()
        {
            this.Providers = new List<ISortParserProvider>();
        }

        public IList<ISortParserProvider> Providers { get; private set; }

        public void Register(ISortParserProvider provider)
        {
            this.Providers.Add(provider);
        }

        public bool TryParse(string sort, out ISortParserResult result)
        {
            var expressions = new List<ISortParserResultExpression>();
            if (!string.IsNullOrEmpty(sort))
            {
                var lines = sort.Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var success = default(bool);
                    foreach (var provider in this.Providers)
                    {
                        var expression = default(ISortParserResultExpression);
                        if (provider.TryParse(line, out expression))
                        {
                            expressions.Add(expression);
                            success = true;
                            break;
                        }
                    }
                    if (!success)
                    {
                        result = default(ISortParserResult);
                        return false;
                    }
                }
            }
            result = new SortParserResult(expressions);
            return true;
        }

        [Component("EEB65782-C66C-46F8-9F58-BA0E5A16194F", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_LOW)]
        [ComponentDependency(Slot = ComponentSlots.Database)]
        public class DefaultSortParserProvider : SortParserProvider
        {
            public override bool TryParse(string sort, out ISortParserResultExpression expression)
            {
                if (string.IsNullOrEmpty(sort))
                {
                    expression = default(ISortParserResultExpression);
                    return false;
                }
                var name = default(string);
                if (!this.TryGetName(sort, out name))
                {
                    expression = default(ISortParserResultExpression);
                    return false;
                }
                expression = new SortParserResultExpression(name);
                return true;
            }
        }
    }

    [Component("A72D5D30-BD85-4FC8-A166-B9F93D65680F", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_NORMAL)]
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class NumericSortParserProvider : SortParserProvider
    {
        public override bool TryParse(string sort, out ISortParserResultExpression expression)
        {
            if (string.IsNullOrEmpty(sort))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var parts = sort.Split(' ');
            if (parts.Length != 2)
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var name = default(string);
            if (!this.TryGetName(parts[0], out name))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            if (!string.Equals(parts[1], "num", StringComparison.OrdinalIgnoreCase))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            expression = new SortParserResultExpression(name, SortParserResultOperator.Numeric);
            return true;
        }
    }

    [Component("55C94971-C5DE-44D8-B136-8F94C33A0F1D", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public class BinarySortParserProvider : SortParserProvider
    {
        private static readonly IDictionary<string, SortParserResultOperator> Operators = new Dictionary<string, SortParserResultOperator>(StringComparer.OrdinalIgnoreCase)
        {
            { "?", SortParserResultOperator.NullCoalesce }
        };

        public override bool TryParse(string sort, out ISortParserResultExpression expression)
        {
            if (string.IsNullOrEmpty(sort))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var parts = sort.Split(' ');
            if (parts.Length != 3)
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var name1 = default(string);
            if (!this.TryGetName(parts[0], out name1))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var @operator = default(SortParserResultOperator);
            if (!Operators.TryGetValue(parts[1], out @operator))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            var name2 = default(string);
            if (!this.TryGetName(parts[2], out name2))
            {
                expression = default(ISortParserResultExpression);
                return false;
            }
            expression = new SortParserResultExpression(
                name1,
                @operator,
                new SortParserResultExpression(
                    name2
                )
            );
            return true;
        }
    }

    public class SortParserResult : ISortParserResult
    {
        public SortParserResult(IEnumerable<ISortParserResultExpression> expressions)
        {
            this.Expressions = expressions;
        }

        public IEnumerable<ISortParserResultExpression> Expressions { get; private set; }

        public virtual bool Equals(ISortParserResult other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (!Enumerable.SequenceEqual(this.Expressions, other.Expressions))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ISortParserResult);
        }

        public override int GetHashCode()
        {
            var hashCode = default(int);
            unchecked
            {
                foreach (var expression in this.Expressions)
                {
                    hashCode += expression.GetHashCode();
                }
            }
            return hashCode;
        }

        public static bool operator ==(SortParserResult a, SortParserResult b)
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

        public static bool operator !=(SortParserResult a, SortParserResult b)
        {
            return !(a == b);
        }
    }

    public class SortParserResultExpression : ISortParserResultExpression
    {
        public SortParserResultExpression(string name)
        {
            this.Name = name;
        }

        public SortParserResultExpression(string name, SortParserResultOperator @operator) : this(name)
        {
            this.Operator = @operator;
        }

        public SortParserResultExpression(string name, SortParserResultOperator @operator, ISortParserResultExpression child) : this(name, @operator)
        {
            this.Child = child;
        }

        public string Name { get; private set; }

        public SortParserResultOperator Operator { get; private set; }

        public ISortParserResultExpression Child { get; private set; }

        public virtual bool Equals(ISortParserResultExpression other)
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
            if (!object.Equals(this.Child, other.Child))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ISortParserResultExpression);
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
                if (this.Child != null)
                {
                    hashCode += this.Child.GetHashCode();
                }
            }
            return hashCode;
        }

        public static bool operator ==(SortParserResultExpression a, SortParserResultExpression b)
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

        public static bool operator !=(SortParserResultExpression a, SortParserResultExpression b)
        {
            return !(a == b);
        }
    }
}
