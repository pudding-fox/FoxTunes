using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FoxTunes
{
    public class BassEncoderReport : BaseComponent, IReport
    {
        public BassEncoderReport(IEnumerable<EncoderItem> encoderItems)
        {
            this.EncoderItems = encoderItems.ToDictionary(encoderItem => Guid.NewGuid());
        }

        public Dictionary<Guid, EncoderItem> EncoderItems { get; private set; }

        public string Title
        {
            get
            {
                return "Encoder Output";
            }
        }

        public string Description
        {
            get
            {
                return string.Join(
                    Environment.NewLine,
                    this.EncoderItems.Values.Select(
                        encoderItem => this.GetDescription(encoderItem)
                    )
                );
            }
        }

        protected virtual string GetDescription(EncoderItem encoderItem)
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                "{0} -> {1}",
                encoderItem.InputFileName,
                !string.IsNullOrEmpty(encoderItem.OutputFileName) ? encoderItem.OutputFileName : "<Cancelled>"
            );
            if (encoderItem.Status != EncoderItemStatus.Complete && encoderItem.Errors.Any())
            {
                builder.Append(Environment.NewLine);
                foreach (var error in encoderItem.Errors)
                {
                    builder.Append("\t");
                    builder.Append(error);
                }
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
                    "Codec",
                    "Status"
                };
            }
        }

        public IEnumerable<IReportRow> Rows
        {
            get
            {
                return this.EncoderItems.Select(element => new ReportRow(element.Key, element.Value));
            }
        }

        public Action<Guid> Action
        {
            get
            {
                return key =>
                {
                    var encoderItem = default(EncoderItem);
                    if (!this.EncoderItems.TryGetValue(key, out encoderItem) || !File.Exists(encoderItem.OutputFileName))
                    {
                        return;
                    }
                    this.FileSystemBrowser.Select(encoderItem.OutputFileName);
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
            public ReportRow(Guid id, EncoderItem encoderItem)
            {
                this.Id = id;
                this.EncoderItem = encoderItem;
            }

            public Guid Id { get; private set; }

            public EncoderItem EncoderItem { get; private set; }

            public string[] Values
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.EncoderItem.OutputFileName))
                    {
                        return new[]
                        {
                            this.EncoderItem.OutputFileName,
                            Path.GetExtension(this.EncoderItem.OutputFileName).TrimStart('.').ToUpper(),
                            Enum.GetName(typeof(EncoderItemStatus), this.EncoderItem.Status)
                        };
                    }
                    else
                    {
                        return new[]
                        {
                            this.EncoderItem.InputFileName,
                            Path.GetExtension(this.EncoderItem.InputFileName).TrimStart('.').ToUpper(),
                            Enum.GetName(typeof(EncoderItemStatus), this.EncoderItem.Status)
                        };
                    }
                }
            }
        }
    }
}
