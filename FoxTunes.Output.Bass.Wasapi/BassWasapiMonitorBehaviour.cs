#if VISTA
using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class BassWasapiMonitorBehaviour : BassDeviceMonitorBehaviour
    {
        public IConfiguration Configuration { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Enabled = string.Equals(value, BassWasapiStreamOutputConfiguration.MODE_WASAPI_OPTION, StringComparison.OrdinalIgnoreCase));
            base.InitializeComponent(core);
        }

        protected override bool RestartRequired(DataFlow? flow, Role? role)
        {
            if (!BassWasapiDevice.IsDefaultDevice)
            {
                return false;
            }
            return base.RestartRequired(flow, role);
        }
    }
}
#endif