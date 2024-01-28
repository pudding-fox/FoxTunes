using System;

namespace FoxTunes.Interfaces
{
    public interface IBackgroundTaskSource : IBaseComponent
    {
        event BackgroundTaskEventHandler BackgroundTask;
    }

    public delegate void BackgroundTaskEventHandler(object sender, BackgroundTaskEventArgs e);

    public class BackgroundTaskEventArgs : AsyncEventArgs
    {
        public BackgroundTaskEventArgs(IBackgroundTask backgroundTask)
        {
            this.BackgroundTask = backgroundTask;
        }

        public IBackgroundTask BackgroundTask { get; private set; }
    }
}
