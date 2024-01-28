using System.Threading.Tasks;

namespace FoxTunes
{
    public class BassEncoderMonitor : PopulatorBase, IBassEncoderMonitor
    {
        public BassEncoderMonitor(IBassEncoder encoder, bool reportProgress) : base(reportProgress)
        {
            this.Encoder = encoder;
        }

        public IBassEncoder Encoder { get; private set; }

        public async Task Encode(string[] fileNames, IBassEncoderSettings settings)
        {
#if NET40
            var task = TaskEx.Run(() =>
#else
            var task = Task.Run(() =>
#endif
            {
                this.Encoder.Encode(fileNames, settings);
            });
            await task;
        }
    }
}
