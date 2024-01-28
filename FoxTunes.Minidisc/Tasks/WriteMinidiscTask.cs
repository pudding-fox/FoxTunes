using MD.Net;
using System;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class WriteMinidiscTask : MinidiscTask, IStatus
    {
        public WriteMinidiscTask(IDevice device, IActions actions)
        {
            this.Device = device;
            this.Actions = actions;
        }

        public IDevice Device { get; private set; }

        public IActions Actions { get; private set; }

        public IResult Result { get; private set; }

        protected override Task OnStarted()
        {
            this.Name = Strings.WriteMinidiscTask_Name;
            return base.OnStarted();
        }

        protected override Task OnRun()
        {
            this.Result = this.DiscManager.ApplyActions(this.Device, this.Actions, this, true);
            if (this.Result.Status != ResultStatus.Success)
            {
                throw new Exception(this.Result.Message);
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public void Update(string message, int position, int count, StatusType type)
        {
            switch (type)
            {
                case StatusType.Action:
                    this.Description = message;
                    this.Position = position;
                    this.Count = count;
                    break;
            }
        }
#pragma warning disable 0067

        public event StatusEventHandler Updated;

#pragma warning restore 0067
    }
}
