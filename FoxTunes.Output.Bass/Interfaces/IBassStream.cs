using ManagedBass;
using System;

namespace FoxTunes.Interfaces
{
    public interface IBassStream
    {
        IBassStreamProvider Provider { get; }

        int ChannelHandle { get; }

        int[] Syncs { get; }

        long Length { get; }

        long Position { get; set; }

        bool IsEmpty { get; }

        Errors Errors { get; }

        event EventHandler Ending;

        event EventHandler Ended;

        void RegisterSyncHandlers();

        bool CanReset { get; }

        void Reset();
    }
}
