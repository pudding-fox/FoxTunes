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
        static BassOutput()
        {
            BassPluginLoader.Instance.Load();
        }

        const int START_ATTEMPTS = 5;

        const int START_ATTEMPT_INTERVAL = 400;

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
                if (this.Pipeline != null)
                {
                    builder.AppendLine();
                    builder.AppendLine(string.Format("Input = {0}", this.Pipeline.Input.Description));
                    foreach (var component in this.Pipeline.Components)
                    {
                        builder.AppendLine(string.Format("Component = {0}", component.Description));
                    }
                    builder.Append(string.Format("Output = {0}", this.Pipeline.Output.Description));
                }
                return builder.ToString();
            }
        }

        public ICore Core { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public IBassStreamFactory StreamFactory { get; private set; }

        public IBassStreamPipelineFactory PipelineFactory { get; private set; }

        public IBassStreamPipeline Pipeline { get; private set; }

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
            set
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
            set
            {
                this._Float = value;
                Logger.Write(this, LogLevel.Debug, "Float = {0}", this.Float);
                this.Shutdown();
            }
        }

        public void Start()
        {
            if (this.IsStarted)
            {
                this.Shutdown();
            }
            var exception = default(Exception);
            for (var a = 1; a <= START_ATTEMPTS; a++)
            {
                Logger.Write(this, LogLevel.Debug, "Starting BASS, attempt: {0}", a);
                try
                {
                    this.OnInit();
                    this.IsStarted = true;
                    break;
                }
                catch (Exception e)
                {
                    exception = e;
                    this.Shutdown(true);
                    Logger.Write(this, LogLevel.Warn, "Failed to start BASS: {0}", e.Message);
                }
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

        public override Task Shutdown()
        {
            return this.Shutdown(false);
        }

        protected virtual Task Shutdown(bool force)
        {
            if (force || this.IsStarted)
            {
                Logger.Write(this, LogLevel.Debug, "Stopping BASS.");
                try
                {
                    this.FreePipeline();
                    this.OnFree();
                    Logger.Write(this, LogLevel.Debug, "Stopped BASS.");
                }
                catch (Exception e)
                {
                    Logger.Write(this, LogLevel.Error, "Failed to stop BASS: {0}", e.Message);
                    this.OnError(e);
                }
                finally
                {
                    this.IsStarted = false;
                }
            }
            return Task.CompletedTask;
        }

        protected virtual void OnInit()
        {
            if (this.Init == null)
            {
                return;
            }
            this.Init(this, EventArgs.Empty);
        }

        public event EventHandler Init = delegate { };

        protected virtual void OnFree()
        {
            if (this.Free == null)
            {
                return;
            }
            this.Free(this, EventArgs.Empty);
        }

        public event EventHandler Free = delegate { };

        public override void InitializeComponent(ICore core)
        {
            this.Core = core;
            this.Configuration = core.Components.Configuration;
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
            this.StreamFactory = ComponentRegistry.Instance.GetComponent<IBassStreamFactory>();
            this.StreamFactory.Register(new BassStreamProvider());
            this.PipelineFactory = ComponentRegistry.Instance.GetComponent<IBassStreamPipelineFactory>();
            base.InitializeComponent(core);
        }

        public override bool IsSupported(string fileName)
        {
            return BassUtils
                .GetInputFormats()
                .Contains(fileName.GetExtension(), true);
        }

        public override Task<IOutputStream> Load(PlaylistItem playlistItem, bool immidiate)
        {
            if (!this.IsStarted)
            {
                this.Start();
            }
            Logger.Write(this, LogLevel.Debug, "Loading stream: {0} => {1}", playlistItem.Id, playlistItem.FileName);
            var channelHandle = default(int);
            if (!this.StreamFactory.CreateStream(playlistItem, immidiate, out channelHandle))
            {
                return Task.FromResult<IOutputStream>(null);
            }
            var outputStream = new BassOutputStream(this, playlistItem, channelHandle);
            outputStream.InitializeComponent(this.Core);
            return Task.FromResult<IOutputStream>(outputStream);
        }

        public void FreeStream(int channelHandle)
        {
            Logger.Write(this, LogLevel.Debug, "Freeing stream: {0}", channelHandle);
            Bass.StreamFree(channelHandle); //Not checking result code as it contains an error if the application is shutting down.
        }

        public override Task<bool> Preempt(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            if (this.IsStarted && this.Pipeline != null)
            {
                if (this.Pipeline.Input.CheckFormat(outputStream.Rate, outputStream.Channels))
                {
                    if (this.Pipeline.Input.Add(outputStream.ChannelHandle))
                    {
                        Logger.Write(this, LogLevel.Debug, "Pre-empted playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
                        return Task.FromResult(true);
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
            else
            {
                Logger.Write(this, LogLevel.Debug, "Not yet started, cannot pre-empt playback of stream from file {0}: {1}", outputStream.FileName, outputStream.ChannelHandle);
            }
            return Task.FromResult(false);
        }

        public override Task Unload(IOutputStream stream)
        {
            var outputStream = stream as BassOutputStream;
            Logger.Write(this, LogLevel.Debug, "Unloading stream: {0}", outputStream.ChannelHandle);
            if (this.IsStarted && this.Pipeline != null)
            {
                if (this.Pipeline.Input.Contains(outputStream.ChannelHandle))
                {
                    var current = this.Pipeline.Input.Position(outputStream.ChannelHandle) == 0;
                    if (current)
                    {
                        Logger.Write(this, LogLevel.Debug, "Stream is playing, stopping the pipeline and clearing the buffer: {0}", outputStream.ChannelHandle);
                        this.Pipeline.Stop();
                    }
                    this.Pipeline.Input.Remove(outputStream.ChannelHandle);
                    if (current)
                    {
                        this.Pipeline.ClearBuffer();
                    }
                }
            }
            stream.Dispose();
            return Task.CompletedTask;
        }

        public IBassStreamPipeline GetOrCreatePipeline(BassOutputStream stream)
        {
            if (this.Pipeline == null)
            {
                lock (BassStreamPipeline.SyncRoot)
                {
                    if (this.Pipeline == null)
                    {
                        return this.Pipeline = this.CreatePipeline(stream);
                    }
                }
            }
            if (!this.Pipeline.Input.CheckFormat(stream.Rate, stream.Channels))
            {
                Logger.Write(this, LogLevel.Debug, "Current pipeline cannot accept stream, shutting it down: {0}", stream.ChannelHandle);
                this.FreePipeline();
                return this.GetOrCreatePipeline(stream);
            }
            return this.Pipeline;
        }

        protected virtual IBassStreamPipeline CreatePipeline(BassOutputStream stream)
        {
            Logger.Write(this, LogLevel.Debug, "Creating pipeline for stream: {0}", stream.ChannelHandle);
            var pipeline = this.PipelineFactory.CreatePipeline(stream);
            pipeline.Input.Add(stream.ChannelHandle);
            pipeline.Error += this.OnPipelineError;
            return pipeline;
        }

        protected virtual void FreePipeline()
        {
            if (this.Pipeline != null)
            {
                lock (BassStreamPipeline.SyncRoot)
                {
                    var pipeline = this.Pipeline;
                    if (pipeline != null)
                    {
                        //Remove this value so the pipeline cannot be returned once disposal begins.
                        this.Pipeline = null;
                        Logger.Write(this, LogLevel.Debug, "Shutting down the pipeline.");
                        pipeline.Error -= this.OnPipelineError;
                        pipeline.Dispose();
                    }
                }
            }
        }

        protected virtual Task OnPipelineError(object sender, ComponentOutputErrorEventArgs e)
        {
            Logger.Write(this, LogLevel.Error, "Pipeline encountered an error, shutting it down: {0}", e.Message);
            this.Shutdown();
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
}
