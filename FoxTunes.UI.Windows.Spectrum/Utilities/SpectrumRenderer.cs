using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public class SpectrumRenderer : RendererBase
    {
        const int SCALE_FACTOR = 4;

        const int ROLLOFF_INTERVAL = 500;

        public readonly object SyncRoot = new object();

        public SpectrumRendererData RendererData { get; private set; }

        public bool IsStarted;

        public global::System.Timers.Timer Timer;

        public IOutput Output { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public BooleanConfigurationElement ShowPeaks { get; private set; }

        public BooleanConfigurationElement HighCut { get; private set; }

        public BooleanConfigurationElement Smooth { get; private set; }

        public IntegerConfigurationElement SmoothingFactor { get; private set; }

        public IntegerConfigurationElement HoldInterval { get; private set; }

        public IntegerConfigurationElement UpdateInterval { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public IntegerConfigurationElement Amplitude { get; private set; }

        public SpectrumRenderer()
        {
            this.Timer = new global::System.Timers.Timer();
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }

        public override void InitializeComponent(ICore core)
        {
            base.InitializeComponent(core);
            PlaybackStateNotifier.Notify += this.OnNotify;
            this.Output = core.Components.Output;
            this.Bars = this.Configuration.GetElement<SelectionConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
            this.ShowPeaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
             );
            this.HighCut = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.HIGH_CUT_ELEMENT
            );
            this.Smooth = this.Configuration.GetElement<BooleanConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.SMOOTH_ELEMENT
            );
            this.SmoothingFactor = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.SMOOTH_FACTOR_ELEMENT
            );
            this.HoldInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.HOLD_ELEMENT
            );
            this.UpdateInterval = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.INTERVAL_ELEMENT
            );
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            this.Amplitude = this.Configuration.GetElement<IntegerConfigurationElement>(
               SpectrumBehaviourConfiguration.SECTION,
               SpectrumBehaviourConfiguration.AMPLITUDE_ELEMENT
            );
            this.ScalingFactor.ValueChanged += this.OnValueChanged;
            this.Bars.ValueChanged += this.OnValueChanged;
            this.ShowPeaks.ValueChanged += this.OnValueChanged;
            this.HighCut.ValueChanged += this.OnValueChanged;
            this.Smooth.ValueChanged += this.OnValueChanged;
            this.SmoothingFactor.ValueChanged += this.OnValueChanged;
            this.HoldInterval.ValueChanged += this.OnValueChanged;
            this.UpdateInterval.ValueChanged += this.OnValueChanged;
            this.FFTSize.ValueChanged += this.OnValueChanged;
            this.Amplitude.ValueChanged += this.OnValueChanged;
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                await this.CreateBitmap().ConfigureAwait(false);
                if (PlaybackStateNotifier.IsPlaying)
                {
                    this.Start();
                }
            });
        }

        protected virtual void OnNotify(object sender, EventArgs e)
        {
            if (PlaybackStateNotifier.IsPlaying && !this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was started, starting renderer.");
                this.Start();
            }
            else if (!PlaybackStateNotifier.IsPlaying && this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Playback was stopped, stopping renderer.");
                this.Stop();
            }
        }

        protected virtual void OnValueChanged(object sender, EventArgs e)
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Interval = UpdateInterval.Value;
                }
            }
            if (object.ReferenceEquals(sender, this.Bars))
            {
                //Changing bars requires full refresh.
                var task = this.CreateBitmap();
            }
            else
            {
                var task = this.RefreshBitmap();
            }
        }

        protected override void CreateViewBox()
        {
            this.RendererData = Create(
                this.Output,
                this.Bitmap.PixelWidth,
                this.Bitmap.PixelHeight,
                SpectrumBehaviourConfiguration.GetBars(this.Bars.Value),
                SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                this.HoldInterval.Value,
                this.UpdateInterval.Value,
                this.SmoothingFactor.Value,
                this.Amplitude.Value,
                this.ShowPeaks.Value,
                this.HighCut.Value
            );
            this.Viewbox = new Rect(0, 0, this.GetPixelWidth(), this.Bitmap.PixelHeight);
        }

        protected virtual Task RefreshBitmap()
        {
            return Windows.Invoke(() =>
            {
                this.RendererData = Create(
                    this.Output,
                    this.Bitmap.PixelWidth,
                    this.Bitmap.PixelHeight,
                    SpectrumBehaviourConfiguration.GetBars(this.Bars.Value),
                    SpectrumBehaviourConfiguration.GetFFTSize(this.FFTSize.Value),
                    this.HoldInterval.Value,
                    this.UpdateInterval.Value,
                    this.SmoothingFactor.Value,
                    this.Amplitude.Value,
                    this.ShowPeaks.Value,
                    this.HighCut.Value
                );
            });
        }

        public void Start()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.IsStarted = true;
                    this.Timer.Interval = UpdateInterval.Value;
                    this.Timer.Start();
                }
            }
        }

        public void Stop()
        {
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.IsStarted = false;
                }
            }
        }

        protected virtual async Task Render()
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
                info = BitmapHelper.CreateRenderInfo(bitmap, this.Color);
            }).ConfigureAwait(false);

            if (!success)
            {
                //Failed to establish lock.
                this.Start();
                return;
            }

            Render(info, this.RendererData);

            await Windows.Invoke(() =>
            {
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();
                this.Start();
            }).ConfigureAwait(false);
        }

        protected virtual void Clear()
        {
            var elements = this.RendererData.Elements;
            var peaks = this.RendererData.Peaks;

            for (var a = 0; a < this.RendererData.Count; a++)
            {
                elements[a].X = a * this.RendererData.Step;
                elements[a].Y = this.RendererData.Height - 1;
                elements[a].Width = this.RendererData.Step;
                elements[a].Height = 1;
                if (this.RendererData.Peaks != null)
                {
                    peaks[a].X = a * this.RendererData.Step;
                    peaks[a].Y = this.RendererData.Height - 1;
                    peaks[a].Width = this.RendererData.Step;
                    peaks[a].Height = 1;
                }
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var length = this.Output.GetData(this.RendererData.Samples, this.RendererData.FFTSize, false);
                if (length <= 0)
                {
                    this.Clear();
                }
                else
                {
                    UpdateValues(this.RendererData);
                    if (this.Smooth.Value)
                    {
                        UpdateElementsSmooth(this.RendererData);
                    }
                    else
                    {
                        UpdateElementsFast(this.RendererData);
                    }
                    if (this.ShowPeaks.Value)
                    {
                        UpdatePeaks(this.RendererData);
                    }
                }
                var task = this.Render();
            }
            catch (Exception exception)
            {
                Logger.Write(this.GetType(), LogLevel.Warn, "Failed to update spectrum data, disabling: {0}", exception.Message);
            }
        }

        protected virtual double GetPixelWidth()
        {
            if (this.RendererData == null)
            {
                return 1;
            }
            return this.RendererData.Count * this.RendererData.Step;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SpectrumRenderer();
        }

        protected override void OnDisposing()
        {
            PlaybackStateNotifier.Notify -= this.OnNotify;
            lock (this.SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
            if (this.ScalingFactor != null)
            {
                this.ScalingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.Bars != null)
            {
                this.Bars.ValueChanged -= this.OnValueChanged;
            }
            if (this.ShowPeaks != null)
            {
                this.ShowPeaks.ValueChanged -= this.OnValueChanged;
            }
            if (this.HighCut != null)
            {
                this.HighCut.ValueChanged -= this.OnValueChanged;
            }
            if (this.Smooth != null)
            {
                this.Smooth.ValueChanged -= this.OnValueChanged;
            }
            if (this.SmoothingFactor != null)
            {
                this.SmoothingFactor.ValueChanged -= this.OnValueChanged;
            }
            if (this.HoldInterval != null)
            {
                this.HoldInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.UpdateInterval != null)
            {
                this.UpdateInterval.ValueChanged -= this.OnValueChanged;
            }
            if (this.FFTSize != null)
            {
                this.FFTSize.ValueChanged -= this.OnValueChanged;
            }
            if (this.Amplitude != null)
            {
                this.Amplitude.ValueChanged -= this.OnValueChanged;
            }
        }

        private static void Render(BitmapHelper.RenderInfo info, SpectrumRendererData rendererData)
        {
            var elements = rendererData.Elements;
            var peaks = rendererData.Peaks;

            BitmapHelper.Clear(info);

            for (var a = 0; a < rendererData.Count; a++)
            {
                BitmapHelper.DrawRectangle(
                    info,
                    elements[a].X,
                    elements[a].Y,
                    elements[a].Width,
                    elements[a].Height
                );
                if (peaks != null)
                {
                    if (peaks[a].Y >= elements[a].Y)
                    {
                        continue;
                    }
                    BitmapHelper.DrawRectangle(
                        info,
                        peaks[a].X,
                        peaks[a].Y,
                        peaks[a].Width,
                        peaks[a].Height
                    );
                }
            }
        }

        private static void UpdateValues(SpectrumRendererData data)
        {
            var samples = data.Samples;

            if (data.SamplesPerElement > 1)
            {
                for (int a = 0, b = 0; a < data.FFTRange; a += data.SamplesPerElement, b++)
                {
                    var value = 0.0f;
                    for (var c = 0; c < data.SamplesPerElement; c++)
                    {
                        var boost = (float)(1.0f + a * data.Amplitude);
                        value += (float)(Math.Sqrt(samples[a + c] * boost) * SCALE_FACTOR);
                    }
                    data.Values[b] = Math.Min(Math.Max(value / data.SamplesPerElement, 0), 1);
                }
            }
            else
            {
                //Not enough samples to fill the values, do the best we can.
                for (int a = 0; a < data.Count; a++)
                {
                    var boost = (float)(1.0f + a * data.Amplitude);
                    var value = (float)(Math.Sqrt(samples[a] * boost) * SCALE_FACTOR);
                    data.Values[a] = Math.Min(Math.Max(value, 0), 1);
                }
            }
        }

        private static void UpdateElementsFast(SpectrumRendererData data)
        {
            var values = data.Values;
            var elements = data.Elements;

            for (var a = 0; a < data.Count; a++)
            {
                var barHeight = Convert.ToInt32(values[a] * data.Height);
                elements[a].X = a * data.Step;
                elements[a].Width = data.Step;
                if (barHeight > 0)
                {
                    elements[a].Height = barHeight;
                }
                else
                {
                    elements[a].Height = 1;
                }
                elements[a].Y = data.Height - elements[a].Height;
            }
        }

        private static void UpdateElementsSmooth(SpectrumRendererData data)
        {
            var values = data.Values;
            var elements = data.Elements;

            var fast = (float)data.Height / data.Smoothing;
            for (var a = 0; a < data.Count; a++)
            {
                var barHeight = Convert.ToInt32(values[a] * data.Height);
                elements[a].X = a * data.Step;
                elements[a].Width = data.Step;
                if (barHeight > 0)
                {
                    var difference = Math.Abs(elements[a].Height - barHeight);
                    if (difference > 0)
                    {
                        if (difference < 2)
                        {
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height++;
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height--;
                            }
                        }
                        else
                        {
                            var distance = (float)difference / barHeight;
                            //TODO: We should use some kind of easing function.
                            //var increment = distance * distance * distance;
                            //var increment = 1 - Math.Pow(1 - distance, 5);
                            var increment = distance;
                            if (barHeight > elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Min(elements[a].Height + Math.Min(Math.Max(fast * increment, 1), difference), data.Height);
                            }
                            else if (barHeight < elements[a].Height)
                            {
                                elements[a].Height = (int)Math.Max(elements[a].Height - Math.Min(Math.Max(fast * increment, 1), difference), 1);
                            }
                        }
                    }
                }
                else
                {
                    elements[a].Height = 1;
                }
                elements[a].Y = data.Height - elements[a].Height;
            }
        }

        private static void UpdatePeaks(SpectrumRendererData data)
        {
            var duration = Convert.ToInt32(
                Math.Min(
                    (DateTime.UtcNow - data.LastUpdated).TotalMilliseconds,
                    data.UpdateInterval * 100
                )
            );

            var peaks = data.Peaks;
            var holds = data.Holds;
            var elements = data.Elements;

            var fast = data.Height / 4;
            for (int a = 0; a < data.Count; a++)
            {
                if (elements[a].Y < peaks[a].Y)
                {
                    peaks[a].X = a * data.Step;
                    peaks[a].Width = data.Step;
                    peaks[a].Height = 1;
                    peaks[a].Y = elements[a].Y;
                    holds[a] = data.HoldInterval + ROLLOFF_INTERVAL;
                }
                else if (elements[a].Y > peaks[a].Y && peaks[a].Y < data.Height - 1)
                {
                    if (holds[a] > 0)
                    {
                        if (holds[a] < data.HoldInterval)
                        {
                            var distance = 1 - ((float)holds[a] / data.HoldInterval);
                            var increment = fast * (distance * distance * distance);
                            if (peaks[a].Y < data.Height - increment)
                            {
                                peaks[a].Y += (int)Math.Round(increment);
                            }
                            else if (peaks[a].Y < data.Height - 1)
                            {
                                peaks[a].Y = data.Height - 1;
                            }
                        }
                        holds[a] -= duration;
                    }
                    else if (peaks[a].Y < data.Height - fast)
                    {
                        peaks[a].Y += fast;
                    }
                    else if (peaks[a].Y < data.Height - 1)
                    {
                        peaks[a].Y = data.Height - 1;
                    }
                }
            }

            data.LastUpdated = DateTime.UtcNow;
        }

        public static SpectrumRendererData Create(IOutput output, int width, int height, int count, int fftSize, int holdInterval, int updateInterval, int smoothingFactor, int amplitude, bool showPeaks, bool highCut)
        {
            var data = new SpectrumRendererData()
            {
                Width = width,
                Height = height,
                Count = count,
                FFTSize = fftSize,
                HoldInterval = holdInterval,
                UpdateInterval = updateInterval,
                Smoothing = smoothingFactor,
                Amplitude = (float)amplitude / 500,
                Samples = output.GetBuffer(fftSize),
                Values = new float[count],
                Elements = new Int32Rect[count]
            };
            if (showPeaks)
            {
                data.Peaks = new Int32Rect[count];
                data.Holds = new int[count];
            }
            if (highCut)
            {
                data.FFTRange = data.Samples.Length - (data.Samples.Length / 4);
            }
            else
            {
                data.FFTSize = data.Samples.Length;
            }
            data.SamplesPerElement = Math.Max(data.FFTRange / count, 1);
            data.Step = width / count;
            return data;
        }

        public class SpectrumRendererData
        {
            public int FFTSize;

            public int FFTRange;

            public float Amplitude;

            public float[] Samples;

            public float[] Values;

            public int SamplesPerElement;

            public int Step;

            public int Width;

            public int Height;

            public int Count;

            public Int32Rect[] Elements;

            public Int32Rect[] Peaks;

            public int[] Holds;

            public int UpdateInterval;

            public int HoldInterval;

            public int Smoothing;

            public DateTime LastUpdated;
        }
    }
}
