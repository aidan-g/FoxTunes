﻿using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public class Core : BaseComponent, ICore
    {
        public static bool IsShuttingDown { get; set; }

        private Core()
        {
            ComponentRegistry.Instance.Clear();
            IsShuttingDown = false;
        }

        public Core(ICoreSetup setup) : this()
        {
            this.Setup = setup;
            foreach (var slot in ComponentSlots.All)
            {
                if (!this.Setup.HasSlot(slot))
                {
                    ComponentResolver.Slots[slot] = ComponentSlots.Blocked;
                }
            }
        }

        public ICoreSetup Setup { get; private set; }

        public IStandardComponents Components
        {
            get
            {
                return StandardComponents.Instance;
            }
        }

        public IStandardManagers Managers
        {
            get
            {
                return StandardManagers.Instance;
            }
        }

        public IStandardFactories Factories
        {
            get
            {
                return StandardFactories.Instance;
            }
        }

        public void Load()
        {
            this.Load(Enumerable.Empty<IBaseComponent>());
        }

        public void Load(IEnumerable<IBaseComponent> components)
        {
            try
            {
                this.LoadComponents();
                this.LoadFactories();
                this.LoadManagers();
                this.LoadBehaviours();
                ComponentRegistry.Instance.AddComponents(components);
                this.LoadConfiguration();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Debug, "Failed to load the core, we will crash soon: {0}", e.Message);
                throw;
            }
        }

        public void Initialize()
        {
            try
            {
                this.InitializeComponents();
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Debug, "Failed to initialize the core, we will crash soon: {0}", e.Message);
                throw;
            }
        }

        protected virtual void LoadComponents()
        {
            ComponentRegistry.Instance.AddComponents(ComponentLoader.Instance.Load(this));
        }

        protected virtual void LoadManagers()
        {
            ComponentRegistry.Instance.AddComponents(ManagerLoader.Instance.Load(this));
        }

        protected virtual void LoadFactories()
        {
            ComponentRegistry.Instance.AddComponents(FactoryLoader.Instance.Load(this));
        }

        protected virtual void LoadBehaviours()
        {
            ComponentRegistry.Instance.AddComponents(BehaviourLoader.Instance.Load(this));
        }

        protected virtual void LoadConfiguration()
        {
            ComponentRegistry.Instance.ForEach<IConfigurableComponent>(component =>
            {
                Logger.Write(this, LogLevel.Debug, "Registering configuration for component {0}.", component.GetType().Name);
                try
                {
                    var sections = component.GetConfigurationSections();
                    foreach (var section in sections)
                    {
                        this.Components.Configuration.WithSection(section);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to register configuration for component {0}: {1}", component.GetType().Name, e.Message);
                }
            });
            this.Components.Configuration.Load();
            this.Components.Configuration.ConnectDependencies();
        }

        protected virtual void InitializeComponents()
        {
            ComponentRegistry.Instance.ForEach(component =>
            {
                try
                {
                    component.InitializeComponent(this);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to initialize component {0}: {1}", component.GetType().Name, e.Message);
                }
            });
        }

        public void InitializeDatabase(IDatabaseComponent database, DatabaseInitializeType type)
        {
            ComponentRegistry.Instance.ForEach<IDatabaseInitializer>(component =>
            {
                try
                {
                    component.InitializeDatabase(database, type);
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to initialize database {0}: {1}", component.GetType().Name, e.Message);
                }
            });
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
            ComponentRegistry.Instance.ForEach<IDisposable>(component =>
            {
                if (object.ReferenceEquals(this, component))
                {
                    return;
                }
                try
                {
                    component.Dispose();
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Warn, "Failed to dispose component {0}: {1}", component.GetType().Name, e.Message);
                }
            });
        }

        ~Core()
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
