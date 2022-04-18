using MD.Net;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class OpenMinidiscTask : MinidiscTask
    {
        public IDevice Device { get; private set; }

        public IDisc Disc { get; private set; }

        protected override Task OnStarted()
        {
            this.Name = Strings.OpenMinidiscTask_Name;
            return base.OnStarted();
        }

        protected override Task OnRun()
        {
            this.Device = this.DeviceManager.GetDevices().FirstOrDefault();
            if (this.Device != null)
            {
                this.Disc = this.DiscManager.GetDisc(this.Device);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
