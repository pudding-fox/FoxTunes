using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ReportBehaviour : StandardBehaviour, IDisposable
    {
        public ICore Core { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            ComponentRegistry.Instance.ForEach(component =>
            {
                if (component is IReportSource)
                {
                    (component as IReportSource).Report += this.OnReport;
                }
            });
            this.Core = core;
            base.InitializeComponent(core);
        }

        protected virtual void OnReport(object sender, ReportSourceEventArgs e)
        {
            var task = Windows.Invoke(() =>
            {
                var window = new ReportWindow()
                {
                    DataContext = this.Core,
                    Source = e.Report,
                    ShowActivated = true,
                    Owner = Windows.ActiveWindow,
                };
                window.Show();
            });
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            ComponentRegistry.Instance.ForEach(component =>
            {
                if (component is IReportSource)
                {
                    (component as IReportSource).Report -= this.OnReport;
                }
            });
        }

        ~ReportBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }
}
