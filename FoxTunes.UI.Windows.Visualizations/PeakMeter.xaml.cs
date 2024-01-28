using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for PeakMeter.xaml
    /// </summary>
    [UIComponent("F8231616-9D5E-45C8-BD72-506FC5FC9C95", role: UIComponentRole.Visualization)]
    public partial class PeakMeter : ConfigurableUIComponentBase
    {
        public const string CATEGORY = "F58AE444-E7F1-4D3D-9CE6-D1612892CF28";

        public PeakMeter()
        {
            this.InitializeComponent();
        }

        public ThemeLoader ThemeLoader { get; private set; }

        public BooleanConfigurationElement Peaks { get; private set; }

        public BooleanConfigurationElement Rms { get; private set; }

        public TextConfigurationElement ColorPalette { get; private set; }

        protected override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            base.InitializeComponent(core);
        }

        protected override void OnConfigurationChanged()
        {
            if (this.Configuration != null)
            {
                this.Peaks = this.Configuration.GetElement<BooleanConfigurationElement>(
                   PeakMeterConfiguration.SECTION,
                   PeakMeterConfiguration.PEAKS
               );
                this.Rms = this.Configuration.GetElement<BooleanConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.RMS
                );
                this.ColorPalette = this.Configuration.GetElement<TextConfigurationElement>(
                    PeakMeterConfiguration.SECTION,
                    PeakMeterConfiguration.COLOR_PALETTE
                );
            }
            base.OnConfigurationChanged();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            var size = sizeInfo.NewSize;
            if (!double.IsNaN(size.Width) && !double.IsNaN(size.Height) && size.Width > 0 && size.Height > 0)
            {
                var ratio = size.Width / size.Height;
                var orientation = default(Orientation);
                if (ratio > 1)
                {
                    orientation = Orientation.Horizontal;
                    this.MinWidth = 160;
                    this.MinHeight = 30;
                }
                else
                {
                    orientation = Orientation.Vertical;
                    this.MinWidth = 30;
                    this.MinHeight = 160;
                }
                var viewModel = this.FindResource<global::FoxTunes.ViewModel.PeakMeter>("ViewModel");
                if (viewModel != null && viewModel.Orientation != orientation)
                {
                    viewModel.Orientation = orientation;
                }
            }
            base.OnRenderSizeChanged(sizeInfo);
        }

        public override IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return CATEGORY;
            }
        }

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.ColorPalette.IsVisible)
                {
                    foreach (var component in this.ThemeLoader.SelectColorPalette(CATEGORY, this.ColorPalette))
                    {
                        yield return component;
                    }
                }
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Peaks.Id,
                    this.Peaks.Name,
                    attributes: this.Peaks.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    CATEGORY,
                    this.Rms.Id,
                    this.Rms.Name,
                    attributes: this.Rms.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                foreach (var invocationComponent in base.Invocations)
                {
                    yield return invocationComponent;
                }
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            if (string.Equals(component.Id, this.Peaks.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Peaks.Toggle();
            }
            else if (string.Equals(component.Id, this.Rms.Id, StringComparison.OrdinalIgnoreCase))
            {
                this.Rms.Toggle();
            }
            else if (this.ThemeLoader.SelectColorPalette(this.ColorPalette, component))
            {
                //Nothing to do.
            }
            else if (string.Equals(component.Id, SETTINGS, StringComparison.OrdinalIgnoreCase))
            {
                return this.ShowSettings();
            }
            this.SaveSettings();
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task<bool> ShowSettings()
        {
            return this.ShowSettings(
                Strings.PeakMeterConfiguration_Path,
                PeakMeterConfiguration.SECTION
            );
        }

        public override IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PeakMeterConfiguration.GetConfigurationSections();
        }
    }
}