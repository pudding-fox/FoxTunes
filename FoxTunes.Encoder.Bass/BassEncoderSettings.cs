using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.IO;
using System.Linq;

namespace FoxTunes
{
    public abstract class BassEncoderSettings : BaseComponent, IBassEncoderSettings
    {
        public const int DEPTH_AUTO = 0;

        public const int DEPTH_16 = 16;

        public const int DEPTH_24 = 24;

        public const int DEPTH_32 = 32;

        public abstract string Name { get; }

        public string Executable { get; protected set; }

        public virtual string Directory
        {
            get
            {
                return Path.GetDirectoryName(this.Executable);
            }
        }

        public abstract string Extension { get; }

        public abstract IBassEncoderFormat Format { get; }

        public IFileSystemBrowser FileSystemBrowser { get; private set; }

        public BassEncoderOutputDestination Destination { get; private set; }

        private string _BrowseFolder { get; set; }

        public bool GetBrowseFolder(string fileName, out string directoryName)
        {
            if (string.IsNullOrEmpty(this._BrowseFolder))
            {
                var options = new BrowseOptions(
                    "Save As",
                    Path.GetDirectoryName(fileName),
                    Enumerable.Empty<BrowseFilter>(),
                    BrowseFlags.Folder
                );
                var result = this.FileSystemBrowser.Browse(options);
                if (!result.Success)
                {
                    Logger.Write(this, LogLevel.Debug, "Save As folder browse dialog was cancelled.");
                    directoryName = null;
                    return false;
                }
                this._BrowseFolder = result.Paths.FirstOrDefault();
                Logger.Write(this, LogLevel.Debug, "Browse folder: {0}", this._BrowseFolder);
            }
            directoryName = this._BrowseFolder;
            return true;
        }

        public string SpecificFolder { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.FileSystemBrowser = core.Components.FileSystemBrowser;
            core.Components.Configuration.GetElement<SelectionConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_ELEMENT
            ).ConnectValue(value => this.Destination = BassEncoderBehaviourConfiguration.GetDestination(value));
            core.Components.Configuration.GetElement<TextConfigurationElement>(
                BassEncoderBehaviourConfiguration.SECTION,
                BassEncoderBehaviourConfiguration.DESTINATION_LOCATION_ELEMENT
            ).ConnectValue(value => this.SpecificFolder = value);
            base.InitializeComponent(core);
        }

        public abstract string GetArguments(EncoderItem encoderItem, IBassStream stream);

        public virtual bool TryGetOutput(string inputFileName, out string outputFileName)
        {
            var name = Path.GetFileNameWithoutExtension(inputFileName);
            var directoryName = default(string);
            switch (this.Destination)
            {
                default:
                case BassEncoderOutputDestination.Browse:
                    if (!this.GetBrowseFolder(inputFileName, out directoryName))
                    {
                        outputFileName = default(string);
                        return false;
                    }
                    break;
                case BassEncoderOutputDestination.Source:
                    directoryName = Path.GetDirectoryName(inputFileName);
                    break;
                case BassEncoderOutputDestination.Specific:
                    directoryName = this.SpecificFolder;
                    break;
            }
            if (!this.CanWrite(directoryName))
            {
                throw new InvalidOperationException(string.Format("Cannot output to path \"{0}\" please check encoder settings.", directoryName));
            }
            outputFileName = Path.Combine(directoryName, string.Format("{0}.{1}", name, this.Extension));
            return true;
        }

        protected virtual bool CanWrite(string directoryName)
        {
            var uri = default(Uri);
            if (!Uri.TryCreate(directoryName, UriKind.Absolute, out uri))
            {
                return false;
            }
            return string.Equals(uri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase);
        }

        public virtual int GetDepth(EncoderItem encoderItem, IBassStream stream)
        {
            if (this.Format.AutoDepth)
            {
                if (encoderItem.BitsPerSample != 0)
                {
                    Logger.Write(this, LogLevel.Debug, "Using meta data suggested bit depth for file \"{0}\": {1} bit", encoderItem.InputFileName, encoderItem.BitsPerSample);
                    return encoderItem.BitsPerSample;
                }
                var channelInfo = default(ChannelInfo);
                if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
                {
                    throw new NotImplementedException();
                }
                if (channelInfo.Flags.HasFlag(BassFlags.Float))
                {
                    Logger.Write(this, LogLevel.Debug, "Using decoder bit depth for file \"{0}\": 32 bit", encoderItem.InputFileName);
                    return DEPTH_32;
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "Using decoder bit depth for file \"{0}\": 16 bit", encoderItem.InputFileName);
                    return DEPTH_16;
                }
            }
            Logger.Write(this, LogLevel.Debug, "Using user defined bit depth for file \"{0}\": {1} bit", encoderItem.InputFileName, this.Format.Depth);
            return this.Format.Depth;
        }

        public virtual long GetLength(EncoderItem encoderItem, IBassStream stream)
        {
            var source = default(int);
            var channelInfo = default(ChannelInfo);
            var inputLength = Bass.ChannelGetLength(stream.ChannelHandle, PositionFlags.Bytes);
            if (!Bass.ChannelGetInfo(stream.ChannelHandle, out channelInfo))
            {
                throw new NotImplementedException();
            }
            if (channelInfo.Flags.HasFlag(BassFlags.Float))
            {
                source = DEPTH_32;
            }
            else
            {
                source = DEPTH_16;
            }
            var outputLength = (long)(inputLength / (source / (float)this.GetDepth(encoderItem, stream)));
            if (inputLength != outputLength)
            {
                Logger.Write(this, LogLevel.Debug, "Conversion requires change of data length: {0} bytes => {1} bytes.", inputLength, outputLength);
            }
            return outputLength;
        }
    }
}
