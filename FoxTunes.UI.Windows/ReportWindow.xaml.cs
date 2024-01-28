using System.Collections.Generic;
using System.Windows;
using FoxTunes.Interfaces;
using System.Linq;
using System;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : WindowBase
    {
        const int STARTUP_LOCATION_OFFSET = 90;

        protected static ILogger Logger
        {
            get
            {
                return LogManager.Logger;
            }
        }

        static ReportWindow()
        {
            Instances = new List<WeakReference<ReportWindow>>();
        }

        private static IList<WeakReference<ReportWindow>> Instances { get; set; }

        public static IEnumerable<ReportWindow> Active
        {
            get
            {
                lock (Instances)
                {
                    return Instances
                        .Where(instance => instance != null && instance.IsAlive)
                        .Select(instance => instance.Target)
                        .ToArray();
                }
            }
        }

        protected static void OnActiveChanged(ReportWindow sender)
        {
            if (ActiveChanged == null)
            {
                return;
            }
            ActiveChanged(sender, EventArgs.Empty);
        }

        public static event EventHandler ActiveChanged;

        public ReportWindow()
        {
            var instance = Active.LastOrDefault();
            if (instance != null)
            {
                this.Left = instance.Left + STARTUP_LOCATION_OFFSET;
                this.Top = instance.Top + STARTUP_LOCATION_OFFSET;
                this.Width = instance.Width;
                this.Height = instance.Height;
            }
            else if (!global::FoxTunes.Properties.Settings.Default.ReportWindowBounds.IsEmpty())
            {
                if (ScreenHelper.WindowBoundsVisible(global::FoxTunes.Properties.Settings.Default.ReportWindowBounds))
                {
                    this.Left = global::FoxTunes.Properties.Settings.Default.ReportWindowBounds.Left;
                    this.Top = global::FoxTunes.Properties.Settings.Default.ReportWindowBounds.Top;
                }
                this.Width = global::FoxTunes.Properties.Settings.Default.ReportWindowBounds.Width;
                this.Height = global::FoxTunes.Properties.Settings.Default.ReportWindowBounds.Height;
            }
            else
            {
                this.Width = 600;
                this.Height = 400;
            }
            if (double.IsNaN(this.Left) || double.IsNaN(this.Top))
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            this.InitializeComponent();
            lock (Instances)
            {
                Instances.Add(new WeakReference<ReportWindow>(this));
            }
            OnActiveChanged(this);
        }

        public IEnumerable<global::FoxTunes.ViewModel.Report> ViewModels
        {
            get
            {
                return new FrameworkElement[] { this, this.Report }.Select(
                    element => element.FindResource<global::FoxTunes.ViewModel.Report>("ViewModel")
                );
            }
        }

        public IReport Source
        {
            get
            {
                foreach (var viewModel in this.ViewModels)
                {
                    return viewModel.Source;
                }
                return default(IReport);
            }
            set
            {
                foreach (var viewModel in this.ViewModels)
                {
                    viewModel.Source = value;
                }
            }
        }

        protected virtual void OnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
