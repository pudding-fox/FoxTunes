using System;

namespace FoxTunes
{
    [Serializable]
    public class CommandConfigurationElement : ConfigurationElement
    {
        public CommandConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {
        }

        public void Invoke()
        {
            if (this.Invoked != null)
            {
                this.Invoked(this, EventArgs.Empty);
            }
        }

        [field: NonSerialized]
        public EventHandler Invoked;

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
                    this.OnError(exception);
                }
            });
            this.Invoked += handler;
            return this;
        }

        public override void Reset()
        {
            //Nothing to do.
        }
    }
}
