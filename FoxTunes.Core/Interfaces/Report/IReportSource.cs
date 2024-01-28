namespace FoxTunes.Interfaces
{
    public interface IReportSource : IBaseComponent
    {
        event ReportEventHandler Report;
    }

    public delegate void ReportEventHandler(object sender, ReportEventArgs e);

    public class ReportEventArgs : AsyncEventArgs
    {
        public ReportEventArgs(IReport report)
        {
            this.Report = report;
        }

        public IReport Report { get; private set; }
    }
}
