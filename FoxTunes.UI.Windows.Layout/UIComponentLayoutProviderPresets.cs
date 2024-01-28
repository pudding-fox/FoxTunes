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

            public const string D = "DDDD8DCA-65F9-4704-8AAC-17631219E70D";

            public const string E = "EEEE2131-1882-43BE-9FCC-AE0EECA67EFD";

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
                new Preset(
                    D,
                    Strings.UIComponentLayoutProviderPresets_Simple_1,
                    Resources.Simple_1
                ),
                new Preset(
                    E,
                    Strings.UIComponentLayoutProviderPresets_Simple_2,
                    Resources.Simple_2
                ),
            };
        }

        public static Preset GetPresetById(IEnumerable<Preset> presets, string id)
        {
            return presets.FirstOrDefault(preset => string.Equals(preset.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static Preset GetPresetByName(IEnumerable<Preset> presets, string name)
        {
            return presets.FirstOrDefault(preset => string.Equals(preset.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static Preset GetActivePreset(string sectionId, string presetId, string layoutId, IEnumerable<Preset> presets)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var presetElement = configuration.GetElement<SelectionConfigurationElement>(
                sectionId,
                presetId
            );
            if (presetElement.Value == null)
            {
                //No preset selected.
                return null;
            }
            var preset = GetPresetById(presets, presetElement.Value.Id);
            if (preset == null)
            {
                //The selected preset was not found.
                return null;
            }
            var layoutElement = configuration.GetElement<TextConfigurationElement>(
                sectionId,
                layoutId
            );
            if (!string.Equals(preset.Layout, layoutElement.Value, StringComparison.OrdinalIgnoreCase))
            {
                //The selected preset has been modified.
                return null;
            }
            return preset;
        }

        public static bool IsLoaded(string sectionId, string presetId, string layoutId, IEnumerable<Preset> presets, Preset preset)
        {
            var configuration = ComponentRegistry.Instance.GetComponent<IConfiguration>();
            var presetElement = configuration.GetElement<SelectionConfigurationElement>(
                sectionId,
                presetId
            );
            if (presetElement.Value == null || !string.Equals(presetElement.Value.Id, preset.Id, StringComparison.OrdinalIgnoreCase))
            {
                //Preset not selected.
                return false;
            }
            var layoutElement = configuration.GetElement<TextConfigurationElement>(
                sectionId,
                layoutId
            );
            if (!string.Equals(preset.Layout, layoutElement.Value, StringComparison.OrdinalIgnoreCase))
            {
                //The selected preset has been modified.
                return false;
            }
            return true;
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
                if (presetElement.Value == null)
                {
                    //No preset selected.
                    return;
                }
                var preset = GetPresetById(presets, presetElement.Value.Id);
                if (preset == null)
                {
                    //The selected preset was not found.
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
