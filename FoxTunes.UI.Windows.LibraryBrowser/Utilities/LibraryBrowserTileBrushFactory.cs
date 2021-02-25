using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    [Component("8080D985-D642-4189-901B-A84530A1F110", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class LibraryBrowserTileBrushFactory : StandardFactory
    {
        public LibraryBrowserTileBrushFactory()
        {
            this.FallbackValue = new ResettableLazy<ImageBrush>(() =>
            {
                var source = ImageLoader.Load(
                    this.ThemeLoader.Theme.Id,
                    this.ThemeLoader.Theme.GetArtworkPlaceholder,
                    this.PixelWidth,
                    this.PixelHeight,
                    true
                );
                var brush = new ImageBrush(source)
                {
                    Stretch = Stretch.Uniform
                };
                brush.Freeze();
                return brush;
            });
        }

        public LibraryBrowserTileProvider LibraryBrowserTileProvider { get; private set; }

        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public IntegerConfigurationElement TileSize { get; private set; }

        public TaskFactory Factory { get; private set; }

        public CappedDictionary<LibraryHierarchyNode, ImageBrush> Store { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int PixelWidth { get; private set; }

        public int PixelHeight { get; private set; }

        public ResettableLazy<ImageBrush> FallbackValue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryBrowserTileProvider = ComponentRegistry.Instance.GetComponent<LibraryBrowserTileProvider>();
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.TileSize = this.Configuration.GetElement<IntegerConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                LibraryBrowserBehaviourConfiguration.LIBRARY_BROWSER_TILE_SIZE
            );
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.THREADS
            ).ConnectValue(value => this.CreateTaskFactory(value));
            this.Configuration.GetElement<IntegerConfigurationElement>(
                ImageBehaviourConfiguration.SECTION,
                ImageLoaderConfiguration.CACHE_SIZE
            ).ConnectValue(value => this.CreateCache(value));
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.ImagesUpdated:
                    return this.Reset();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public AsyncResult<ImageBrush> Create(LibraryHierarchyNode libraryHierarchyNode)
        {
            if (libraryHierarchyNode == null)
            {
                return AsyncResult<ImageBrush>.FromValue(this.FallbackValue.Value);
            }
            var cache = string.IsNullOrEmpty(this.LibraryHierarchyBrowser.Filter);
            if (cache)
            {
                var brush = default(ImageBrush);
                if (this.Store.TryGetValue(libraryHierarchyNode, out brush))
                {
                    return AsyncResult<ImageBrush>.FromValue(brush);
                }
            }
            if (this.Width != this.TileSize.Value || this.Height != this.TileSize.Value)
            {
                this.CalculateTileSize(this.TileSize.Value, this.TileSize.Value);
            }
            return new AsyncResult<ImageBrush>(this.FallbackValue.Value, this.Factory.StartNew(() =>
            {
                if (cache)
                {
                    return this.Store.GetOrAdd(
                        libraryHierarchyNode,
                        () => this.Create(libraryHierarchyNode, true)
                    );
                }
                else
                {
                    return this.Create(libraryHierarchyNode, false);
                }
            }));
        }

        protected virtual ImageBrush Create(LibraryHierarchyNode libraryHierarchyNode, bool cache)
        {
            var source = this.LibraryBrowserTileProvider.CreateImageSource(
                libraryHierarchyNode,
                this.PixelWidth,
                this.PixelWidth,
                cache
            );
            if (source == null)
            {
                return this.FallbackValue.Value;
            }
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            return brush;
        }

        protected virtual void CreateTaskFactory(int threads)
        {
            this.Factory = new TaskFactory(new TaskScheduler(new ParallelOptions()
            {
                MaxDegreeOfParallelism = threads
            }));
        }

        protected virtual void CreateCache(int capacity)
        {
            this.Store = new CappedDictionary<LibraryHierarchyNode, ImageBrush>(capacity);
        }

        protected virtual void CalculateTileSize(int width, int height)
        {
            if (this.Store != null)
            {
                this.Store.Clear();
            }
            if (this.FallbackValue != null)
            {
                this.FallbackValue.Reset();
            }
            if (width == 0 || height == 0)
            {
                this.Width = 0;
                this.Height = 0;
                this.PixelWidth = 0;
                this.PixelHeight = 0;
                return;
            }
            var size = Windows.ActiveWindow.GetElementPixelSize(
                width * this.ScalingFactor.Value,
                height * this.ScalingFactor.Value
            );
            this.Width = width;
            this.Height = height;
            this.PixelWidth = global::System.Convert.ToInt32(size.Width);
            this.PixelHeight = global::System.Convert.ToInt32(size.Height);
        }

        protected virtual Task Reset()
        {
            return Windows.Invoke(() => this.CalculateTileSize(this.Width, this.Height));
        }

        protected override void OnDisposing()
        {
            if (this.SignalEmitter != null)
            {
                this.SignalEmitter.Signal -= this.OnSignal;
            }
            base.OnDisposing();
        }
    }
}
