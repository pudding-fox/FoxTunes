using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

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

        protected override void OnUpdate(ConfigurationElement element, bool create)
        {
            //Nothing to do.
        }

        #region ISerializable

        public override bool IsPersistent
        {
            get
            {
                return false;
            }
        }

        protected CommandConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
