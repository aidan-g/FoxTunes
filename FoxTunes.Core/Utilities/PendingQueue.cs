using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Timers;

namespace FoxTunes
{
    public class PendingQueue<T> : IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public static readonly object SyncRoot = new object();

        private PendingQueue()
        {
            this.Queue = new Queue<T>();
        }

        public PendingQueue(int timeout)
            : this()
        {
            this.Timer = new Timer(timeout);
            this.Timer.AutoReset = false;
            this.Timer.Elapsed += this.OnElapsed;
        }


        public PendingQueue(TimeSpan timeout)
            : this(Convert.ToInt32(timeout.TotalMilliseconds))
        {

        }
        public Timer Timer { get; private set; }

        public Queue<T> Queue { get; private set; }

        public void Enqueue(T value)
        {
            lock (SyncRoot)
            {
                this.Queue.Enqueue(value);
                this.Timer.Stop();
                this.Timer.Start();
            }
        }

        public void EnqueueRange(IEnumerable<T> values)
        {
            lock (SyncRoot)
            {
                this.Queue.EnqueueRange(values);
                this.Timer.Stop();
                this.Timer.Start();
            }
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (SyncRoot)
                {
                    this.OnComplete();
                }
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        protected virtual void OnComplete()
        {
            if (this.Complete != null)
            {
                var e = new PendingQueueEventArgs<T>(this.Queue.ToArray());
                this.Complete(this, e);
            }
            this.Queue.Clear();
        }

        public event PendingQueueEventHandler<T> Complete;

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
            lock (SyncRoot)
            {
                if (this.Timer != null)
                {
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                }
            }
        }

        ~PendingQueue()
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

    public delegate void PendingQueueEventHandler<T>(object sender, PendingQueueEventArgs<T> e);

    public class PendingQueueEventArgs<T> : EventArgs
    {
        public PendingQueueEventArgs(IEnumerable<T> sequence)
        {
            this.Sequence = sequence;
        }

        public IEnumerable<T> Sequence { get; private set; }
    }
}
