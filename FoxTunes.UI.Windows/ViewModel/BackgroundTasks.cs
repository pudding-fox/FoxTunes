using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class BackgroundTasks : ViewModelBase
    {
        public BackgroundTasks()
        {
            this.RunningTasks = new ObservableCollection<IBackgroundTask>();
            this.FaultedTasks = new ObservableCollection<IBackgroundTask>();
        }

        public ObservableCollection<IBackgroundTask> RunningTasks { get; set; }

        public ObservableCollection<IBackgroundTask> FaultedTasks { get; set; }

        protected override void OnCoreChanged()
        {
            ComponentRegistry.Instance.ForEach<IBackgroundTaskSource>(component => component.BackgroundTask += this.OnBackgroundTask);
            base.OnCoreChanged();
        }

        protected virtual void OnBackgroundTask(object sender, BackgroundTaskEventArgs e)
        {
            e.BackgroundTask.Started += this.OnBackgroundTaskStarted;
            e.BackgroundTask.Completed += this.OnBackgroundTaskCompleted;
            e.BackgroundTask.Faulted += this.OnBackgroundTaskFaulted;
        }

        protected virtual void OnBackgroundTaskStarted(object sender, EventArgs e)
        {
            var task = sender as IBackgroundTask;
            if (!task.Visible)
            {
                return;
            }
            this.RunningTasks.Add(task);
        }

        protected virtual void OnBackgroundTaskCompleted(object sender, EventArgs e)
        {
            var task = sender as IBackgroundTask;
            if (!task.Visible)
            {
                return;
            }
            this.RunningTasks.Remove(task);
        }

        protected virtual void OnBackgroundTaskFaulted(object sender, EventArgs e)
        {
            this.RunningTasks.Remove(sender as IBackgroundTask);
            this.FaultedTasks.Add(sender as IBackgroundTask);
        }

        public ICommand ClearFaultedTasksCommand
        {
            get
            {
                return new Command(() => this.FaultedTasks.Clear());
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BackgroundTasks();
        }
    }
}
