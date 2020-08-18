using FoxTunes.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class WaveFormRenderer : RendererBase
    {
        public static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register(
            "Bitmap",
            typeof(WriteableBitmap),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnBitmapChanged))
        );

        public static WriteableBitmap GetBitmap(WaveFormRenderer source)
        {
            return (WriteableBitmap)source.GetValue(BitmapProperty);
        }

        public static void SetBitmap(WaveFormRenderer source, WriteableBitmap value)
        {
            source.SetValue(BitmapProperty, value);
        }

        public static void OnBitmapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnBitmapChanged();
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnWidthChanged))
        );

        public static double GetWidth(WaveFormRenderer source)
        {
            return (double)source.GetValue(WidthProperty);
        }

        public static void SetWidth(WaveFormRenderer source, double value)
        {
            source.SetValue(WidthProperty, value);
        }

        public static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnWidthChanged();
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
           "Height",
           typeof(double),
           typeof(WaveFormRenderer),
           new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnHeightChanged))
       );

        public static double GetHeight(WaveFormRenderer source)
        {
            return (double)source.GetValue(HeightProperty);
        }

        public static void SetHeight(WaveFormRenderer source, double value)
        {
            source.SetValue(HeightProperty, value);
        }

        public static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnHeightChanged();
        }

        public static readonly DependencyProperty ViewboxProperty = DependencyProperty.Register(
           "Viewbox",
           typeof(Rect),
           typeof(WaveFormRenderer),
           new FrameworkPropertyMetadata(new Rect(0, 0, 1, 1), new PropertyChangedCallback(OnViewboxChanged))
       );

        public static Rect GetViewbox(WaveFormRenderer source)
        {
            return (Rect)source.GetValue(ViewboxProperty);
        }

        protected static void SetViewbox(WaveFormRenderer source, Rect value)
        {
            source.SetValue(ViewboxProperty, value);
        }

        public static void OnViewboxChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnViewboxChanged();
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(WaveFormRenderer),
            new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnColorChanged))
        );

        public static Color GetColor(WaveFormRenderer source)
        {
            return (Color)source.GetValue(ColorProperty);
        }

        public static void SetColor(WaveFormRenderer source, Color value)
        {
            source.SetValue(ColorProperty, value);
        }

        public static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as WaveFormRenderer;
            if (renderer == null)
            {
                return;
            }
            renderer.OnColorChanged();
        }

        public WaveFormGenerator.WaveFormGeneratorData GeneratorData { get; private set; }

        public WaveFormRendererData RendererData { get; private set; }

        public WaveFormGenerator Generator { get; private set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public SelectionConfigurationElement Mode { get; private set; }

        public IntegerConfigurationElement Resolution { get; private set; }

        public IntegerConfigurationElement Amplitude { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public WriteableBitmap Bitmap
        {
            get
            {
                return (WriteableBitmap)this.GetValue(BitmapProperty);
            }
            set
            {
                this.SetValue(BitmapProperty, value);
            }
        }

        protected virtual void OnBitmapChanged()
        {
            if (this.BitmapChanged != null)
            {
                this.BitmapChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Bitmap");
        }

        public event EventHandler BitmapChanged;

        public double Width
        {
            get
            {
                return (double)this.GetValue(WidthProperty);
            }
            set
            {
                this.SetValue(WidthProperty, value);
            }
        }

        protected virtual void OnWidthChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.WidthChanged != null)
            {
                this.WidthChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Width");
        }

        public event EventHandler WidthChanged;

        public double Height
        {
            get
            {
                return (double)this.GetValue(HeightProperty);
            }
            set
            {
                this.SetValue(HeightProperty, value);
            }
        }

        protected virtual void OnHeightChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.HeightChanged != null)
            {
                this.HeightChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Height");
        }

        public event EventHandler HeightChanged;

        public Rect Viewbox
        {
            get
            {
                return (Rect)this.GetValue(ViewboxProperty);
            }
            protected set
            {
                this.SetValue(ViewboxProperty, value);
            }
        }

        protected virtual void OnViewboxChanged()
        {
            if (this.ViewboxChanged != null)
            {
                this.ViewboxChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Viewbox");
        }

        public event EventHandler ViewboxChanged;

        public Color Color
        {
            get
            {
                return (Color)this.GetValue(ColorProperty);
            }
            set
            {
                this.SetValue(ColorProperty, value);
            }
        }

        protected virtual void OnColorChanged()
        {
            if (this.IsInitialized)
            {
                var task = this.CreateBitmap();
            }
            if (this.ColorChanged != null)
            {
                this.ColorChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Color");
        }

        public event EventHandler ColorChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Generator = ComponentRegistry.Instance.GetComponent<WaveFormGenerator>();
            this.PlaybackManager = core.Managers.Playback;
            this.PlaybackManager.CurrentStreamChanged += this.OnCurrentStreamChanged;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.Mode = this.Configuration.GetElement<SelectionConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.MODE_ELEMENT
            );
            this.Resolution = this.Configuration.GetElement<IntegerConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.RESOLUTION_ELEMENT
            );
            this.Amplitude = this.Configuration.GetElement<IntegerConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.AMPLITUDE_ELEMENT
            );
            this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                WaveBarBehaviourConfiguration.SECTION,
                WaveBarBehaviourConfiguration.RMS_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.Mode.ValueChanged += this.OnValueChanged;
            this.Resolution.ValueChanged += this.OnValueChanged;
            this.Amplitude.ValueChanged += this.OnValueChanged;
            this.Rms.ValueChanged += this.OnValueChanged;
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                await this.CreateBitmap().ConfigureAwait(false);
                if (this.PlaybackManager.CurrentStream != null)
                {
                    await this.Update(this.PlaybackManager.CurrentStream).ConfigureAwait(false);
                }
            });
            base.InitializeComponent(core);
        }

        protected virtual void OnCurrentStreamChanged(object sender, EventArgs e)
        {
            this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            if (object.ReferenceEquals(sender, this.Resolution))
            {
                //Changing resolution requires full refresh.
                this.Dispatch(() => this.Update(this.PlaybackManager.CurrentStream));
            }
            else
            {
                this.RefreshBitmap();
            }
        }

        protected virtual async Task Update(IOutputStream stream)
        {
            if (this.GeneratorData != null)
            {
                this.GeneratorData.Updated -= this.OnUpdated;
                if (this.GeneratorData.CancellationToken != null)
                {
                    this.GeneratorData.CancellationToken.Cancel();
                }
            }

            this.GeneratorData = null;
            this.RendererData = null;

            if (stream == null)
            {
                await this.Clear().ConfigureAwait(false);
                return;
            }

            this.GeneratorData = this.Generator.Generate(stream);
            this.GeneratorData.Updated += this.OnUpdated;

            await Windows.Invoke(() =>
            {
                this.RendererData = Create(this.GeneratorData, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight);
                this.Viewbox = new Rect(0, 0, this.GetActualWidth(), this.GetActualHeight());
            }).ConfigureAwait(false);

            this.Update();
        }

        protected virtual void OnUpdated(object sender, EventArgs e)
        {
            this.Update();
        }

        protected virtual async Task CreateBitmap()
        {
            await Windows.Invoke(() =>
            {
                var width = this.Width;
                var height = this.Height;
                if (width == 0 || double.IsNaN(width) || height == 0 || double.IsNaN(height))
                {
                    //We need proper dimentions.
                    return;
                }

                var size = Windows.ActiveWindow.GetElementPixelSize(
                    width * this.ScalingFactor.Value,
                    height * this.ScalingFactor.Value
                );

                this.Bitmap = new WriteableBitmap(
                    Convert.ToInt32(size.Width),
                    Convert.ToInt32(size.Height),
                    96,
                    96,
                    PixelFormats.Pbgra32,
                    null
                );

                if (this.GeneratorData != null)
                {
                    this.RendererData = Create(this.GeneratorData, this.Bitmap.PixelWidth, this.Bitmap.PixelHeight);
                    this.Viewbox = new Rect(0, 0, this.GetActualWidth(), this.GetActualHeight());
                }
            }).ConfigureAwait(false);

            this.Update();
        }

        protected virtual void RefreshBitmap()
        {
            if (this.RendererData != null)
            {
                this.RendererData.Position = 0;
            }

            this.Update();
        }

        public async Task Render()
        {
            const byte SHADE = 30;

            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var waveRenderInfo = default(BitmapHelper.RenderInfo);
            var powerRenderInfo = default(BitmapHelper.RenderInfo);

            if (this.RendererData == null)
            {
                return;
            }

            await Windows.Invoke(() =>
            {
                bitmap = this.Bitmap;
                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                if (this.Rms.Value)
                {
                    var colors = this.Color.ToPair(SHADE);
                    waveRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, colors[0]);
                    powerRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, colors[1]);
                }
                else
                {
                    waveRenderInfo = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
                }
            }).ConfigureAwait(false);

            if (!success)
            {
                //No bitmap or failed to establish lock.
                return;
            }

            Render(
                this.RendererData,
                waveRenderInfo,
                powerRenderInfo,
                this.Rms.Value,
                WaveBarBehaviourConfiguration.GetMode(this.Mode.Value)
            );

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }).ConfigureAwait(false);
        }

        public async Task Clear()
        {
            var bitmap = default(WriteableBitmap);
            var success = default(bool);
            var info = default(BitmapHelper.RenderInfo);

            await Windows.Invoke(() =>
            {
                bitmap = this.Bitmap;
                success = bitmap.TryLock(LockTimeout);
                if (!success)
                {
                    return;
                }
                info = BitmapHelper.CreateRenderInfo(bitmap, Colors.Transparent);
            }).ConfigureAwait(false);

            BitmapHelper.Clear(info);

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
            }).ConfigureAwait(false);
        }

        public void Update()
        {
            if (this.GeneratorData != null && this.RendererData != null)
            {
                Update(
                    this.GeneratorData,
                    this.RendererData,
                    this.Amplitude.Value,
                    this.Rms.Value,
                    WaveBarBehaviourConfiguration.GetMode(this.Mode.Value)
                );
            }
            var task = this.Render();
        }

        protected virtual double GetActualWidth()
        {
            if (this.GeneratorData == null || this.RendererData == null)
            {
                return 1;
            }
            return this.GeneratorData.Capacity / this.RendererData.ValuesPerElement;
        }

        protected virtual double GetActualHeight()
        {
            if (this.RendererData == null)
            {
                return 1;
            }
            return this.Height;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new WaveFormRenderer();
        }

        protected override void OnDisposing()
        {
            if (this.PlaybackManager != null)
            {
                this.PlaybackManager.CurrentStreamChanged -= this.OnCurrentStreamChanged;
            }
            if (this.GeneratorData != null)
            {
                this.GeneratorData.Updated -= this.OnUpdated;
            }
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.Mode != null)
            {
                this.Mode.ValueChanged -= this.OnValueChanged;
            }
            if (this.Resolution != null)
            {
                this.Resolution.ValueChanged += this.OnValueChanged;
            }
            if (this.Amplitude != null)
            {
                this.Amplitude.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Update(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude, bool rms, WaveFormRendererMode mode)
        {
            if (generatorData.Peak == 0)
            {
                return;
            }
            else if (generatorData.Peak != rendererData.Peak)
            {
                rendererData.Position = 0;
                rendererData.Peak = generatorData.Peak;
            }

            switch (mode)
            {
                case WaveFormRendererMode.Mono:
                    UpdateMono(generatorData, rendererData, amplitude, rms);
                    break;
                case WaveFormRendererMode.Seperate:
                    UpdateSeperate(generatorData, rendererData, amplitude, rms);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void UpdateMono(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude, bool rms)
        {
            var center = rendererData.Height / 2.0f;
            var factor = (rendererData.Peak / 2.0f) * (10.0f - amplitude);

            var data = generatorData.Data;
            var waveElements = rendererData.WaveElements;
            var powerElements = rendererData.PowerElements;
            var valuesPerElement = rendererData.ValuesPerElement;

            while (rendererData.Position < rendererData.Capacity)
            {
                var valuePosition = rendererData.Position * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    if (generatorData.Position <= generatorData.Capacity)
                    {
                        break;
                    }
                    else
                    {
                        valuesPerElement = generatorData.Capacity - valuePosition;
                    }
                }

                var x = rendererData.Position;

                {

                    var y = default(int);
                    var width = 1;
                    var height = default(int);

                    var topValue = default(float);
                    var bottomValue = default(float);
                    for (var a = 0; a < valuesPerElement; a++)
                    {
                        for (var b = 0; b < generatorData.Channels; b++)
                        {
                            topValue += Math.Abs(data[valuePosition + a, b].Min);
                            bottomValue += Math.Abs(data[valuePosition + a, b].Max);
                        }
                    }
                    topValue /= (valuesPerElement * generatorData.Channels);
                    bottomValue /= (valuesPerElement * generatorData.Channels);

                    topValue /= factor;
                    bottomValue /= factor;

                    topValue = Math.Min(topValue, 1);
                    bottomValue = Math.Min(bottomValue, 1);

                    y = Convert.ToInt32(center - (topValue * center));
                    height = Convert.ToInt32((center - y) + (bottomValue * center));

                    waveElements[rendererData.Position, 0].X = x;
                    waveElements[rendererData.Position, 0].Y = y;
                    waveElements[rendererData.Position, 0].Width = width;
                    waveElements[rendererData.Position, 0].Height = height;

                }

                if (rms)
                {

                    var y = default(int);
                    var width = 1;
                    var height = default(int);

                    var value = default(float);
                    for (var a = 0; a < valuesPerElement; a++)
                    {
                        for (var b = 0; b < generatorData.Channels; b++)
                        {
                            value += Math.Abs(data[valuePosition + a, b].Rms);
                        }
                    }
                    value /= (valuesPerElement * generatorData.Channels);

                    value /= factor;

                    y = Convert.ToInt32(center - (value * center));
                    height = Convert.ToInt32((center - y) + (value * center));

                    powerElements[rendererData.Position, 0].X = x;
                    powerElements[rendererData.Position, 0].Y = y;
                    powerElements[rendererData.Position, 0].Width = width;
                    powerElements[rendererData.Position, 0].Height = height;

                }

                rendererData.Position++;
            }
        }

        private static void UpdateSeperate(WaveFormGenerator.WaveFormGeneratorData generatorData, WaveFormRendererData rendererData, int amplitude, bool rms)
        {
            var factor = rendererData.Peak / (generatorData.Channels * 2) * (10.0f - amplitude);

            var data = generatorData.Data;
            var waveElements = rendererData.WaveElements;
            var powerElements = rendererData.PowerElements;
            var valuesPerElement = rendererData.ValuesPerElement;

            while (rendererData.Position < rendererData.Capacity)
            {
                var valuePosition = rendererData.Position * rendererData.ValuesPerElement;
                if ((valuePosition + rendererData.ValuesPerElement) > generatorData.Position)
                {
                    if (generatorData.Position <= generatorData.Capacity)
                    {
                        break;
                    }
                    else
                    {
                        valuesPerElement = generatorData.Capacity - valuePosition;
                    }
                }

                var x = rendererData.Position;
                var waveHeight = rendererData.Height / generatorData.Channels;

                for (var channel = 0; channel < generatorData.Channels; channel++)
                {
                    var waveCenter = (waveHeight * channel) + (waveHeight / 2);

                    {

                        var y = default(int);
                        var width = 1;
                        var height = default(int);

                        var topValue = default(float);
                        var bottomValue = default(float);
                        for (var a = 0; a < valuesPerElement; a++)
                        {
                            topValue += Math.Abs(data[valuePosition + a, channel].Min);
                            bottomValue += Math.Abs(data[valuePosition + a, channel].Max);
                        }
                        topValue /= (valuesPerElement * generatorData.Channels);
                        bottomValue /= (valuesPerElement * generatorData.Channels);

                        topValue /= factor;
                        bottomValue /= factor;

                        topValue = Math.Min(topValue, 1);
                        bottomValue = Math.Min(bottomValue, 1);

                        y = Convert.ToInt32(waveCenter - (topValue * (waveHeight / 2)));
                        height = Convert.ToInt32((waveCenter - y) + (bottomValue * (waveHeight / 2)));

                        waveElements[rendererData.Position, channel].X = x;
                        waveElements[rendererData.Position, channel].Y = y;
                        waveElements[rendererData.Position, channel].Width = width;
                        waveElements[rendererData.Position, channel].Height = height;

                    }

                    if (rms)
                    {

                        var y = default(int);
                        var width = 1;
                        var height = default(int);

                        var value = default(float);
                        for (var a = 0; a < valuesPerElement; a++)
                        {
                            value += Math.Abs(data[valuePosition + a, channel].Rms);
                        }
                        value /= (valuesPerElement * generatorData.Channels);

                        value /= factor;

                        y = Convert.ToInt32(waveCenter - (value * (waveHeight / 2)));
                        height = Convert.ToInt32((waveCenter - y) + (value * (waveHeight / 2)));

                        powerElements[rendererData.Position, channel].X = x;
                        powerElements[rendererData.Position, channel].Y = y;
                        powerElements[rendererData.Position, channel].Width = width;
                        powerElements[rendererData.Position, channel].Height = height;

                    }
                }

                rendererData.Position++;
            }
        }

        public static void Render(WaveFormRendererData rendererData, BitmapHelper.RenderInfo waveRenderInfo, BitmapHelper.RenderInfo powerRenderInfo, bool rms, WaveFormRendererMode mode)
        {
            BitmapHelper.Clear(waveRenderInfo);
            switch (mode)
            {
                case WaveFormRendererMode.Mono:
                    RenderMono(rendererData, waveRenderInfo, powerRenderInfo, rms);
                    break;
                case WaveFormRendererMode.Seperate:
                    RenderSeperate(rendererData, waveRenderInfo, powerRenderInfo, rms);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void RenderMono(WaveFormRendererData rendererData, BitmapHelper.RenderInfo waveRenderInfo, BitmapHelper.RenderInfo powerRenderInfo, bool rms)
        {
            if (rms)
            {
                var waveElements = rendererData.WaveElements;
                var powerElements = rendererData.PowerElements;
                for (var position = 0; position < rendererData.Position; position++)
                {
                    var waveElement = waveElements[position, 0];
                    var powerElement = powerElements[position, 0];
                    BitmapHelper.DrawRectangle(
                        waveRenderInfo,
                        waveElement.X,
                        waveElement.Y,
                        waveElement.Width,
                        waveElement.Height
                    );
                    BitmapHelper.DrawRectangle(
                        powerRenderInfo,
                        powerElement.X,
                        powerElement.Y,
                        powerElement.Width,
                        powerElement.Height
                    );
                }
            }
            else
            {
                var elements = rendererData.WaveElements;
                for (var position = 0; position < rendererData.Position; position++)
                {
                    var element = elements[position, 0];
                    BitmapHelper.DrawRectangle(
                        waveRenderInfo,
                        element.X,
                        element.Y,
                        element.Width,
                        element.Height
                    );
                }
            }
        }

        public static void RenderSeperate(WaveFormRendererData rendererData, BitmapHelper.RenderInfo waveRenderInfo, BitmapHelper.RenderInfo powerRenderInfo, bool rms)
        {
            if (rms)
            {
                var waveElements = rendererData.WaveElements;
                var powerElements = rendererData.PowerElements;
                for (var position = 0; position < rendererData.Position; position++)
                {
                    for (var channel = 0; channel < rendererData.Channels; channel++)
                    {
                        var waveElement = waveElements[position, channel];
                        var powerElement = powerElements[position, channel];
                        BitmapHelper.DrawRectangle(
                            waveRenderInfo,
                            waveElement.X,
                            waveElement.Y,
                            waveElement.Width,
                            waveElement.Height
                        );
                        BitmapHelper.DrawRectangle(
                            powerRenderInfo,
                            powerElement.X,
                            powerElement.Y,
                            powerElement.Width,
                            powerElement.Height
                        );
                    }
                }
            }
            else
            {
                var elements = rendererData.WaveElements;
                for (var position = 0; position < rendererData.Position; position++)
                {
                    for (var channel = 0; channel < rendererData.Channels; channel++)
                    {
                        var element = elements[position, channel];
                        BitmapHelper.DrawRectangle(
                            waveRenderInfo,
                            element.X,
                            element.Y,
                            element.Width,
                            element.Height
                        );
                    }
                }
            }
        }

        public static WaveFormRendererData Create(WaveFormGenerator.WaveFormGeneratorData generatorData, int width, int height)
        {
            var valuesPerElement = Convert.ToInt32(
                Math.Ceiling(
                    Math.Max(
                        (float)generatorData.Capacity / width,
                        1
                    )
                )
            );
            return new WaveFormRendererData()
            {
                Width = width,
                Height = height,
                ValuesPerElement = valuesPerElement,
                WaveElements = new Int32Rect[width, generatorData.Channels],
                PowerElements = new Int32Rect[width, generatorData.Channels],
                Channels = generatorData.Channels,
                Position = 0,
                Capacity = width,
                Peak = 0
            };
        }

        public class WaveFormRendererData
        {
            public int Width;

            public int Height;

            public int ValuesPerElement;

            public Int32Rect[,] WaveElements;

            public Int32Rect[,] PowerElements;

            public int Channels;

            public int Position;

            public int Capacity;

            public float Peak;
        }
    }

    public enum WaveFormRendererMode : byte
    {
        None,
        Mono,
        Seperate
    }
}
