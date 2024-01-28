using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace FoxTunes.ViewModel
{
    public class BackgroundTasks : ViewModelBase
    {
        public BackgroundTasks()
        {
            this.RunningTasks = new ObservableCollection<BackgroundTask>();
        }

        public ObservableCollection<BackgroundTask> RunningTasks { get; set; }

        public override void InitializeComponent(ICore core)
        {
            ComponentRegistry.Instance.ForEach<IBackgroundTaskSource>(component => component.BackgroundTask += this.OnBackgroundTask);
            base.InitializeComponent(core);
        }

        protected virtual void OnBackgroundTask(object sender, BackgroundTaskEventArgs e)
        {
            e.BackgroundTask.Started += this.OnBackgroundTaskStarted;
            e.BackgroundTask.Completed += this.OnBackgroundTaskCompleted;
            e.BackgroundTask.Faulted += this.OnBackgroundTaskFaulted;
        }

        protected virtual void OnBackgroundTaskStarted(object sender, EventArgs e)
        {
            var backgroundTask = sender as IBackgroundTask;
            if (backgroundTask == null || !backgroundTask.Visible)
            {
                return;
            }
            this.Add(backgroundTask);
        }

        protected virtual void OnBackgroundTaskCompleted(object sender, EventArgs e)
        {
            var backgroundTask = sender as IBackgroundTask;
            if (backgroundTask == null)
            {
                return;
            }
            this.Remove(backgroundTask);
        }

        protected virtual void OnBackgroundTaskFaulted(object sender, EventArgs e)
        {
            var backgroundTask = sender as IBackgroundTask;
            if (backgroundTask == null)
            {
                return;
            }
            this.Remove(backgroundTask);
        }

        public Task Add(IBackgroundTask backgroundTask)
        {
            return Windows.Invoke(() => this.RunningTasks.Add(new BackgroundTask(backgroundTask)));
        }

        public Task Remove(IBackgroundTask backgroundTask)
        {
            backgroundTask.Started -= this.OnBackgroundTaskStarted;
            backgroundTask.Completed -= this.OnBackgroundTaskCompleted;
            backgroundTask.Faulted -= this.OnBackgroundTaskFaulted;
            foreach (var element in this.RunningTasks)
            {
                if (object.ReferenceEquals(element.InnerBackgroundTask, backgroundTask))
                {
                    return Windows.Invoke(() => this.RunningTasks.Remove(element));
                }
            }
            return Task.CompletedTask;
        }

        protected override void OnDisposing()
        {
            ComponentRegistry.Instance.ForEach<IBackgroundTaskSource>(component => component.BackgroundTask -= this.OnBackgroundTask);
            base.OnDisposing();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BackgroundTasks();
        }
    }
}
