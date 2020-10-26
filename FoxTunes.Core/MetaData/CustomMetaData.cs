using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoxTunes
{
    public static class CustomMetaData
    {
        public const string VariousArtists = "__FT_VariousArtists";

        public const string LeadIn = "__FT_LeadIn";

        public const string LeadOut = "__FT_LeadOut";

        public static IDictionary<string, string> Lookup = GetLookup();

        private static IDictionary<string, string> GetLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof(CustomMetaData).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var name = field.GetValue(null) as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                lookup.Add(name, name);
                if (!string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    lookup.Add(field.Name, name);
                }
            }
            return lookup;
        }
    }
}
