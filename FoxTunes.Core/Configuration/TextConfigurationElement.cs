using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class TextConfigurationElement : ConfigurationElement<string>
    {
        public TextConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
        }

        protected override void OnUpdate(ConfigurationElement element, bool create)
        {
            var other = element as ConfigurationElement<string>;
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

        protected TextConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
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
