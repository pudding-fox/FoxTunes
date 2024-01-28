using MD.Net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class EraseMinidiscTask : MinidiscTask, IStatus
    {
        public IResult Result { get; private set; }

        protected override Task OnStarted()
        {
            this.Name = Strings.EraseMinidiscTask_Name;
            return base.OnStarted();
        }

        protected override Task OnRun()
        {
            var device = this.DeviceManager.GetDevices().FirstOrDefault();
            if (device != null)
            {
                var currentDisc = this.DiscManager.GetDisc(device);
                if (currentDisc != null)
                {
                    var updatedDisc = currentDisc.Clone();
                    updatedDisc.Title = string.Empty;
                    updatedDisc.Tracks.Clear();
                    var actions = this.ActionBuilder.GetActions(device, currentDisc, updatedDisc);
                    this.Result = this.DiscManager.ApplyActions(device, actions, this, true);
                    if (this.Result.Status != ResultStatus.Success)
                    {
                        throw new Exception(this.Result.Message);
                    }
                }
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
