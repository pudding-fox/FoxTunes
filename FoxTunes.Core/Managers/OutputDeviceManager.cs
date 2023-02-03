using FoxDb;
using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class OutputDeviceManager : StandardManager, IOutputDeviceManager, IDisposable
    {
        public OutputDeviceManager()
        {
            this.Selectors = new List<IOutputDeviceSelector>();
        }

        public IList<IOutputDeviceSelector> Selectors { get; private set; }

        public IOutputDeviceSelector Selector
        {
            get
            {
                return this.Selectors.FirstOrDefault(selector => selector.IsActive);
            }
        }

        public IEnumerable<OutputDevice> Devices
        {
            get
            {
                return this.Selectors.SelectMany(selector => selector.Devices);
            }
        }

        protected virtual void OnDevicesChanged()
        {
            if (this.DevicesChanged != null)
            {
                this.DevicesChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Devices");
        }

        public event EventHandler DevicesChanged;

        public OutputDevice Device
        {
            get
            {
                var selector = this.Selector;
                if (selector == null)
                {
                    return null;
                }
                return selector.Device;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                value.Selector.IsActive = true;
                value.Selector.Device = value;
            }
        }

        protected virtual void OnDeviceChanged()
        {
            if (this.DeviceChanged != null)
            {
                this.DeviceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Device");
        }

        public event EventHandler DeviceChanged;

        public override void InitializeComponent(ICore core)
        {
            this.Selectors.AddRange(
                ComponentRegistry.Instance.GetComponents<IOutputDeviceSelector>()
            );
            foreach (var selector in this.Selectors)
            {
                selector.IsActiveChanged += this.OnIsActiveChanged;
                selector.DevicesChanged += this.OnDevicesChanged;
                selector.DeviceChanged += this.OnDeviceChanged;
            }
            base.InitializeComponent(core);
        }

        protected virtual void OnIsActiveChanged(object sender, EventArgs e)
        {
            this.OnDeviceChanged();
        }

        protected virtual void OnDevicesChanged(object sender, EventArgs e)
        {
            if (this.IsRefreshing)
            {
                return;
            }
            this.OnDevicesChanged();
        }

        protected virtual void OnDeviceChanged(object sender, EventArgs e)
        {
            this.OnDeviceChanged();
        }

        public bool IsRefreshing { get; private set; }

        public void Refresh()
        {
            this.IsRefreshing = true;
            try
            {
                foreach (var selector in this.Selectors)
                {
                    selector.Refresh();
                }
            }
            finally
            {
                this.IsRefreshing = false;
                this.OnDevicesChanged();
            }
        }

        protected override void OnDisposing()
        {
            if (this.Selectors != null)
            {
                foreach (var selector in this.Selectors)
                {
                    selector.IsActiveChanged -= this.OnIsActiveChanged;
                    selector.DevicesChanged -= this.OnDevicesChanged;
                    selector.DeviceChanged -= this.OnDeviceChanged;
                }
            }
            base.OnDisposing();
        }
    }
}
