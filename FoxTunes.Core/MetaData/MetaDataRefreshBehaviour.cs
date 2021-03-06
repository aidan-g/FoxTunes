﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class MetaDataRefreshBehaviour : StandardBehaviour, IDisposable
    {
        public MetaDataRefreshBehaviour()
        {
            this.Elements = new Dictionary<ConfigurationElement, string>();
        }

        public IDictionary<ConfigurationElement, string> Elements { get; private set; }

        public ILibraryHierarchyCache LibraryHierarchyCache { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyCache = core.Components.LibraryHierarchyCache;
            this.UserInterface = core.Components.UserInterface;
            this.LibraryManager = core.Managers.Library;
            this.Configuration = core.Components.Configuration;
            this.Monitor();
            this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                MetaDataBehaviourConfiguration.ENABLE_ELEMENT
            ).ConnectValue(value =>
            {
                if (value)
                {
                    this.Enable();
                }
                else
                {
                    this.Disable();
                }
            });
            base.InitializeComponent(core);
        }

        public void Enable()
        {
            this.Configuration.Saved += this.OnSaved;
            Logger.Write(this, LogLevel.Info, "Enabled.");
        }

        public void Disable()
        {
            if (this.Configuration != null)
            {
                this.Configuration.Saved -= this.OnSaved;
            }
            Logger.Write(this, LogLevel.Info, "Disabled.");
        }

        public void Monitor()
        {
            var section = this.Configuration.GetSection(MetaDataBehaviourConfiguration.SECTION);
            var elementIds = new[]
            {
                MetaDataBehaviourConfiguration.READ_EMBEDDED_IMAGES,
                MetaDataBehaviourConfiguration.READ_LOOSE_IMAGES,
                MetaDataBehaviourConfiguration.LOOSE_IMAGES_FRONT,
                MetaDataBehaviourConfiguration.LOOSE_IMAGES_BACK,
                MetaDataBehaviourConfiguration.IMAGES_PREFERENCE,
                MetaDataBehaviourConfiguration.MAX_IMAGE_SIZE,
                MetaDataBehaviourConfiguration.READ_EXTENDED_TAGS,
                MetaDataBehaviourConfiguration.READ_MUSICBRAINZ_TAGS,
                MetaDataBehaviourConfiguration.READ_LYRICS_TAGS,
                MetaDataBehaviourConfiguration.READ_REPLAY_GAIN_TAGS,
                MetaDataBehaviourConfiguration.READ_POPULARIMETER_TAGS,
                MetaDataBehaviourConfiguration.DETECT_COMPILATIONS
            };
            foreach (var elementId in elementIds)
            {
                this.Monitor(section.GetElement(elementId));
            }
        }

        public void Monitor(ConfigurationElement element)
        {
            this.Elements[element] = element.GetPersistentValue();
        }

        protected virtual void OnSaved(object sender, EventArgs e)
        {
            if (!this.LibraryHierarchyCache.HasItems)
            {
                //Library is empty.
                return;
            }
            var refresh = default(bool);
            foreach (var element in this.Elements.Keys.ToArray())
            {
                if (string.Equals(this.Elements[element], element.GetPersistentValue(), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                this.Monitor(element);
                refresh = true;
            }
            if (!refresh)
            {
                return;
            }
            Logger.Write(this, LogLevel.Info, "Configuration was changed, updating meta data.");
            this.Refresh();
        }

        public Task Refresh()
        {
            if (!this.UserInterface.Confirm(Strings.MetaDataRefreshBehaviour_Confirm))
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return this.LibraryManager.Rescan(true);
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
            this.Disable();
        }

        ~MetaDataRefreshBehaviour()
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
