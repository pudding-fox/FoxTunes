using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Components : ViewModelBase
    {
        public Components()
        {
            this.Errors = new ObservableCollection<ComponentError>();
        }

        public IForegroundTaskRunner ForegroundTaskRunner { get; private set; }

        public ObservableCollection<ComponentError> Errors { get; set; }

        protected override void OnCoreChanged()
        {
            this.ForegroundTaskRunner = this.Core.Components.ForegroundTaskRunner;
            ComponentRegistry.Instance.ForEach(component => component.Error += this.OnError);
            base.OnCoreChanged();
        }

        protected virtual Task OnError(object sender, ComponentOutputErrorEventArgs e)
        {
            return this.ForegroundTaskRunner.RunAsync(() => this.Errors.Add(new ComponentError(sender as IBaseComponent, e.Message, e.Exception)));
        }

        public ICommand ClearErrorsCommand
        {
            get
            {
                return new Command(() => this.Errors.Clear());
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Components();
        }
    }

    public class ComponentError
    {
        public ComponentError(IBaseComponent component, string message, Exception exception)
        {
            this.Component = component;
            this.Message = message;
            this.Exception = exception;
        }

        public IBaseComponent Component { get; private set; }

        public string Source
        {
            get
            {
                return this.Component.GetType().Name;
            }
        }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }
    }
}
