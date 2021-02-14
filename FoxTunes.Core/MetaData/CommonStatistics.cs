using System;
using System.Collections.Generic;
using System.Reflection;

namespace FoxTunes
{
    public static class CommonStatistics
    {
        public const string Rating = "Rating";
        public const string LastPlayed = "LastPlayed";
        public const string PlayCount = "PlayCount";

        public static IDictionary<string, string> Lookup = GetLookup();

        private static IDictionary<string, string> GetLookup()
        {
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof(CommonStatistics).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var name = field.GetValue(null) as string;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                lookup.Add(name, name);
            }
            return lookup;
        }
    }
}
