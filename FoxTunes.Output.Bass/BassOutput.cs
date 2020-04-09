using FoxTunes.Interfaces;
using ManagedBass;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("E0318CB1-57A0-4DC3-AA8D-F6E100F86190", ComponentSlots.Output)]
    public class BassOutput : Output, IBassOutput
    {
        const int START_STOP_TIMEOUT = 10000;

        static BassOutput()
        {
            BassPluginLoader.Instance.Load();
        }

        const int START_ATTEMPTS = 5;

        const int START_ATTEMPT_INTERVAL = 400;

        public BassOutput()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public SemaphoreSlim Semaphore { get; private set; }

        public override string Name
        {
            get
            {
                return "BASS";
            }
        }

        public override string Description
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(this.Name);
                builder.Append(string.Format(" v{0}", Bass.Version));
                this.PipelineManager.WithPipeline(pipeline =>
                {
                    if (pipeline != null)
                    {
                        builder.AppendLine();
                        builder.AppendLine(string.Format("Input = {0}", pipeline.Input.Description));
                        foreach (var component in pipeline.Components)
                        {
                            builder.AppendLine(string.Format("Component = {0}", component.Description));
                        }
                        builder.Append(string.Format("Output = {0}", pipeline.Output.Description));
                    }
                });
                return builder.ToString();
            }
        }

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamFactory StreamFactory { get; private set; }

        public IBassStreamPipelineManager PipelineManager { get; private set; }

        private int _Rate { get; set; }

        public int Rate
        {
            get
            {
                return this._Rate;
            }
            set
            {
                this._Rate = value;
                Logger.Write(this, LogLevel.Debug, "Rate = {0}", this.Rate);
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
        }

        private bool _EnforceRate { get; set; }

        public bool EnforceRate
        {
            get
            {
                return this._EnforceRate;
            }
            set
            {
                this._EnforceRate = value;
                Logger.Write(this, LogLevel.Debug, "Enforce Rate = {0}", this.EnforceRate);
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
        }

        private bool _Float { get; set; }

        public bool Float
        {
            get
            {
                return this._Float;
            }
            set
            {
                this._Float = value;
                Logger.Write(this, LogLevel.Debug, "Float = {0}", this.Float);
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
        }

        private bool _PlayFromMemory { get; set; }

        public bool PlayFromMemory
        {
            get
            {
                return this._PlayFromMemory;
            }
            set
            {
                this._PlayFromMemory = value;
                Logger.Write(this, LogLevel.Debug, "PlayFromMemory = {0}", this.PlayFromMemory);
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
        }

        private int _BufferLength { get; set; }

        public int BufferLength
        {
            get
            {
                return this._BufferLength;
            }
            set
            {
                this._BufferLength = value;
                Logger.Write(this, LogLevel.Debug, "BufferLength = {0}", this.BufferLength);
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
        }

        public override bool ShowBuffering
        {
            get
            {
                return this.PipelineManager.WithPipeline(pipeline => pipeline == null || this.PlayFromMemory);
            }
        }

        public override Task Start()
        {
            return this.Start(false);
        }

        protected virtual async Task Start(bool force)
        {
#if NET40
            if (!this.Semaphore.Wait(START_STOP_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(START_STOP_TIMEOUT).ConfigureAwait(false))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already starting.", this.GetType().Name));
            }
            try
            {
                await this.OnStart(force).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task OnStart(bool force)
        {
            if (force || !this.IsStarted)
            {
                await this.ShutdownCore(false).ConfigureAwait(false);
                var exception = default(Exception);
                for (var a = 1; a <= START_ATTEMPTS; a++)
                {
                    Logger.Write(this, LogLevel.Debug, "Starting BASS, attempt: {0}", a);
                    try
                    {
                        this.OnInit();
                        await this.SetIsStarted(true).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        Logger.Write(this, LogLevel.Warn, "Failed to start BASS: {0}", e.Message);
                    }
                    await this.ShutdownCore(true).ConfigureAwait(false);
                    Thread.Sleep(START_ATTEMPT_INTERVAL);
                }
                if (this.IsStarted)
                {
                    Logger.Write(this, LogLevel.Debug, "Started BASS.");
                    return;
                }
                else if (exception != null)
                {
                    throw exception;
                }
            }
        }

        public override async Task Shutdown()
        {
#if NET40
            if (!this.Semaphore.Wait(START_STOP_TIMEOUT))
#else
            if (!await this.Semaphore.WaitAsync(START_STOP_TIMEOUT).ConfigureAwait(false))
#endif
            {
                throw new InvalidOperationException(string.Format("{0} is already stopping.", this.GetType().Name));
            }
            try
            {
                await this.ShutdownCore(false).ConfigureAwait(false);
            }
            finally
            {
                this.Semaphore.Release();
            }
        }

        protected virtual async Task ShutdownCore(bool force)
        {
            if (force || this.IsStarted)
            {
                var exception = default(Exception);
                Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
                try
                {
                    await this.PipelineManager.FreePipeline().ConfigureAwait(false);
                    this.OnFree();
                    Logger.Write(this, LogLevel.Debug, "Stopped BASS.");
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Failed to stop BASS: {0}", e.Message);
                    exception = e;
                }
                await this.SetIsStarted(false).ConfigureAwait(false);
                if (exception != null)
                {
                    await this.OnError(exception).ConfigureAwait(false);
                }
            }
        }

        protected virtual void OnInit()
        {
            if (this.Init == null)
            {
                return;
            }
            this.Init(this, EventArgs.Empty);
        }

        public event EventHandler Init;

        protected virtual void OnFree()
        {
            if (this.Free == null)
            {
                return;
            }
            this.Free(this, EventArgs.Empty);
        }

        public event EventHandler Free;

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.RATE_ELEMENT
            ).ConnectValue(value => this.Rate = BassOutputConfiguration.GetRate(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.ENFORCE_RATE_ELEMENT
            ).ConnectValue(value => this.EnforceRate = value);
            this.Configuration.GetElement<SelectionConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.DEPTH_ELEMENT
            ).ConnectValue(value => this.Float = BassOutputConfiguration.GetFloat(value));
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.PLAY_FROM_RAM_ELEMENT
            ).ConnectValue(value => this.PlayFromMemory = value);
            this.Configuration.GetElement<IntegerConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.BUFFER_LENGTH_ELEMENT
            ).ConnectValue(value => this.BufferLength = value);
            this.Configuration.GetElement<BooleanConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.VOLUME_ENABLED_ELEMENT
            ).ConnectValue(value => this.CanControlVolume = value);
            this.Configuration.GetElement<DoubleConfigurationElement>(
                BassOutputConfiguration.SECTION,
                BassOutputConfiguration.VOLUME_ELEMENT
            ).ConnectValue(value => this.Volume = Convert.ToSingle(value));
            this.StreamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            this.PipelineManager = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineManager>();
            this.PipelineManager.Error += this.OnPipelineManagerError;
            base.InitializeComponent(core);
        }

        protected virtual Task OnPipelineManagerError(object sender, ComponentErrorEventArgs e)
        {
            //This usually means that the device buffer was lost.
            //Nothing can be done.
            return this.ShutdownCore(true);
        }

        public override IEnumerable<string> SupportedExtensions
        {
            get
            {
                return BassUtils.GetInputFormats();
            }
        }

        public override bool IsSupported(string fileName)
        {
            var extension = fileName.GetExtension();
            return BassUtils.IsSupported(extension);
        }

        public override async Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate)
        {
            if (!this.IsStarted)
            {
                await this.Start().ConfigureAwait(false);
            }
            Logger.Write(this, LogLevel.Debug, "Loading stream: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            var stream = await this.StreamFactory.CreateStream(playlistItem, immidiate).ConfigureAwait(false);
            if (stream.IsEmpty)
            {
                return null;
            }
            var outputStream = new BassOutputStream(this, this.PipelineManager, stream.Provider, playlistItem, stream.ChannelHandle);
            outputStream.InitializeComponent(this.Core);
            this.OnLoaded(outputStream);
            return outputStream;
        }

        public override Task<bool> Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            if (this.IsStarted)
            {
                return this.PipelineManager.WithPipelineExclusive(pipeline =>
                {
                    if (pipeline != null)
                    {
                        if (pipeline.Input.CheckFormat(outputStream.Rate, outputStream.Channels))
                        {
                            if (pipeline.Input.Add(outputStream.ChannelHandle))
                            {
                                Logger.Write(this, LogLevel.Debug, "Pre-empted playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                                return true;
                            }
                            else
                            {
                                //Probably already in the queue.
                            }
                        }
                        else
                        {
                            Logger.Write(this, LogLevel.Debug, "Properties differ from current configuration, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                        }
                    }
                    return false;
                });
            }
            else
            {
                Logger.Write(this, LogLevel.Debug, "Not yet started, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.FromResult(false);
#endif
        }

        public override async Task Unload(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            Logger.Write(this, LogLevel.Debug, "Unloading stream: {0}", outputStream.ChannelHandle);
            if (this.IsStarted)
            {
                await this.PipelineManager.WithPipelineExclusive(pipeline =>
                {
                    if (pipeline != null)
                    {
                        if (pipeline.Input.Contains(outputStream.ChannelHandle))
                        {
                            var current = pipeline.Input.Position(outputStream.ChannelHandle) == 0;
                            if (current)
                            {
                                Logger.Write(this, LogLevel.Debug, "Stream is playing, stopping the pipeline and clearing the buffer: {0}", outputStream.ChannelHandle);
                                pipeline.Stop();
                            }
                            pipeline.Input.Remove(outputStream.ChannelHandle);
                            if (current)
                            {
                                pipeline.ClearBuffer();
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            this.OnUnloaded(outputStream);
            outputStream.Dispose();
        }

        public override int GetData(float[] buffer)
        {
            var result = default(int);
            this.PipelineManager.WithPipeline(pipeline =>
            {
                if (pipeline != null)
                {
                    result = pipeline.Output.GetData(buffer);
                }
            });
            return result;
        }

        private bool _CanControlVolume { get; set; }

        public override bool CanControlVolume
        {
            get
            {
                return this._CanControlVolume;
            }
            protected set
            {
                this._CanControlVolume = value;
                this.OnCanControlVolumeChanged();
            }
        }

        protected override void OnCanControlVolumeChanged()
        {
            Logger.Write(this, LogLevel.Debug, "CanControlVolume = {0}", this.CanControlVolume);
            //TODO: Bad .Wait().
            this.Shutdown().Wait();
            base.OnCanControlVolumeChanged();
        }

        private float _Volume { get; set; }

        public override float Volume
        {
            get
            {
                return this._Volume;
            }
            set
            {
                this._Volume = value;
                this.OnVolumeChanged();
            }
        }

        protected override void OnVolumeChanged()
        {
            if (this.PipelineManager != null && this.CanControlVolume)
            {
                this.PipelineManager.WithPipeline(pipeline =>
                {
                    if (pipeline != null && pipeline.Output.CanControlVolume)
                    {
                        pipeline.Output.Volume = this.Volume;
                    }
                });
            }
            base.OnVolumeChanged();
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
                //TODO: Bad .Wait().
                this.Shutdown().Wait();
            }
            if (this.Semaphore != null)
            {
                this.Semaphore.Dispose();
            }
        }

        ~BassOutput()
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
