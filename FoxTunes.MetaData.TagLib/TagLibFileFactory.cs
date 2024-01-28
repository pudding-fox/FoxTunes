using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using TagLib;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.MetaData)]
    public class TagLibFileFactory : StandardFactory, IConfigurableComponent
    {
        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement WindowsMedia { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.WindowsMedia = this.Configuration.GetElement<BooleanConfigurationElement>(
                MetaDataBehaviourConfiguration.SECTION,
                TagLibFileFactoryConfiguration.READ_WINDOWS_MEDIA_TAGS
             );
            base.InitializeComponent(core);
        }

        public File Create(string fileName)
        {
            var file = default(File);
            if (this.WindowsMedia.Value && string.Equals(fileName.GetExtension(), "m4a", StringComparison.OrdinalIgnoreCase))
            {
                file = new global::FoxTunes.Mpeg4.File(fileName);
            }
            else
            {
                file = File.Create(fileName);
            }
            return file;
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return TagLibFileFactoryConfiguration.GetConfigurationSections();
        }
    }
}
