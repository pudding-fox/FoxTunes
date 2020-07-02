using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class UIComponent : ISerializable
    {
        public const string PLACEHOLDER = "00000000-0000-0000-0000-000000000000";

        public UIComponent(UIComponentAttribute attribute, Type type)
        {
            this.Id = attribute.Id;
            this.Slot = attribute.Slot;
            this.Name = attribute.Name;
            this.Description = attribute.Description;
            this.Role = attribute.Role;
            this.Type = type;
        }

        public string Id { get; private set; }

        public string Slot { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public UIComponentRole Role { get; private set; }

        public Type Type { get; private set; }

        #region ISerializable

        protected UIComponent(SerializationInfo info, StreamingContext context)
        {
            this.Id = info.GetString(nameof(this.Id));
            this.Slot = info.GetString(nameof(this.Slot));
            this.Name = info.GetString(nameof(this.Name));
            this.Description = info.GetString(nameof(this.Description));
            this.Role = (UIComponentRole)info.GetValue(nameof(this.Role), typeof(UIComponentRole));
            this.Type = (Type)info.GetValue(nameof(this.Type), typeof(Type));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Id), this.Id);
            info.AddValue(nameof(this.Slot), this.Slot);
            info.AddValue(nameof(this.Name), this.Name);
            info.AddValue(nameof(this.Description), this.Description);
            info.AddValue(nameof(this.Role), this.Role);
            info.AddValue(nameof(this.Type), this.Type);
        }

        #endregion
    }
}
