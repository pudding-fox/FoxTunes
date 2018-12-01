using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
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
            ComponentRegistry.Instance.ForEach(component =>
            {
                component.Error += this.OnError;
                if (component is IBackgroundTaskSource)
                {
                    (component as IBackgroundTaskSource).BackgroundTask += this.OnBackgroundTask;
                }
            });
            ViewModelBase.Error += this.OnError;
            base.OnCoreChanged();
        }

        protected virtual async Task OnError(object sender, ComponentErrorEventArgs e)
        {
            var component = sender as IBaseComponent;
            if (e.Exception is AggregateException)
            {
                foreach (var innerException in (e.Exception as AggregateException).InnerExceptions)
                {
                    await this.Add(new ComponentError(component, component.GetType().Name, innerException));
                }
            }
            else
            {
                await this.Add(new ComponentError(component, component.GetType().Name, e.Exception));
            }
        }

        protected virtual void OnBackgroundTask(object sender, BackgroundTaskEventArgs e)
        {
            e.BackgroundTask.Faulted += this.OnFaulted;
        }

        protected virtual async void OnFaulted(object sender, AsyncEventArgs e)
        {
            var backgroundTask = sender as IBackgroundTask;
            using (e.Defer())
            {
                if (backgroundTask.Exception is AggregateException)
                {
                    foreach (var innerException in (backgroundTask.Exception as AggregateException).InnerExceptions)
                    {
                        await this.Add(new ComponentError(backgroundTask, backgroundTask.Name, innerException));
                    }
                }
                else
                {
                    await this.Add(new ComponentError(backgroundTask, backgroundTask.Name, backgroundTask.Exception));
                }
            }
        }

        public Task Add(ComponentError error)
        {
            return this.ForegroundTaskRunner.Run(() =>
            {
                if (this.Errors.Contains(error))
                {
                    return;
                }
                this.Errors.Add(error);
                //WPF glitch; ItemsControl in a Popup displays duplicate items.
                CollectionViewSource.GetDefaultView(this.Errors).Refresh();
            });
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

    public class ComponentError : IEquatable<ComponentError>
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

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (!string.IsNullOrEmpty(this.Message))
            {
                hashCode += this.Message.GetHashCode();
            }
            if (this.Exception != null && !string.IsNullOrEmpty(this.Exception.Message))
            {
                hashCode += this.Exception.Message.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object other)
        {
            if (other is ComponentError)
            {
                return this.Equals(other as ComponentError);
            }
            return base.Equals(other);
        }

        public bool Equals(ComponentError other)
        {
            if (other == null)
            {
                return false;
            }
            if (!string.Equals(this.Message, other.Message, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (this.Exception != null && other.Exception == null)
            {
                return false;
            }
            if (this.Exception == null && other.Exception != null)
            {
                return false;
            }
            if (!string.Equals(this.Exception.Message, other.Exception.Message, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }
    }
}
