using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public static class Profiles
    {
        private static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        private static readonly string ConfigurationFileName = Path.Combine(
            Publication.StoragePath,
            "Profiles.txt"
        );

        private static ResettableLazy<IEnumerable<string>> _AvailableProfiles = new ResettableLazy<IEnumerable<string>>(() =>
        {
            var profiles = new List<string>();
            if (File.Exists(ConfigurationFileName))
            {
                try
                {
                    using (var stream = File.OpenRead(ConfigurationFileName))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                profiles.Add(line);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(typeof(Profiles), LogLevel.Warn, "Failed to read available profiles: {0}", e.Message);
                }
            }
            if (!profiles.Contains(Strings.Profiles_Default, true))
            {
                profiles.Insert(0, Strings.Profiles_Default);
            }
            Logger.Write(typeof(Profiles), LogLevel.Warn, "Available profiles: {0}", string.Join(", ", profiles));
            return profiles;
        });

        public static IEnumerable<string> AvailableProfiles
        {
            get
            {
                return _AvailableProfiles.Value;
            }
        }

        public static string Profile
        {
            get
            {
                return AvailableProfiles.FirstOrDefault();
            }
            set
            {
                if (string.Equals(Profile, value))
                {
                    //Profile is unchanged, nothing to do.
                    return;
                }
                Add(value);
            }
        }

        public static void Add(string profile)
        {
            Update(profile, false);
        }

        public static void Delete(string profile)
        {
            Update(profile, true);
        }

        private static void Update(string profile, bool delete)
        {
            var profiles = AvailableProfiles.ToList();
            profiles.RemoveAll(
                _profile => string.Equals(_profile, profile, StringComparison.OrdinalIgnoreCase)
            );
            if (!delete)
            {
                profiles.Insert(0, profile);
            }
            try
            {
                using (var stream = File.Create(ConfigurationFileName))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var _profile in profiles)
                        {
                            writer.WriteLine(_profile);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Write(typeof(Profiles), LogLevel.Warn, "Failed to write available profiles: {0}", e.Message);
            }
            _AvailableProfiles.Reset();
        }

        public static string GetFileName(string profile)
        {
            if (string.IsNullOrEmpty(profile) || string.Equals(profile, Strings.Profiles_Default, StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(
                    Publication.StoragePath,
                    "Settings.xml"
                );
            }
            return Path.Combine(
                Publication.StoragePath,
                string.Format("Settings_{0}.xml", Convert.ToString(Math.Abs(profile.GetHashCode())))
            );
        }
    }
}
