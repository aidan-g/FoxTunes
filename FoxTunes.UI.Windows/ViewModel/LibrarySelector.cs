using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class LibrarySelector : ViewModelBase
    {
        private LibraryHierarchyCollection _Hierarchies { get; set; }

        public LibraryHierarchyCollection Hierarchies
        {
            get
            {
                return this._Hierarchies;
            }
            set
            {
                this._Hierarchies = value;
                this.OnHierarchiesChanged();
            }
        }

        protected virtual void OnHierarchiesChanged()
        {
            if (this.HierarchiesChanged != null)
            {
                this.HierarchiesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Hierarchies");
        }

        public event EventHandler HierarchiesChanged;

        private LibraryHierarchy _SelectedHierarchy { get; set; }

        public LibraryHierarchy SelectedHierarchy
        {
            get
            {
                return this._SelectedHierarchy;
            }
            set
            {
                if (this.LibraryManager != null)
                {
                    if (value != null)
                    {
                        this.LibraryManager.SelectedHierarchy = value.InnerLibraryHierarchy;
                    }
                    else
                    {
                        this.LibraryManager.SelectedHierarchy = global::FoxTunes.LibraryHierarchy.Empty;
                    }
                }
            }
        }

        protected virtual void OnSelectedHierarchyChanged()
        {
            if (this.SelectedHierarchyChanged != null)
            {
                this.SelectedHierarchyChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedHierarchy");
        }

        public event EventHandler SelectedHierarchyChanged;

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryHierarchyBrowser = this.Core.Components.LibraryHierarchyBrowser;
            this.LibraryManager = this.Core.Managers.Library;
            this.LibraryManager.SelectedHierarchyChanged += this.OnSelectedHierarchyChanged;
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual void OnSelectedHierarchyChanged(object sender, EventArgs e)
        {
            var task = this.RefreshSelectedHierarchy();
        }

        public virtual async Task Refresh()
        {
            await this.RefreshHierarchies().ConfigureAwait(false);
            await this.RefreshSelectedHierarchy().ConfigureAwait(false);
        }

        protected virtual Task RefreshHierarchies()
        {
            var hierarchies = this.LibraryHierarchyBrowser.GetHierarchies().Select(
                hiearchy => new LibraryHierarchy(hiearchy)
            ).ToArray();
            if (this.Hierarchies == null)
            {
                return Windows.Invoke(() => this.Hierarchies = new LibraryHierarchyCollection(hierarchies));
            }
            else
            {
                return Windows.Invoke(this.Hierarchies.Reset(hierarchies));
            }
        }

        protected virtual Task RefreshSelectedHierarchy()
        {
            if (this.LibraryManager.SelectedHierarchy == null)
            {
                this._SelectedHierarchy = LibraryHierarchy.Empty;
            }
            else
            {
                this._SelectedHierarchy = this.Hierarchies
                    .OfType<LibraryHierarchy>()
                    .FirstOrDefault(hiearchy => object.ReferenceEquals(hiearchy.InnerLibraryHierarchy, this.LibraryManager.SelectedHierarchy));
            }
            return Windows.Invoke(new Action(this.OnSelectedHierarchyChanged));
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.HierarchiesUpdated:
                    return this.Refresh();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LibrarySelector();
        }

        public class LibraryHierarchy : global::FoxTunes.LibraryHierarchy
        {
            public LibraryHierarchy(global::FoxTunes.LibraryHierarchy libraryHierarchy)
            {
                this.InnerLibraryHierarchy = libraryHierarchy;
                this.Id = libraryHierarchy.Id;
                this.Sequence = libraryHierarchy.Sequence;
                this.Name = libraryHierarchy.Name;
                this.Type = libraryHierarchy.Type;
                this.Enabled = libraryHierarchy.Enabled;
            }

            public global::FoxTunes.LibraryHierarchy InnerLibraryHierarchy { get; private set; }

            public override int GetHashCode()
            {
                var hashCode = base.GetHashCode();
                unchecked
                {
                    hashCode += this.Sequence.GetHashCode();
                    if (!string.IsNullOrEmpty(this.Name))
                    {
                        hashCode += this.Name.GetHashCode();
                    }
                    hashCode += this.Type.GetHashCode();
                    hashCode += this.Enabled.GetHashCode();
                }
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as LibraryHierarchy);
            }

            protected virtual bool Equals(LibraryHierarchy other)
            {
                if (!base.Equals(other))
                {
                    return false;
                }
                if (this.Sequence != other.Sequence)
                {
                    return false;
                }
                if (!string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if (this.Type != other.Type)
                {
                    return false;
                }
                if (this.Enabled != other.Enabled)
                {
                    return false;
                }
                return true;
            }

            new public static LibraryHierarchy Empty
            {
                get
                {
                    return new LibraryHierarchy(global::FoxTunes.LibraryHierarchy.Empty);
                }
            }
        }
    }
}
