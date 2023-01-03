using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [WindowsUserInterfaceDependency]
    public class SpectrumBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string CATEGORY = "3DF40656-FDD5-4B98-A25C-66DDFFD66CA0";

        public IUserInterface UserInterface { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public SelectionConfigurationElement Bars { get; private set; }

        public BooleanConfigurationElement Peaks { get; private set; }

        public IntegerConfigurationElement CutOff { get; private set; }

        public SelectionConfigurationElement FFTSize { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.UserInterface = core.Components.UserInterface;
            this.Configuration = core.Components.Configuration;
            this.Bars = this.Configuration.GetElement<SelectionConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.BARS_ELEMENT
            );
            this.Peaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.PEAKS_ELEMENT
            );
            this.CutOff = this.Configuration.GetElement<IntegerConfigurationElement>(
                SpectrumBehaviourConfiguration.SECTION,
                SpectrumBehaviourConfiguration.CUT_OFF_ELEMENT
            );
            this.FFTSize = this.Configuration.GetElement<SelectionConfigurationElement>(
               VisualizationBehaviourConfiguration.SECTION,
               VisualizationBehaviourConfiguration.FFT_SIZE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                foreach (var option in this.Bars.Options)
                {
                    yield return new InvocationComponent(
                        CATEGORY,
                        option.Id,
                        string.Format("{0} {1}", option.Name, this.Bars.Name),
                        path: this.Bars.Name,
                        attributes: this.Bars.Value == option ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                    );
                }
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Peaks.Id,
                    this.Peaks.Name,
                    attributes: this.Peaks.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            if (string.Equals(component.Id, this.Peaks.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Peaks.Toggle();
            }
            else
            {
                var bars = this.Bars.Options.FirstOrDefault(option => string.Equals(option.Id, component.Id, StringComparison.OrdinalIgnoreCase));
                if (bars != null)
                {
                    this.Bars.Value = bars;
                    this.CheckSettings();
                }
            }

            this.Configuration.Save();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual void CheckSettings()
        {
            var bars = SpectrumBehaviourConfiguration.GetBars(this.Bars.Value);
            if (bars <= 128)
            {
                return;
            }
            if (this.CutOff.IsModified || this.FFTSize.IsModified)
            {
                return;
            }
            //Looks like we're using the default settings but a high count was selected, warn the user and present the settings.
            this.UserInterface.Warn(Strings.SpectrumBehaviour_Warning);
            var task = this.UserInterface.ShowSettings(
                Strings.SpectrumBehaviourConfiguration_Path,
                new[]
                {
                    SpectrumBehaviourConfiguration.SECTION
                }
            );
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return SpectrumBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
