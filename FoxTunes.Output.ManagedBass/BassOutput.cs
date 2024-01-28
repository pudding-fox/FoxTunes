using FoxTunes.Interfaces;
using ManagedBass;
using ManagedBass.Dsd;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("E0318CB1-57A0-4DC3-AA8D-F6E100F86190", ComponentSlots.Output)]
    public class BassOutput : Output, IConfigurableComponent, IDisposable
    {
        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BassOutputChannel OutputChannel { get; private set; }

        private int _Rate { get; set; }

        public int Rate
        {
            get
            {
                return this._Rate;
            }
            private set
            {
                this._Rate = value;
                Logger.Write(this, LogLevel.Debug, "Rate = {0}", this.Rate);
                this.Shutdown();
            }
        }

        private bool _EnforceRate { get; set; }

        public bool EnforceRate
        {
            get
            {
                return this._EnforceRate;
            }
            private set
            {
                this._EnforceRate = value;
                Logger.Write(this, LogLevel.Debug, "Enforce Rate = {0}", this.EnforceRate);
                this.Shutdown();
            }
        }

        private bool _Float { get; set; }

        public bool Float
        {
            get
            {
                return this._Float;
            }
            private set
            {
                this._Float = value;
                Logger.Write(this, LogLevel.Debug, "Float = {0}", this.Float);
                this.Shutdown();
            }
        }

        private BassOutputMode _Mode { get; set; }

        public BassOutputMode Mode
        {
            get
            {
                return this._Mode;
            }
            private set
            {
                this._Mode = value;
                Logger.Write(this, LogLevel.Debug, "Mode = {0}", Enum.GetName(typeof(BassOutputMode), this.Mode));
                this.Shutdown();
            }
        }

        private int _DirectSoundDevice { get; set; }

        public int DirectSoundDevice
        {
            get
            {
                return this._DirectSoundDevice;
            }
            private set
            {
                this._DirectSoundDevice = value;
                Logger.Write(this, LogLevel.Debug, "Direct Sound Device = {0}", this.DirectSoundDevice);
                this.Shutdown();
            }
        }

        private int _AsioDevice { get; set; }

        public int AsioDevice
        {
            get
            {
                return this._AsioDevice;
            }
            private set
            {
                this._AsioDevice = value;
                Logger.Write(this, LogLevel.Debug, "ASIO Device = {0}", this.AsioDevice);
                this.Shutdown();
            }
        }

        private int _WasapiDevice { get; set; }

        public int WasapiDevice
        {
            get
            {
                return this._WasapiDevice;
            }
            private set
            {
                this._WasapiDevice = value;
                Logger.Write(this, LogLevel.Debug, "WASAPI Device = {0}", this.WasapiDevice);
                this.Shutdown();
            }
        }

        private bool _DsdDirect { get; set; }

        public bool DsdDirect
        {
            get
            {
                return this._DsdDirect;
            }
            private set
            {
                this._DsdDirect = value;
                Logger.Write(this, LogLevel.Debug, "DSD = {0}", this.DsdDirect);
                this.Shutdown();
            }
        }


        private bool _SoxResampler { get; set; }

        public bool SoxResampler
        {
            get
            {
                return this._SoxResampler;
            }
            private set
            {
                this._SoxResampler = value;
                Logger.Write(this, LogLevel.Debug, "Sox = {0}", this.SoxResampler);
                this.Shutdown();
            }
        }

        public BassFlags Flags
        {
            get
            {
                var flags = BassFlags.Decode;
                if (this.Float)
                {
                    flags |= BassFlags.Float;
                }
                return flags;
            }
        }

        public void Start()
        {
            if (this.IsStarted)
            {
                this.Shutdown();
            }
            Logger.Write(this, LogLevel.Debug, "Starting BASS.");
            try
            {
                switch (this.Mode)
                {
                    case BassOutputMode.DirectSound:
                        this.StartDirectSound();
                        break;
                    case BassOutputMode.ASIO:
                        this.StartASIO();
                        break;
                    case BassOutputMode.WASAPI:
                        this.StartWASAPI();
                        break;
                }
                this.OutputChannel = BassOutputChannelFactory.Instance.Create(this);
                this.OutputChannel.InitializeComponent(this.Core);
                this.OutputChannel.Error += this.OutputChannel_Error;
                this.IsStarted = true;
                Logger.Write(this, LogLevel.Debug, "Started BASS.");
            }
            catch (Exception e)
            {
                this.Shutdown(true);
                this.OnError(e);
            }
        }

        private void StartDirectSound()
        {
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 1));
            BassUtils.OK(Bass.Init(this.DirectSoundDevice, this.Rate));
            Logger.Write(this, LogLevel.Debug, "BASS Initialized.");
        }

        private void StartASIO()
        {
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice));
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        private void StartWASAPI()
        {
            BassUtils.OK(Bass.Configure(global::ManagedBass.Configuration.UpdateThreads, 0));
            BassUtils.OK(Bass.Init(Bass.NoSoundDevice));
            Logger.Write(this, LogLevel.Debug, "BASS (No Sound) Initialized.");
        }

        public void Shutdown(bool force = false)
        {
            if (!force && !this.IsStarted)
            {
                return;
            }
            Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
            try
            {
                if (this.OutputChannel != null)
                {
                    this.OutputChannel.Dispose();
                    this.OutputChannel = null;
                }
                Bass.Free();
                Logger.Write(this, LogLevel.Debug, "Stopped BASS.");
            }
            catch (Exception e)
            {
                this.OnError(e);
            }
            finally
            {
                this.IsStarted = false;
            }
        }

        protected virtual void OutputChannel_Error(object sender, ComponentOutputErrorEventArgs e)
        {
            this.Shutdown();
            this.OnError(e.Exception);
        }

        public override void InitializeComponent(ICore core)
        {
            BassPluginLoader.Instance.Load();
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.MODE_ELEMENT
            ).ConnectValue<string>(value => this.Mode = BassOutputConfiguration.GetMode(value));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_DS_DEVICE
            ).ConnectValue<string>(value => this.DirectSoundDevice = BassOutputConfiguration.GetDsDevice(value));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_ASIO_DEVICE
            ).ConnectValue<string>(value => this.AsioDevice = BassOutputConfiguration.GetAsioDevice(value));
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ELEMENT_WASAPI_DEVICE
            ).ConnectValue<string>(value => this.WasapiDevice = BassOutputConfiguration.GetWasapiDevice(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DSD_RAW_ELEMENT
            ).ConnectValue<bool>(value => this.DsdDirect = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.RATE_ELEMENT
            ).ConnectValue<string>(value => this.Rate = BassOutputConfiguration.GetRate(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.ENFORCE_RATE_ELEMENT
            ).ConnectValue<bool>(value => this.EnforceRate = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.DEPTH_ELEMENT
            ).ConnectValue<string>(value => this.Float = BassOutputConfiguration.GetFloat(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.OUTPUT_SECTION,
                BassOutputConfiguration.SOX_RESAMPLER_ELEMENT
            ).ConnectValue<bool>(value => this.SoxResampler = value);
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            extension = extension.Substring(1);
            return BassUtils
                .GetInputFormats()
                .Any(format => string.Equals(format, extension, StringComparison.OrdinalIgnoreCase));
        }

        public override Task<IOutputStream> Load(PlaylistItem playlistItem)
        {
            if (!this.IsStarted)
            {
                this.Start();
            }
            var channelHandle = this.CreateStream(playlistItem);
            var outputStream = new BassOutputStream(this, playlistItem, channelHandle);
            outputStream.InitializeComponent(this.Core);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        private int CreateStream(PlaylistItem playlistItem)
        {
            Logger.Write(this, LogLevel.Debug, "Creating stream from file {0}", playlistItem.FileName);
            var channelHandle = default(int);
            if (this.IsDsd(playlistItem) && this.Mode == BassOutputMode.ASIO && this.DsdDirect)
            {
                Logger.Write(this, LogLevel.Debug, "Creating DSD RAW stream from file {0}", playlistItem.FileName);
                channelHandle = BassDsd.CreateStream(playlistItem.FileName, 0, 0, BassFlags.Decode | BassFlags.DSDRaw);
            }
            else
            {
                channelHandle = Bass.CreateStream(playlistItem.FileName, 0, 0, this.Flags);
            }
            if (channelHandle == 0)
            {
                BassUtils.Throw();
            }
            Logger.Write(this, LogLevel.Debug, "Created stream from file {0}: {1}", playlistItem.FileName, channelHandle);
            return channelHandle;
        }

        private bool IsDsd(PlaylistItem playlistItem)
        {
            var extension = Path.GetExtension(playlistItem.FileName);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            extension = extension.Substring(1).ToLower(CultureInfo.InvariantCulture);
            return new[] { "dsd", "dsf" }.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public override Task Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            if (this.IsStarted && this.OutputChannel.IsStarted)
            {
                if (this.OutputChannel.CanPlay(outputStream))
                {
                    Logger.Write(this, LogLevel.Debug, "Pre-empting playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                    if (!this.OutputChannel.Contains(outputStream))
                    {
                        this.OutputChannel.Enqueue(outputStream);
                    }
                }
                else
                {
                    Logger.Write(this, LogLevel.Debug, "Properties differ from current configuration, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                }
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Not yet started, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            }
            return Task.CompletedTask;
        }

        public override Task Unload(IOutputStream stream)
        {
            stream.Dispose();
            return Task.CompletedTask;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return BassOutputConfiguration.GetConfigurationSections();
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
            if (this.IsStarted)
            {
                this.Shutdown();
            }
        }

        ~BassOutput()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }
    }

    public enum BassOutputMode : byte
    {
        None,
        DirectSound,
        ASIO,
        WASAPI
    }
}
