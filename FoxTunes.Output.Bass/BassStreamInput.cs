using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public abstract class BassStreamInput : BaseComponent, IBassStreamInput
    {
        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract IEnumerable<int> Queue { get; }

        public virtual bool PreserveBuffer
        {
            get
            {
                return false;
            }
        }

        public abstract int Rate { get; protected set; }

        public abstract int Channels { get; protected set; }

        public abstract BassFlags Flags { get; protected set; }

        public abstract int ChannelHandle { get; protected set; }

        public virtual long BufferLength
        {
            get
            {
                return 0;
            }
        }

        public virtual bool IsActive
        {
            get
            {
                return true;
            }
        }

        public abstract void Connect(IBassStreamComponent previous);

        public virtual void ClearBuffer()
        {
            //Nothing to do.
        }

        protected virtual void OnInvalidate()
        {
            if (this.Invalidate == null)
            {
                return;
            }
            this.Invalidate(this, EventArgs.Empty);
        }

        public event EventHandler Invalidate;

        public abstract bool CheckFormat(BassOutputStream stream);

        public abstract bool Contains(BassOutputStream stream);

        public abstract int Position(BassOutputStream stream);

        public abstract bool Add(BassOutputStream stream);

        public abstract bool Remove(BassOutputStream stream, Action<BassOutputStream> callBack);

        public abstract void Reset();

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

        protected abstract void OnDisposing();

        ~BassStreamInput()
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
