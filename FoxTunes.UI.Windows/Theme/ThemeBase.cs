using System.Windows;

namespace FoxTunes.Theme
{
    public abstract class ThemeBase : StandardComponent, ITheme
    {
        protected ThemeBase(string id, string name = null, string description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public abstract void Apply(Application application);
    }
}
