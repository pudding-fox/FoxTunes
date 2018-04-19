using System;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IBassOutput : IConfigurableComponent, IDisposable
    {
        int Rate { get; }

        bool EnforceRate { get; }

        bool Float { get; }

        void FreeStream(int channelHandle);

        Task Shutdown();

        event EventHandler Init;

        event EventHandler Free;
    }
}
