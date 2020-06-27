using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FoxTunes
{
    [Serializable]
    public class SelectionConfigurationElement : ConfigurationElement<SelectionConfigurationOption>
    {
        public SelectionConfigurationElement(string id, string name = null, string description = null, string path = null)
            : base(id, name, description, path)
        {
            this.Options = new ObservableCollection<SelectionConfigurationOption>();
        }

        public ObservableCollection<SelectionConfigurationOption> Options { get; set; }

        new public SelectionConfigurationOption DefaultValue
        {
            get
            {
                return base.DefaultValue as SelectionConfigurationOption;
            }
        }

        private bool Contains(string id)
        {
            return this.GetOption(id) != null;
        }

        private SelectionConfigurationOption GetOption(string optionId)
        {
            return this.Options.FirstOrDefault(option => string.Equals(option.Id, optionId, StringComparison.OrdinalIgnoreCase));
        }

        public SelectionConfigurationElement WithOptions(IEnumerable<SelectionConfigurationOption> options)
        {
            return this.WithOptions(options, false);
        }

        public SelectionConfigurationElement WithOptions(IEnumerable<SelectionConfigurationOption> options, bool clear)
        {
            var value = this.Value;
            if (clear)
            {
                this.Options.Clear();
            }
            foreach (var option in options)
            {
                this.Options.Add(option);
            }
            //If nothing is selected or the selection is no longer valid we try to select something.
            if (this.Value == null || !this.Contains(this.Value.Id))
            {
                if (value != null && this.Contains(value.Id))
                {
                    //The previous selection is valid, restore it.
                    this.Value = value;
                }
                else
                {
                    //Either no previous selection or it's invalid, select the first "default" option or just the first option if none are "default".
                    this.Value = this.Options.FirstOrDefault(option => option.IsDefault) ?? this.Options.FirstOrDefault();
                }
            }
            return this;
        }

        protected override void OnUpdate(ConfigurationElement element, bool create)
        {
            var other = element as SelectionConfigurationElement;
            if (other == null)
            {
                return;
            }
            if (create)
            {
                this.WithOptions(other.Options);
            }
            else
            {
                if (other.Value != null)
                {
                    var option = this.GetOption(other.Value.Id);
                    if (option == null)
                    {
                        return;
                    }
                    this.Value = option;
                }
            }
        }

        #region ISerializable

        public override bool IsPersistent
        {
            get
            {
                return true;
            }
        }

        protected SelectionConfigurationElement(SerializationInfo info, StreamingContext context) : base(info, context)
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
