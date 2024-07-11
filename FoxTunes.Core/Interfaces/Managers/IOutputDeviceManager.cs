using System;
using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IOutputDeviceManager : IStandardComponent
    {
        IEnumerable<OutputDevice> Devices { get; }

        event EventHandler DevicesChanged;

        OutputDevice Device { get; set; }

        event EventHandler DeviceChanged;

        void Refresh();

        void Restart();
    }
}
