using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class BackgroundTasks : ViewModelBase
    {
        public BackgroundTasks()
        {
            this.Items = new ObservableCollection<IBackgroundTask>();
        }

        public ObservableCollection<IBackgroundTask> Items { get; set; }

        protected override void OnCoreChanged()
        {
            ComponentRegistry.Instance.ForEach<IBackgroundTaskSource>(component => component.BackgroundTask += this.OnBackgroundTask);
            base.OnCoreChanged();
        }

        protected virtual void OnBackgroundTask(object sender, BackgroundTaskEventArgs e)
        {
            e.BackgroundTask.Started += this.OnBackgroundTaskStarted;
            e.BackgroundTask.Completed += this.OnBackgroundTaskCompleted;
        }

        protected virtual void OnBackgroundTaskStarted(object sender, EventArgs e)
        {
            this.Items.Add(sender as IBackgroundTask);
        }

        protected virtual void OnBackgroundTaskCompleted(object sender, EventArgs e)
        {
            this.Items.Remove(sender as IBackgroundTask);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BackgroundTasks();
        }
    }
}
