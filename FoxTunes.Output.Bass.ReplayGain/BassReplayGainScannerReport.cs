using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class BassReplayGainScannerReport : BaseComponent, IReport
    {
        public BassReplayGainScannerReport(IEnumerable<IFileData> fileDatas, IEnumerable<ScannerItem> scannerItems)
        {
            this.FileDatas = fileDatas.ToDictionary(fileData => fileData.FileName, StringComparer.OrdinalIgnoreCase);
            this.ScannerItems = scannerItems.ToDictionary(scannerItem => Guid.NewGuid());
        }

        public Dictionary<string, IFileData> FileDatas { get; private set; }

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
            var builder = new StringBuilder();
            builder.Append(scannerItem.FileName);
            if (scannerItem.Status != ScannerItemStatus.Complete && scannerItem.Errors.Any())
            {
                builder.AppendLine(" -> Error");
                foreach (var error in scannerItem.Errors)
                {
                    builder.AppendLine('\t' + error);
                }
            }
            else
            {
                builder.AppendLine(" -> OK");
            }
            return builder.ToString();
        }

        public string[] Headers
        {
            get
            {
                return new[]
                {
                    "Path",
                    "Album",
                    "Album gain",
                    "Album peak",
                    "Track gain",
                    "Track peak",
                    "Status"
                };
            }
        }

        public IEnumerable<IReportRow> Rows
        {
            get
            {
                return this.ScannerItems.Select(element =>
                {
                    return new ReportRow(element.Key, this.FileDatas[element.Value.FileName], element.Value);
                });
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
            public ReportRow(Guid id, IFileData fileData, ScannerItem scannerItem)
            {
                this.Id = id;
                this.FileData = fileData;
                this.ScannerItem = scannerItem;
            }

            public Guid Id { get; private set; }

            public IFileData FileData { get; private set; }

            public ScannerItem ScannerItem { get; private set; }

            public string[] Values
            {
                get
                {
                    var value = default(double);
                    var path = this.ScannerItem.FileName;
                    var album = this.ScannerItem.GroupName;
                    var albumGain = default(string);
                    var albumPeak = default(string);
                    var trackGain = default(string);
                    var trackPeak = default(string);
                    var status = Enum.GetName(typeof(ScannerItemStatus), this.ScannerItem.Status);
                    lock (this.FileData.MetaDatas)
                    {
                        var metaDatas = this.FileData.MetaDatas.ToDictionary(
                            element => element.Name,
                            StringComparer.OrdinalIgnoreCase
                        );
                        var metaDataItem = default(MetaDataItem);
                        if (metaDatas.TryGetValue(CommonMetaData.ReplayGainAlbumGain, out metaDataItem) && double.TryParse(metaDataItem.Value, out value))
                        {
                            albumGain = string.Format(
                                "{0}{1:0.00} dB",
                                value > 0 ? "+" : string.Empty,
                                value
                            );
                        }
                        else
                        {
                            albumGain = string.Empty;
                        }
                        if (metaDatas.TryGetValue(CommonMetaData.ReplayGainAlbumPeak, out metaDataItem) && double.TryParse(metaDataItem.Value, out value))
                        {
                            albumPeak = string.Format(
                                "{0:0.000000}",
                                value
                            );
                        }
                        else
                        {
                            albumPeak = string.Empty;
                        }
                        if (metaDatas.TryGetValue(CommonMetaData.ReplayGainTrackGain, out metaDataItem) && double.TryParse(metaDataItem.Value, out value))
                        {
                            trackGain = string.Format(
                                "{0}{1:0.00} dB",
                                value > 0 ? "+" : string.Empty,
                                value
                            );
                        }
                        else
                        {
                            trackGain = string.Empty;
                        }
                        if (metaDatas.TryGetValue(CommonMetaData.ReplayGainTrackPeak, out metaDataItem) && double.TryParse(metaDataItem.Value, out value))
                        {
                            trackPeak = string.Format(
                                "{0:0.000000}",
                                value
                            );
                        }
                        else
                        {
                            trackPeak = string.Empty;
                        }
                    }
                    return new[]
                    {
                        path,
                        album,
                        albumGain,
                        albumPeak,
                        trackGain,
                        trackPeak,
                        status
                    };
                }
            }
        }
    }
}
