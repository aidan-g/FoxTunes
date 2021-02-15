using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class PlaylistGridViewColumnFactory : StandardComponent, IDisposable
    {
        public PlaylistColumnProviderManager PlaylistColumnProviderManager { get; private set; }

        public IScriptingRuntime ScriptingRuntime { get; private set; }

        public Lazy<IScriptingContext> ScriptingContext { get; private set; }

        public bool Suspended { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistColumnProviderManager = ComponentRegistry.Instance.GetComponent<PlaylistColumnProviderManager>();
            this.ScriptingRuntime = core.Components.ScriptingRuntime;
            this.ScriptingContext = new Lazy<IScriptingContext>(this.ScriptingRuntime.CreateContext);
            base.InitializeComponent(core);
        }

        public PlaylistGridViewColumn Create(PlaylistColumn column)
        {
            var gridViewColumn = new PlaylistGridViewColumn(column);
            switch (column.Type)
            {
                case PlaylistColumnType.Script:
                    if (!string.IsNullOrEmpty(column.Script))
                    {
                        gridViewColumn.DisplayMemberBinding = new PlaylistScriptBinding()
                        {
                            ScriptingContext = this.ScriptingContext.Value,
                            Script = column.Script
                        };
                    }
                    break;
                case PlaylistColumnType.Plugin:
                    if (!string.IsNullOrEmpty(column.Plugin))
                    {
                        var provider = PlaylistColumnProviderManager.GetProvider(column.Plugin);
                        if (provider != null)
                        {
                            gridViewColumn.CellTemplate = provider.CellTemplate;
                        }
                    }
                    break;
                case PlaylistColumnType.Tag:
                    if (!string.IsNullOrEmpty(column.Tag))
                    {
                        gridViewColumn.DisplayMemberBinding = new PlaylistMetaDataBinding()
                        {
                            Name = column.Tag,
                            Format = column.Format
                        };
                    }
                    break;
            }
            BindingHelper.Create(
                gridViewColumn,
                GridViewColumn.WidthProperty,
                typeof(GridViewColumn),
                column,
                "Width",
                ColumnWidthConverter.Instance,
                (sender, e) => this.OnWidthChanged(column)
            );
            BindingHelper.Create(
                gridViewColumn,
                GridViewColumnExtensions.PositionProperty,
                typeof(GridViewColumn),
                column,
                "Sequence",
                (sender, e) => this.OnPositionChanged(column)
            );
            return gridViewColumn;
        }

        protected virtual void OnWidthChanged(PlaylistColumn column)
        {
            if (this.Suspended || this.WidthChanged == null)
            {
                return;
            }
            this.WidthChanged(this, column);
        }

        public event PlaylistColumnEventHandler WidthChanged;

        protected virtual void OnPositionChanged(PlaylistColumn column)
        {
            if (this.Suspended || this.PositionChanged == null)
            {
                return;
            }
            this.PositionChanged(this, column);
        }

        public event PlaylistColumnEventHandler PositionChanged;

        public void Refresh(PlaylistGridViewColumn column)
        {
            column.Refresh();
        }

        public bool ShouldRefreshColumn(PlaylistGridViewColumn column, IEnumerable<string> names)
        {
            if (names == null || !names.Any())
            {
                return true;
            }
            switch (column.PlaylistColumn.Type)
            {
                case PlaylistColumnType.Script:
                    return names.Any(name => column.PlaylistColumn.Script.Contains(name, true));
                case PlaylistColumnType.Plugin:
                    var provider = PlaylistColumnProviderManager.GetProvider(column.PlaylistColumn.Plugin);
                    if (provider != null)
                    {
                        return names.Any(name => provider.MetaData.Contains(name, true));
                    }
                    break;
            }
            return true;
        }

        public void Resize(GridViewColumn column)
        {
            this.Suspended = true;
            try
            {
                if (double.IsNaN(column.Width))
                {
                    column.Width = column.ActualWidth;
                    column.Width = double.NaN;
                }
            }
            finally
            {
                this.Suspended = false;
            }
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
            if (this.ScriptingContext != null && this.ScriptingContext.IsValueCreated)
            {
                this.ScriptingContext.Value.Dispose();
            }
        }

        ~PlaylistGridViewColumnFactory()
        {
            Logger.Write(this.GetType(), LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
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

    public delegate void PlaylistColumnEventHandler(object sender, PlaylistColumn playlistColumn);
}
