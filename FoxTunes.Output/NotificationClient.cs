using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class NotificationClient : IMMNotificationClient, IDisposable
    {
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
            var task = TaskEx.Run(() => this.DeviceStateChanged(this, new NotificationClientEventArgs(null, null, deviceId, state, PropertyKey.Empty)));
#else
            var task = Task.Run(() => this.DeviceStateChanged(this, new NotificationClientEventArgs(null, null, deviceId, state, PropertyKey.Empty)));
#endif
        }

        public event NotificationClientEventHandler DeviceStateChanged = delegate { };

        public void OnDeviceAdded(string deviceId)
        {
            if (this.DeviceAdded == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.DeviceAdded(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty)));
#else
            var task = Task.Run(() => this.DeviceAdded(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty)));
#endif
        }

        public event NotificationClientEventHandler DeviceAdded = delegate { };

        public void OnDeviceRemoved(string deviceId)
        {
            if (this.DeviceRemoved == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.DeviceRemoved(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty)));
#else
            var task = Task.Run(() => this.DeviceRemoved(this, new NotificationClientEventArgs(null, null, deviceId, null, PropertyKey.Empty)));
#endif
        }

        public event NotificationClientEventHandler DeviceRemoved = delegate { };

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
        {
            if (this.DefaultDeviceChanged == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.DefaultDeviceChanged(this, new NotificationClientEventArgs(flow, role, deviceId, null, PropertyKey.Empty)));
#else
            var task = Task.Run(() => this.DefaultDeviceChanged(this, new NotificationClientEventArgs(flow, role, deviceId, null, PropertyKey.Empty)));
#endif
        }

        public event NotificationClientEventHandler DefaultDeviceChanged = delegate { };

        public void OnPropertyValueChanged(string deviceId, PropertyKey key)
        {
            if (this.PropertyValueChanged == null)
            {
                return;
            }
            //Critical: Don't block in this event handler, it causes a deadlock.
#if NET40
            var task = TaskEx.Run(() => this.PropertyValueChanged(this, new NotificationClientEventArgs(null, null, deviceId, null, key)));
#else
            var task = Task.Run(() => this.PropertyValueChanged(this, new NotificationClientEventArgs(null, null, deviceId, null, key)));
#endif
        }

        public event NotificationClientEventHandler PropertyValueChanged = delegate { };

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
            this.Dispose(true);
        }

        public static readonly IMMDeviceEnumerator Enumerator = new DeviceEnumerator() as IMMDeviceEnumerator;
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
