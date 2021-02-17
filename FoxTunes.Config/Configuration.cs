using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    [Component("BA77B392-1900-4931-B720-16206B23DDA1", ComponentSlots.Configuration, priority: ComponentAttribute.PRIORITY_HIGH)]
    public class Configuration : StandardComponent, IConfiguration, IDisposable
    {
        const int TIMEOUT = 1000;

        public Configuration()
        {
            this.Debouncer = new Debouncer(TIMEOUT);
            this.Sections = new ObservableCollection<ConfigurationSection>();
        }

        public Debouncer Debouncer { get; private set; }

        private static readonly Lazy<ReleaseType> _ReleaseType = new Lazy<ReleaseType>(() =>
        {
            try
            {
                var value = ConfigurationManager.AppSettings.Get("ReleaseType");
                var result = default(ReleaseType);
                if (string.IsNullOrEmpty(value) || !Enum.TryParse<ReleaseType>(value, out result))
                {
                    Logger.Write(typeof(Configuration), LogLevel.Error, "Failed to parse the release type \"{0}\", falling back to default.", value);
                    return ReleaseType.Default;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Write(typeof(Configuration), LogLevel.Error, "Failed to read the release type, falling back to default: {0}", e.Message);
                return ReleaseType.Default;
            }
        });

        public IEnumerable<string> AvailableProfiles
        {
            get
            {
                return Profiles.AvailableProfiles;
            }
        }

        public string Profile
        {
            get
            {
                return Profiles.Profile;
            }
        }

        public bool IsDefaultProfile
        {
            get
            {
                return string.Equals(this.Profile, Strings.Profiles_Default, StringComparison.OrdinalIgnoreCase);
            }
        }

        public ReleaseType ReleaseType
        {
            get
            {
                return _ReleaseType.Value;
            }
        }

        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        public IConfiguration WithSection(ConfigurationSection section)
        {
            if (this.Contains(section.Id))
            {
                this.Update(section);
            }
            else
            {
                this.Add(section);
            }
            return this;
        }

        public bool Contains(string id)
        {
            return this.GetSection(id) != null;
        }

        private void Add(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Adding configuration section: {0} => {1}", section.Id, section.Name);
            this.Sections.Add(section);
        }

        private void Update(ConfigurationSection section)
        {
            Logger.Write(this, LogLevel.Debug, "Updating configuration section: {0} => {1}", section.Id, section.Name);
            var existing = this.GetSection(section.Id);
            existing.Update(section);
        }

        public void Load()
        {
            this.Load(this.Profile);
        }

        public void Load(string profile)
        {
            foreach (var section in this.Sections)
            {
                if (section.IsInitialized)
                {
                    continue;
                }
                section.InitializeComponent();
            }
            var fileName = Profiles.GetFileName(profile);
            if (!File.Exists(fileName))
            {
                Logger.Write(this, LogLevel.Debug, "Configuration file \"{0}\" does not exist.", fileName);
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Loading configuration from file \"{0}\".", fileName);
            try
            {
                var modifiedElements = this.GetModifiedElements();
                var restoredElements = new List<ConfigurationElement>();
                using (var stream = File.OpenRead(fileName))
                {
                    var sections = Serializer.Load(stream);
                    foreach (var section in sections)
                    {
                        if (!this.Contains(section.Key))
                        {
                            //If config was created by a component that is no longer loaded then it will be lost here.
                            //TODO: Add the config but hide it so it's preserved but not displayed.
                            Logger.Write(this, LogLevel.Warn, "Configuration section \"{0}\" no longer exists.", section.Key);
                            continue;
                        }
                        var existing = this.GetSection(section.Key);
                        try
                        {
                            Logger.Write(this, LogLevel.Debug, "Loading configuration section \"{0}\".", section.Key);
                            restoredElements.AddRange(this.Load(existing, section.Value));
                        }
                        catch (Exception e)
                        {
                            Logger.Write(this, LogLevel.Warn, "Failed to load configuration section \"{0}\": {1}", existing.Id, e.Message);
                        }
                    }
                }
                foreach (var modifiedElement in modifiedElements)
                {
                    if (restoredElements.Contains(modifiedElement))
                    {
                        continue;
                    }
                    Logger.Write(this, LogLevel.Debug, "Resetting configuration element: \"{0}\".", modifiedElement.Id);
                    modifiedElement.Reset();
                }
                Profiles.Profile = profile;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Warn, "Failed to load configuration: {0}", e.Message);
            }
        }

        protected virtual IEnumerable<ConfigurationElement> Load(ConfigurationSection section, IEnumerable<KeyValuePair<string, string>> elements)
        {
            var restoredElements = new List<ConfigurationElement>();
            foreach (var element in elements)
            {
                if (!section.Contains(element.Key))
                {
                    //If config was created by a component that is no longer loaded then it will be lost here.
                    //TODO: Add the config but hide it so it's preserved but not displayed.
                    Logger.Write(this, LogLevel.Warn, "Configuration element \"{0}\" no longer exists.", element.Key);
                    continue;
                }
                Logger.Write(this, LogLevel.Debug, "Loading configuration element: \"{0}\".", element.Key);
                var existing = section.GetElement(element.Key);
                existing.SetPersistentValue(element.Value);
                restoredElements.Add(existing);
            }
            return restoredElements;
        }

        public void Save()
        {
            this.Save(this.Profile);
        }

        public void Save(string profile)
        {
            var fileName = Profiles.GetFileName(profile);
            this.Debouncer.Exec(() =>
            {
                Logger.Write(this, LogLevel.Debug, "Saving configuration to file \"{0}\".", fileName);
                try
                {
                    using (var stream = File.Create(fileName))
                    {
                        Serializer.Save(stream, this.Sections);
                    }
                    Profiles.Profile = profile;
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to save configuration: {0}", e.Message);
                }
            });
        }

        public void Delete()
        {
            this.Delete(this.Profile);
        }

        public void Delete(string profile)
        {
            var fileName = Profiles.GetFileName(profile);
            Profiles.Delete(profile);
            File.Delete(fileName);
            this.Load();
        }

        public void Wait()
        {
            this.Debouncer.Wait();
        }

        public void Reset()
        {
            foreach (var section in this.Sections)
            {
                section.Reset();
            }
        }

        public void ConnectDependencies()
        {
            foreach (var section in this.Sections)
            {
                foreach (var element in section.Elements)
                {
                    if (element.Dependencies == null)
                    {
                        continue;
                    }
                    this.ConnectDependencies(element);
                }
            }
        }

        protected virtual void ConnectDependencies(ConfigurationElement element)
        {
            var dependencies = element.Dependencies.ToDictionary(
                dependency => dependency,
                dependency => this.GetElement(dependency.SectionId, dependency.ElementId)
            );
            var handler = new EventHandler((sender, e) =>
            {
                if (dependencies.All(pair => pair.Key.Validate(pair.Value)))
                {
                    element.Show();
                }
                else
                {
                    element.Hide();
                }
            });
            foreach (var pair in dependencies)
            {
                pair.Key.AddHandler(pair.Value, handler);
            }
            handler(typeof(Configuration), EventArgs.Empty);
        }

        protected virtual IEnumerable<ConfigurationElement> GetModifiedElements()
        {
            var elements = new List<ConfigurationElement>();
            if (!string.IsNullOrEmpty(this.Profile))
            {
                foreach (var section in this.Sections)
                {
                    foreach (var element in section.Elements)
                    {
                        if (!element.IsModified)
                        {
                            continue;
                        }
                        elements.Add(element);
                    }
                }
            }
            return elements;
        }

        public ConfigurationSection GetSection(string sectionId)
        {
            return this.Sections.FirstOrDefault(section => string.Equals(section.Id, sectionId, StringComparison.OrdinalIgnoreCase));
        }

        public T GetElement<T>(string sectionId, string elementId) where T : ConfigurationElement
        {
            return this.GetElement(sectionId, elementId) as T;
        }

        public ConfigurationElement GetElement(string sectionId, string elementId)
        {
            var section = this.GetSection(sectionId);
            if (section == null)
            {
                return default(ConfigurationElement);
            }
            return section.GetElement(elementId);
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            if (this.Debouncer != null)
            {
                this.Debouncer.Dispose();
            }
        }

        ~Configuration()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
