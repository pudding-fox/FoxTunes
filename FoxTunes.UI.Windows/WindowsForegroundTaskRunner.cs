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
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
            return Task.CompletedTask;
        }
    }
}
