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
            Application.Current.Dispatcher.Invoke(action);
            return Task.CompletedTask;
        }
    }
}
