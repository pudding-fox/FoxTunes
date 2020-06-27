using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class BooleanConfigurationElement : ConfigurationElement<bool>
    {
        public BooleanConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
        }

        public void Toggle()
        {
            this.Value = !this.Value;
        }

        protected override void OnUpdate(ConfigurationElement element, bool create)
        {
            var other = element as ConfigurationElement<bool>;
            if (other == null)
            {
                return;
            }
            this.Value = other.Value;
        }

        #region ISerializable

        public override bool IsPersistent
        {
            get
            {
                return true;
            }
        }

        protected BooleanConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
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
