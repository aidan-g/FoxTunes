using FoxTunes.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class BackgroundTask : BaseComponent, IBackgroundTask
    {
        static BackgroundTask()
        {
            Instances = new List<WeakReference<IBackgroundTask>>();
            Semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        private static IList<WeakReference<IBackgroundTask>> Instances { get; set; }

        private static ConcurrentDictionary<string, SemaphoreSlim> Semaphores { get; set; }

        public static IEnumerable<IBackgroundTask> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .Where(backgroundTask => !(backgroundTask.IsCompleted || backgroundTask.IsFaulted))
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(BackgroundTask sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        protected BackgroundTask(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public virtual bool Visible
        {
            get
            {
                return false;
            }
        }

        public virtual bool Cancellable
        {
            get
            {
                return false;
            }
        }

        public bool IsCancellationRequested { get; protected set; }

        protected virtual void OnCancellationRequested()
        {
            if (this.CancellationRequested == null)
            {
                return;
            }
            this.CancellationRequested(this, EventArgs.Empty);
        }

        public event EventHandler CancellationRequested;

        public virtual int Concurrency
        {
            get
            {
                return 1;
            }
        }

        public SemaphoreSlim Semaphore
        {
            get
            {
                return Semaphores.GetOrAdd(this.Id, key => new SemaphoreSlim(this.Concurrency, this.Concurrency));
            }
        }

        private string _Name { get; set; }

        public string Name
        {
            get
            {
                return this._Name;
            }
            protected set
            {
                this._Name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
            if (this.NameChanged != null)
            {
                this.NameChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Name");
        }

        public event EventHandler NameChanged;

        private string _Description { get; set; }

        public string Description
        {
            get
            {
                return this._Description;
            }
            protected set
            {
                this._Description = value;
                this.OnDescriptionChanged();
            }
        }

        protected virtual void OnDescriptionChanged()
        {
            if (this.DescriptionChanged != null)
            {
                this.DescriptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Description");
        }

        public event EventHandler DescriptionChanged;

        private int _Position { get; set; }

        public int Position
        {
            get
            {
                return this._Position;
            }
            protected set
            {
                this._Position = value;
                this.OnPositionChanged();
            }
        }

        protected virtual void OnPositionChanged()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Position");
        }

        public event EventHandler PositionChanged;

        private int _Count { get; set; }

        public int Count
        {
            get
            {
                return this._Count;
            }
            protected set
            {
                this._Count = value;
                this.OnCountChanged();
            }
        }

        protected virtual void OnCountChanged()
        {
            if (this.CountChanged != null)
            {
                this.CountChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Count");
        }

        public event EventHandler CountChanged;

        public override void InitializeComponent(ICore core)
        {
            lock (Instances)
            {
                Instances.Add(new WeakReference<IBackgroundTask>(this));
            }
            OnActiveChanged(this);
            base.InitializeComponent(core);
        }

        public virtual async Task Run()
        {
            Logger.Write(this, LogLevel.Debug, "Running background task.");
            await this.OnStarted().ConfigureAwait(false);
#if NET40
            Semaphore.Wait();
#else
            await Semaphore.WaitAsync().ConfigureAwait(false);
#endif
            try
            {
                try
                {
                    await this.OnRun().ConfigureAwait(false);
                }
                finally
                {
                    this.Semaphore.Release();
                }
                Logger.Write(this, LogLevel.Debug, "Background task succeeded.");
                await this.OnCompleted().ConfigureAwait(false);
                return;
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    Logger.Write(this, LogLevel.Error, "Background task failed: {0}", innerException.Message);
                }
                this.Exception = e;
            }
            catch (Exception e)
            {
                Logger.Write(this, LogLevel.Error, "Background task failed: {0}", e.Message);
                this.Exception = e;
            }
            await this.OnFaulted().ConfigureAwait(false);
        }

        protected abstract Task OnRun();

        protected virtual Task OnStarted()
        {
            this.IsStarted = true;
            if (this.Started != null)
            {
                this.Started(this, EventArgs.Empty);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public event EventHandler Started;

        public bool IsStarted { get; private set; }

        protected virtual Task OnCompleted()
        {
            this.IsCompleted = true;
            if (this.Completed != null)
            {
                this.Completed(this, EventArgs.Empty);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public event EventHandler Completed;

        public bool IsCompleted { get; private set; }

        private Exception _Exception { get; set; }

        public Exception Exception
        {
            get
            {
                return this._Exception;
            }
            protected set
            {
                this._Exception = value;
                this.OnExceptionChanged();
            }
        }

        protected virtual void OnExceptionChanged()
        {
            if (this.ExceptionChanged != null)
            {
                this.ExceptionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Exception");
        }

        public event EventHandler ExceptionChanged;

        protected virtual Task OnFaulted()
        {
            this.IsFaulted = true;
            if (this.Faulted != null)
            {
                this.Faulted(this, EventArgs.Empty);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public event EventHandler Faulted;

        public bool IsFaulted { get; private set; }

        public virtual void Cancel()
        {
            if (!this.Cancellable)
            {
                throw new NotImplementedException();
            }
            this.IsCancellationRequested = true;
            this.OnCancellationRequested();
        }

        protected virtual async Task WithSubTask(IReportsProgress task, Func<Task> func)
        {
            var nameChanged = new EventHandler((sender, e) => this.Name = task.Name);
            var descriptionChanged = new EventHandler((sender, e) => this.Description = task.Description);
            var positionChanged = new EventHandler((sender, e) => this.Position = task.Position);
            var countChanged = new EventHandler((sender, e) => this.Count = task.Count);
            task.NameChanged += nameChanged;
            task.DescriptionChanged += descriptionChanged;
            task.PositionChanged += positionChanged;
            task.CountChanged += countChanged;
            try
            {
                await func().ConfigureAwait(false);
            }
            finally
            {
                task.NameChanged -= nameChanged;
                task.DescriptionChanged -= descriptionChanged;
                task.PositionChanged -= positionChanged;
                task.CountChanged -= countChanged;
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
            lock (Instances)
            {
                for (var a = Instances.Count - 1; a >= 0; a--)
                {
                    var instance = Instances[a];
                    if (instance == null || !instance.IsAlive)
                    {
                        Instances.RemoveAt(a);
                    }
                    else if (object.ReferenceEquals(this, instance.Target))
                    {
                        Instances.RemoveAt(a);
                    }
                }
            }
            OnActiveChanged(this);
        }

        ~BackgroundTask()
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

        public static async Task<bool> Complete(TimeSpan interval, TimeSpan timeout)
        {
            do
            {
                var instances = Active.ToArray();
                if (instances.Length == 0)
                {
                    break;
                }
#if NET40
                await TaskEx.Delay(interval).ConfigureAwait(false);
#else
                await Task.Delay(interval).ConfigureAwait(false);
#endif
                timeout -= interval;
                if (timeout <= TimeSpan.Zero)
                {
                    return false;
                }
            } while (true);
            return true;
        }

        public static async Task<bool> Shutdown(TimeSpan interval, TimeSpan timeout)
        {
            OnShutdownStarted();
            do
            {
                var instances = Active.ToArray();
                if (instances.Length == 0)
                {
                    break;
                }
                foreach (var instance in instances)
                {
                    if (!instance.Cancellable || instance.IsCancellationRequested)
                    {
                        continue;
                    }
                    instance.Cancel();
                }
#if NET40
                await TaskEx.Delay(interval).ConfigureAwait(false);
#else
                await Task.Delay(interval).ConfigureAwait(false);
#endif
                timeout -= interval;
                if (timeout <= TimeSpan.Zero)
                {
                    return false;
                }
            } while (true);
            OnShutdownCompleted();
            return true;
        }

        private static void OnShutdownStarted()
        {
            if (ShutdownStarted == null)
            {
                return;
            }
            ShutdownStarted(null, EventArgs.Empty);
        }

        public static event EventHandler ShutdownStarted;

        private static void OnShutdownCompleted()
        {
            if (ShutdownCompleted == null)
            {
                return;
            }
            ShutdownCompleted(null, EventArgs.Empty);
        }

        public static event EventHandler ShutdownCompleted;
    }
}
