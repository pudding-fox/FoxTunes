using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FoxTunes
{
    public class ImageBrushFactory : StandardFactory
    {
        public ThemeLoader ThemeLoader { get; private set; }

        public ImageLoader ImageLoader { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public DoubleConfigurationElement ScalingFactor { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.ThemeLoader = ComponentRegistry.Instance.GetComponent<ThemeLoader>();
            this.ImageLoader = ComponentRegistry.Instance.GetComponent<ImageLoader>();
            this.SignalEmitter = core.Components.SignalEmitter;
            this.SignalEmitter.Signal += this.OnSignal;
            this.Configuration = core.Components.Configuration;
            this.ScalingFactor = this.Configuration.GetElement<DoubleConfigurationElement>(
                WindowsUserInterfaceConfiguration.SECTION,
                WindowsUserInterfaceConfiguration.UI_SCALING_ELEMENT
            );
            base.InitializeComponent(core);
        }

        protected virtual Task OnSignal(object sender, ISignal signal)
        {
            switch (signal.Name)
            {
                case CommonSignals.PluginInvocation:
                    switch (signal.State as string)
                    {
                        case ImageBehaviour.REFRESH_IMAGES:
                            return this.Reset();
                    }
                    break;
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public ImageBrush Create(string fileName, int width, int height)
        {
            var size = Windows.ActiveWindow.GetElementPixelSize(
                width * this.ScalingFactor.Value,
                height * this.ScalingFactor.Value
            );
            var source = ImageLoader.Load(
                fileName,
                Convert.ToInt32(size.Width),
                Convert.ToInt32(size.Height),
                true
            );
            var brush = new ImageBrush(source)
            {
                Stretch = Stretch.Uniform
            };
            brush.Freeze();
            return brush;
        }

        protected virtual Task Reset()
        {
            //TODO: Reset caches.
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
