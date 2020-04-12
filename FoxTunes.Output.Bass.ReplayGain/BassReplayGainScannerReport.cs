using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public class BassReplayGainScannerReport : BaseComponent, IReport
    {
        public BassReplayGainScannerReport(IEnumerable<ScannerItem> scannerItems)
        {
            this.ScannerItems = scannerItems.ToDictionary(scannerItem => Guid.NewGuid());
        }

        public Dictionary<Guid, ScannerItem> ScannerItems { get; private set; }

        public string Title
        {
            get
            {
                return "Scanner Output";
            }
        }

        public string Description
        {
            get
            {
                return string.Join(
                    Environment.NewLine,
                    this.ScannerItems.Values.Select(
                        scannerItem => this.GetDescription(scannerItem)
                    )
                );
            }
        }

        protected virtual string GetDescription(ScannerItem scannerItem)
        {
            throw new NotImplementedException();
        }

        public string[] Headers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IReportRow> Rows
        {
            get
            {
                return this.ScannerItems.Select(element => new ReportRow(element.Key, element.Value));
            }
        }

        public Action<Guid> Action
        {
            get
            {
                return key =>
                {
                    var scannerItem = default(ScannerItem);
                    if (!this.ScannerItems.TryGetValue(key, out scannerItem) || !File.Exists(scannerItem.FileName))
                    {
                        return;
                    }
                    this.FileSystemBrowser.Select(scannerItem.FileName);
                };
            }
        }

        public IFileSystemBrowser FileSystemBrowser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.FileSystemBrowser = core.Components.FileSystemBrowser;
            base.InitializeComponent(core);
        }

        public class ReportRow : IReportRow
        {
            public ReportRow(Guid id, ScannerItem scannerItem)
            {
                this.Id = id;
                this.ScannerItem = scannerItem;
            }

            public Guid Id { get; private set; }

            public ScannerItem ScannerItem { get; private set; }

            public string[] Values
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
