namespace FoxTunes
{
    public static class CommonSignals
    {
        public const string LibraryUpdated = "LibraryUpdated";

        public const string HierarchiesUpdated = "HierarchiesUpdated";

        public const string PlaylistUpdated = "PlaylistUpdated";

        public const string PlaylistColumnsUpdated = "PlaylistColumnsUpdated";

        public const string MetaDataUpdated = "MetaDataUpdated";

        public const string SettingsUpdated = "SettingsUpdated";

        public const string ImagesUpdated = "ImagesUpdated";

        public const string StreamLoaded = "StreamLoaded";

        public const string StreamFault = "StreamFault";

        public const string PluginInvocation = "PluginInvocation";
    }

    public static class CommonSignalFlags
    {
        public const byte NONE = 0;

        public const byte SOFT = 1;
    }
}
