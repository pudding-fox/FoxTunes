using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class ExecutionStateBehaviour : StandardBehaviour, IConfigurableComponent, IDisposable
    {
        const int UPDATE_INTERVAL = 10000;

        private Timer Timer { get; set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private EXECUTION_STATE _ExecutionState { get; set; }

        public EXECUTION_STATE ExecutionState
        {
            get
            {
                return this._ExecutionState;
            }
            set
            {
                this._ExecutionState = value;
                this.OnExecutionStateChanged();
            }
        }

        protected virtual void OnExecutionStateChanged()
        {
            this.SetThreadExecutionState();
            if (this.ExecutionState.HasFlag(EXECUTION_STATE.ES_SYSTEM_REQUIRED) || this.ExecutionState.HasFlag(EXECUTION_STATE.ES_DISPLAY_REQUIRED))
            {
                if (this.Timer == null)
                {
                    this.Timer = new Timer();
                    this.Timer.Interval = UPDATE_INTERVAL;
                    this.Timer.Elapsed += this.OnElapsed;
                    this.Timer.Start();
                }
            }
            else
            {
                if (this.Timer != null)
                {
                    this.Timer.Stop();
                    this.Timer.Elapsed -= this.OnElapsed;
                    this.Timer.Dispose();
                    this.Timer = null;
                }
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaybackManager = core.Managers.Playback;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<SelectionConfigurationElement>(
                ExecutionStateBehaviourConfiguration.SECTION,
                ExecutionStateBehaviourConfiguration.SLEEP_ELEMENT
            ).ConnectValue(value => this.ExecutionState = ExecutionStateBehaviourConfiguration.GetExecutionState(value));
            base.InitializeComponent(core);
        }

        protected virtual void OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.SetThreadExecutionState();
            }
            catch
            {
                //Nothing can be done, never throw on background thread.
            }
        }

        protected virtual void SetThreadExecutionState()
        {
            if (this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying)
            {
                SetThreadExecutionState(this.ExecutionState);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return ExecutionStateBehaviourConfiguration.GetConfigurationSections();
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
            if (this.Timer != null)
            {
                this.Timer.Stop();
                this.Timer.Elapsed -= this.OnElapsed;
                this.Timer.Dispose();
                this.Timer = null;
            }
        }

        ~ExecutionStateBehaviour()
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);
    }
}
