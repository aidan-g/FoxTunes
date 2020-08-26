using FoxTunes.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoxTunes
{
    public abstract class RendererBase : Freezable, IBaseComponent, INotifyPropertyChanged, IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly DependencyProperty CoreProperty = DependencyProperty.Register(
            "Core",
            typeof(ICore),
            typeof(RendererBase),
            new PropertyMetadata(new PropertyChangedCallback(OnCoreChanged))
        );

        public static ICore GetCore(RendererBase source)
        {
            return (ICore)source.GetValue(CoreProperty);
        }

        public static void SetCore(RendererBase source, ICore value)
        {
            source.SetValue(CoreProperty, value);
        }

        public static void OnCoreChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            if (renderer.Core != null && !renderer.IsInitialized)
            {
                renderer.InitializeComponent(renderer.Core);
            }
            renderer.OnCoreChanged();
        }


        public static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(1));

        public static readonly DependencyProperty BitmapProperty = DependencyProperty.Register(
            "Bitmap",
            typeof(WriteableBitmap),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnBitmapChanged))
        );

        public static WriteableBitmap GetBitmap(RendererBase source)
        {
            return (WriteableBitmap)source.GetValue(BitmapProperty);
        }

        public static void SetBitmap(RendererBase source, WriteableBitmap value)
        {
            source.SetValue(BitmapProperty, value);
        }

        public static void OnBitmapChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnBitmapChanged();
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            "Width",
            typeof(double),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnWidthChanged))
        );

        public static double GetWidth(RendererBase source)
        {
            return (double)source.GetValue(WidthProperty);
        }

        public static void SetWidth(RendererBase source, double value)
        {
            source.SetValue(WidthProperty, value);
        }

        public static void OnWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnWidthChanged();
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
           "Height",
           typeof(double),
           typeof(RendererBase),
           new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnHeightChanged))
       );

        public static double GetHeight(RendererBase source)
        {
            return (double)source.GetValue(HeightProperty);
        }

        public static void SetHeight(RendererBase source, double value)
        {
            source.SetValue(HeightProperty, value);
        }

        public static void OnHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnHeightChanged();
        }

        public static readonly DependencyProperty ViewboxProperty = DependencyProperty.Register(
           "Viewbox",
           typeof(Rect),
           typeof(RendererBase),
           new FrameworkPropertyMetadata(new Rect(0, 0, 1, 1), new PropertyChangedCallback(OnViewboxChanged))
       );

        public static Rect GetViewbox(RendererBase source)
        {
            return (Rect)source.GetValue(ViewboxProperty);
        }

        protected static void SetViewbox(RendererBase source, Rect value)
        {
            source.SetValue(ViewboxProperty, value);
        }

        public static void OnViewboxChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnViewboxChanged();
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(RendererBase),
            new FrameworkPropertyMetadata(Colors.Transparent, new PropertyChangedCallback(OnColorChanged))
        );

        public static Color GetColor(RendererBase source)
        {
            return (Color)source.GetValue(ColorProperty);
        }

        public static void SetColor(RendererBase source, Color value)
        {
            source.SetValue(ColorProperty, value);
        }

        public static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var renderer = sender as RendererBase;
            if (renderer == null)
            {
                return;
            }
            renderer.OnColorChanged();
        }


        public ICore Core
        {
            get
            {
                return this.GetValue(CoreProperty) as ICore;
            }
            set
            {
                this.SetValue(CoreProperty, value);
            }
        }

        protected virtual void OnCoreChanged()
        {
            if (this.CoreChanged != null)
            {
                this.CoreChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Core");
        }

        public event EventHandler CoreChanged;


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

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public bool IsInitialized { get; private set; }

        public virtual void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
               WindowsUserInterfaceConfiguration.SECTION,
               WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            this.IsInitialized = true;
        }

        protected virtual Task CreateBitmap()
        {
            return Windows.Invoke(() =>
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

               this.CreateViewBox();
           });
        }

        protected abstract void CreateViewBox();

        protected virtual void Dispatch(Action action)
        {
#if NET40
            var task = TaskEx.Run(action);
#else
            var task = Task.Run(action);
#endif
        }

        protected virtual void Dispatch(Func<Task> function)
        {
#if NET40
            var task = TaskEx.Run(function);
#else
            var task = Task.Run(function);
#endif
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (this.PropertyChanging == null)
            {
                return;
            }
            this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual Task OnError(string message, Exception exception)
        {
            Logger.Write(this, LogLevel.Error, message, exception);
            if (Error == null)
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.CompletedTask;
#endif
            }
            return Error(this, new ComponentErrorEventArgs(message, exception));
        }

        event ComponentErrorEventHandler IBaseComponent.Error
        {
            add
            {
                Error += value;
            }
            remove
            {
                Error -= value;
            }
        }

        public static event ComponentErrorEventHandler Error;

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
            //Nothing to do.
        }

        ~RendererBase()
        {
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
