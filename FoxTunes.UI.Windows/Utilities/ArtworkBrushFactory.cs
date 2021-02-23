using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    [Component("AA3CF53F-5358-4AD5-A3E5-0F19B1A1F8B5", ComponentSlots.None, priority: ComponentAttribute.PRIORITY_HIGH)]
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ArtworkBrushFactory : BaseComponent
    {
        public ArtworkBrushFactory()
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

        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public CappedDictionary<string, ImageBrush> Store { get; private set; }

        public TaskFactory Factory { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int PixelWidth { get; private set; }

        public int PixelHeight { get; private set; }

        public ResettableLazy<ImageBrush> FallbackValue { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
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
                case CommonSignals.MetaDataUpdated:
                    var names = signal.State as IEnumerable<string>;
                    return this.Reset(names);
                case CommonSignals.ImagesUpdated:
                    return this.Reset();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public AsyncResult<ImageBrush> Create(string fileName, int width, int height)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                return AsyncResult<ImageBrush>.FromValue(this.FallbackValue.Value);
            }
            var brush = default(ImageBrush);
            if (this.Store.TryGetValue(fileName, out brush))
            {
                return AsyncResult<ImageBrush>.FromValue(brush);
            }
            if (this.Width != width || this.Height != height)
            {
                this.CalculateTileSize(width, height);
            }
            return new AsyncResult<ImageBrush>(this.FallbackValue.Value, this.Factory.StartNew(() =>
            {
                return this.Store.GetOrAdd(
                    fileName,
                    () => this.Create(fileName, true)
                );
            }));
        }

        protected virtual ImageBrush Create(string fileName, bool cache)
        {
            var source = ImageLoader.Load(
                fileName,
                this.PixelWidth,
                this.PixelHeight,
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
            this.Store = new CappedDictionary<string, ImageBrush>(capacity, StringComparer.OrdinalIgnoreCase);
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

        protected virtual Task Reset(IEnumerable<string> names)
        {
            if (names != null && names.Any())
            {
                if (!names.Contains(CommonImageTypes.FrontCover, true))
                {
#if NET40
                    return TaskEx.FromResult(false);
#else
                    return Task.CompletedTask;
#endif
                }
            }
            return this.Reset();
        }

        protected virtual Task Reset()
        {
            return Windows.Invoke(() => this.CalculateTileSize(this.Width, this.Height));
        }
    }
}
