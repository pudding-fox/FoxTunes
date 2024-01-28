using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class IntegerConfigurationElement : ConfigurationElement<int>
    {
        public IntegerConfigurationElement(string id, string name = null, string description = null, string path = null) : base(id, name, description, path)
        {
        }

        protected override void OnUpdate(ConfigurationElement element, bool create)
        {
            var other = element as ConfigurationElement<int>;
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

        protected IntegerConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
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