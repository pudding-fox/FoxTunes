using System.Collections.Generic;

namespace FoxTunes
{
    public static class CSCoreOutputConfiguration
    {
        public const string OUTPUT_SECTION = "B60E0AFE-9B14-4919-A88E-F810F037FFA0";

        public const string BACKEND_ELEMENT = "88DAB8DB-E99A-44E9-B190-730F05A51B01";

        public const string DIRECT_SOUND_OPTION = "950D696C-CBFF-44DC-AEFD-20664C2F13D3";

        public const string WASAPI_OPTION = "C83680D5-D1F7-491E-9096-2BDF5BB1AF5C";

        public const string DECODER_ELEMENT = "3928EE4A-65A8-4ADA-B8B2-0C4D89C47027";

        public const string NATIVE_OPTION = "3AFF0EAE-FA4C-4D5C-AB3B-8C95BF4F4DA1";

        public const string FFMPEG_OPTION = "3D553CB8-53F6-43D2-9205-9BC18BC7D0F6";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(OUTPUT_SECTION, "Output")
                .WithElement(
                    new SelectionConfigurationElement(BACKEND_ELEMENT, "Backend")
                        .WithOption(new SelectionConfigurationOption(DIRECT_SOUND_OPTION, "DirectSound"), true)
                        .WithOption(new SelectionConfigurationOption(WASAPI_OPTION, "Wasapi")))
                .WithElement(
                    new SelectionConfigurationElement(DECODER_ELEMENT, "Decoder")
                        .WithOption(new SelectionConfigurationOption(NATIVE_OPTION, "Native"), true)
                        .WithOption(new SelectionConfigurationOption(FFMPEG_OPTION, "Ffmpeg"))
            );
        }
    }
}
