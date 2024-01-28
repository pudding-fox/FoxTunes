using System;

namespace FoxTunes
{
    [Serializable]
    public class SelectionConfigurationOption : IEquatable<SelectionConfigurationOption>
    {
        public SelectionConfigurationOption(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public bool Equals(SelectionConfigurationOption other)
        {
            if (other == null)
            {
                return false;
            }
            return string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SelectionConfigurationOption);
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (!string.IsNullOrEmpty(this.Id))
            {
                hashCode += this.Id.GetHashCode();
            }
            return hashCode;
        }
    }
}
