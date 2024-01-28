using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoxTunes
{
    public static class UIComponentLayoutProviderPresets
    {
        public static class Main
        {
            public const string A = "AAAA5819-A188-4B16-A0D9-825FC4150E5B";

            public const string B = "BBBB00AD-2D37-41EB-968F-86EE5C566C78";

            public const string C = "CCCCEA3C-E75C-48CE-B15D-B11873448DE2";

            public static readonly IEnumerable<Preset> Presets = new[]
            {
                new Preset(
                    A,
                    Strings.UIComponentLayoutProviderPresets_Main_1,
                    Resources.Main_1
                ),
                new Preset(
                    B,
                    Strings.UIComponentLayoutProviderPresets_Main_2,
                    Resources.Main_2
                ),
                new Preset(
                    C,
                    Strings.UIComponentLayoutProviderPresets_Main_3,
                    Resources.Main_3
                ),
            };
        }

        public static EventHandler Loader(string sectionId, string presetId, string layoutId, IEnumerable<Preset> presets)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var presetElement = configuration.GetElement<SelectionConfigurationElement>(
                sectionId,
                presetId
            );
            var layoutElement = configuration.GetElement<TextConfigurationElement>(
                sectionId,
                layoutId
            );
            return Loader(presetElement, layoutElement, presets);
        }

        public static EventHandler Loader(SelectionConfigurationElement presetElement, TextConfigurationElement layoutElement, IEnumerable<Preset> presets)
        {
            return (sender, e) =>
            {
                var preset = presets.FirstOrDefault(
                    _preset => string.Equals(_preset.Id, presetElement.Value.Id, StringComparison.OrdinalIgnoreCase)
                );
                if (preset == null)
                {
                    //The slected preset was not found.
                    return;
                }
                layoutElement.Value = preset.Layout;
            };
        }

        public class Preset
        {
            public Preset(string id, string name, string layout)
            {
                this.Id = id;
                this.Name = name;
                this.Layout = layout;
            }

            public string Id { get; private set; }

            public string Name { get; private set; }

            public string Layout { get; private set; }
        }
    }
}
