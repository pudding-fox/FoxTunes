using FoxTunes.Interfaces;
using ManagedBass.Dsd;
using System.Collections.Generic;

namespace FoxTunes
{
    public class BassDsdStreamProviderBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public int Rate
        {
            get
            {
                return BassDsd.DefaultFrequency;
            }
            set
            {
                BassDsd.DefaultFrequency = value;
                Logger.Write(this, LogLevel.Debug, "DSD to PCM sample rate: {0}", MetaDataInfo.SampleRateDescription(BassDsd.DefaultFrequency));
            }
        }

        public int Gain
        {
            get
            {
                return BassDsd.DefaultGain;
            }
            set
            {
                BassDsd.DefaultGain = value;
                Logger.Write(this, LogLevel.Debug, "DSD to PCM gain: {0}{1}dB", BassDsd.DefaultGain > 0 ? "+" : string.Empty, BassDsd.DefaultGain);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassDsdStreamProviderBehaviourConfiguration.DSD_RATE_ELEMENT
            ).ConnectValue(value => this.Rate = BassDsdStreamProviderBehaviourConfiguration.GetRate(value));
            core.Components.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassDsdStreamProviderBehaviourConfiguration.DSD_GAIN_ELEMENT
            ).ConnectValue(value => this.Gain = value);
            base.InitializeComponent(core);
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassDsdStreamProviderBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
