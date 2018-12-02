using FoxTunes.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes
{
    public class WindowsForegroundTaskRunner : StandardComponent, IForegroundTaskRunner
    {
        public Task Run(Action action)
        {
            if (Application.Current != null)
            {
                return Application.Current.Dispatcher.BeginInvoke(action).Task;
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Cannot execute task: Application.Current is null.");
                return Task.CompletedTask;
            }
        }

        public Task Run(Func<Task> func)
        {
            if (Application.Current != null)
            {
                return Application.Current.Dispatcher.BeginInvoke(func).Task;
            }
            else
            {
                Logger.Write(this, LogLevel.Warn, "Cannot execute task: Application.Current is null.");
                return Task.CompletedTask;
            }
        }
    }
}
