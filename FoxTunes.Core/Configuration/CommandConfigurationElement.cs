using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public class CommandConfigurationElement : ConfigurationElement
    {
        public CommandConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {
        }

        public override bool IsModified
        {
            get
            {
                return false;
            }
        }

        public void Invoke()
        {
            if (this.Invoked != null)
            {
                this.Invoked(this, EventArgs.Empty);
            }
        }

        public event EventHandler Invoked;

        public CommandConfigurationElement WithHandler(Action action)
        {
            var handler = new EventHandler((sender, e) =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Logger.Write(
                        typeof(CommandConfigurationElement),
                        LogLevel.Warn,
                        "Failed to invoke command handler \"{0}\": {1}",
                        this.Id,
                        exception.Message
                    );
                }
            });
            this.Invoked += handler;
            return this;
        }

        public override void InitializeComponent()
        {
            //Nothing to do.
        }

        public override void Update(ConfigurationElement element)
        {
            //Nothing to do.
        }

        public override void Reset()
        {
            //Nothing to do.
        }

        public override string GetPersistentValue()
        {
            throw new NotImplementedException();
        }

        public override void SetPersistentValue(string value)
        {
            throw new NotImplementedException();
        }
    }
}
