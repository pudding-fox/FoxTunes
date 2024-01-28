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
                return Application.Current.Dispatcher.InvokeAsync(action).Task;
            }
            else
            {
                action();
                return Task.CompletedTask;
            }
        }

        public Task Run(Func<Task> func)
        {
            if (Application.Current != null)
            {
                return Application.Current.Dispatcher.InvokeAsync(func).Task;
            }
            else
            {
                return func();
            }
        }

        public Task RunAsync(Action action)
        {
            if (Application.Current != null)
            {
                return Application.Current.Dispatcher.BeginInvoke(action).Task;
            }
            else
            {
                action();
                return Task.CompletedTask;
            }
        }

        public Task RunAsync(Func<Task> func)
        {
            if (Application.Current != null)
            {
                return Application.Current.Dispatcher.BeginInvoke(func).Task;
            }
            else
            {
                return func();
            }
        }
    }
}
