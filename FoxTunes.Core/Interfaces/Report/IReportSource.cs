namespace FoxTunes.Interfaces
{
    public interface IReportSource : IBaseComponent
    {
        event ReportSourceEventHandler Report;
    }

    public delegate void ReportSourceEventHandler(object sender, ReportSourceEventArgs e);

    public class ReportSourceEventArgs : AsyncEventArgs
    {
        public ReportSourceEventArgs(IReport report)
        {
            this.Report = report;
        }

        public IReport Report { get; private set; }
    }
}
