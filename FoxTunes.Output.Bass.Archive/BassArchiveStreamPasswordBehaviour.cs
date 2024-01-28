using FoxTunes.Interfaces;
using ManagedBass.ZipStream;
using System;
using System.Collections.Generic;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class BassArchiveStreamPasswordBehaviour : StandardBehaviour
    {
        public static readonly object SyncRoot = new object();

        public Archive.GetPasswordHandler Handler;

        public BassArchiveStreamPasswordBehaviour()
        {
            this.Passwords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Handler = new Archive.GetPasswordHandler(this.GetPassword);
            Archive.GetPassword(this.Handler);
        }

        public Dictionary<string, string> Passwords { get; private set; }

        public IUserInterface UserInterface { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.UserInterface = core.Components.UserInterface;
            base.InitializeComponent(core);
        }

        protected virtual bool GetPassword(ref Archive.ArchivePassword password)
        {
            lock (SyncRoot)
            {
                if (this.Passwords.TryGetValue(password.path, out password.password))
                {
                    return true;
                }
                password.password = this.UserInterface.Prompt(string.Format("Please enter the password for \"{0}\":", password.path.GetName()));
                if (!string.IsNullOrEmpty(password.password))
                {
                    this.Passwords[password.path] = password.password;
                    return true;
                }
            }
            return false;
        }
    }
}
