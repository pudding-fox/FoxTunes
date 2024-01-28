using FoxTunes.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace FoxTunes
{
    public class ExecutionStateBehaviour : StandardBehaviour, IDisposable
    {
        const int UPDATE_INTERVAL = 10000;

        private Timer Timer { get; set; }

        public IPlaybackManager PlaybackManager { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Timer = new Timer();
            this.Timer.Interval = UPDATE_INTERVAL;
            this.Timer.Elapsed += this.Timer_Elapsed;
            this.Timer.Start();
            this.PlaybackManager = core.Managers.Playback;
            base.InitializeComponent(core);
        }

        protected virtual void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.PlaybackManager != null && this.PlaybackManager.CurrentStream != null && this.PlaybackManager.CurrentStream.IsPlaying)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
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
            this.Timer.Dispose();
        }

        ~ExecutionStateBehaviour()
        {
            Logger.Write(this, LogLevel.Error, "Component was not disposed: {0}", this.GetType().Name);
            this.Dispose(true);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);

        [Flags]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }
    }
}
