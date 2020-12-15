using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class NotificationClient : IMMNotificationClient, IDisposable
    {
        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        public NotificationClient()
        {
            Enumerator.RegisterEndpointNotificationCallback(this);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState state)
        {
            if (this.DeviceStateChanged == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.DeviceStateChanged(this, new NotificationClientEventArgs(null, null, deviceId, state, PropertyKey.Empty))
            );
        }

        public event NotificationClientEventHandler DeviceStateChanged;

        public void OnDeviceAdded(string deviceId)
        {
            if (this.DeviceAdded == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.DeviceAdded(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty))
            );
        }

        public event NotificationClientEventHandler DeviceAdded;

        public void OnDeviceRemoved(string deviceId)
        {
            if (this.DeviceRemoved == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.DeviceRemoved(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty))
            );
        }

        public event NotificationClientEventHandler DeviceRemoved;

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
        {
            if (this.DefaultDeviceChanged == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.DefaultDeviceChanged(this, new NotificationClientEventArgs(flow, role, deviceId, null, PropertyKey.Empty))
            );
        }

        public event NotificationClientEventHandler DefaultDeviceChanged;

        public void OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            if (this.PropertyValueChanged == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(
#else
            var task = Task.Run(
#endif
                () => this.PropertyValueChanged(this, new NotificationClientEventArgs(null, null, deviceId, null, key))
            );
        }

        public event NotificationClientEventHandler PropertyValueChanged;

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
            Enumerator.UnregisterEndpointNotificationCallback(this);
        }

        ~NotificationClient()
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

        public static readonly IMMDeviceEnumerator Enumerator = DeviceEnumeratorFactory.Create();
    }

    public delegate void NotificationClientEventHandler(object sender, NotificationClientEventArgs e);

    public class NotificationClientEventArgs : AsyncEventArgs
    {
        public NotificationClientEventArgs(DataFlow? flow, Role? role, string device, DeviceState? state, PropertyKey key)
        {
            this.Flow = flow;
            this.Role = role;
            this.Device = device;
            this.State = state;
            this.Key = key;
        }

        public DataFlow? Flow { get; private set; }

        public Role? Role { get; private set; }

        public string Device { get; private set; }

        public DeviceState? State { get; private set; }

        public PropertyKey Key { get; private set; }
    }
}