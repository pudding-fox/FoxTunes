using System;
using System.Collections.Generic;

namespace FoxTunes
{
    public static class MiniPlayerBehaviourConfiguration
    {
        public const string SECTION = "F3E58830-97C0-4BA2-9E07-3EC27E3D4418";

        public const string NOW_PLAYING_SCRIPT_ELEMENT = "BBBB78F3-B32F-4A8C-B566-9A8B39A896C7";

        public const string TOPMOST_ELEMENT = "CCCC7F71-A506-48DF-9420-E6926465FFDC";

        public const string SHOW_ARTWORK_ELEMENT = "FFFFB3A4-E59D-46CE-B80A-86EAB9427108";

        public const string SHOW_PLAYLIST_ELEMENT = "GGGG60A9-B108-49DD-9D9D-EA608BBDB0E7";

        public const string PLAYLIST_SCRIPT_ELEMENT = "HHHHD917-2172-421D-9E22-F549B17CE0C8";

        public const string DROP_COMMIT_ELEMENT = "IIIIF490-CE3D-481A-8924-B698BD443D88";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            yield return new ConfigurationSection(SECTION, Strings.MiniPlayerBehaviourConfiguration_Section)
                .WithElement(
                    new TextConfigurationElement(NOW_PLAYING_SCRIPT_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_NowPlayingScript, path: Strings.MiniPlayerBehaviourConfiguration_Advanced).WithValue(Resources.NowPlaying).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(
                    new TextConfigurationElement(PLAYLIST_SCRIPT_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_PlaylistScript, path: Strings.MiniPlayerBehaviourConfiguration_Advanced).WithValue(Resources.Playlist).WithFlags(ConfigurationElementFlags.MultiLine))
                .WithElement(
                    new BooleanConfigurationElement(TOPMOST_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_Topmost).WithValue(false))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_ARTWORK_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_ShowArtwork).WithValue(true))
                .WithElement(
                    new BooleanConfigurationElement(SHOW_PLAYLIST_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_ShowPlaylist).WithValue(false))
                .WithElement(
                    new SelectionConfigurationElement(DROP_COMMIT_ELEMENT, Strings.MiniPlayerBehaviourConfiguration_DropCommit).WithOptions(GetDropBehaviourOptions())
            );
        }

        private static IEnumerable<SelectionConfigurationOption> GetDropBehaviourOptions()
        {
            yield return new SelectionConfigurationOption(
                Enum.GetName(typeof(MiniPlayerDropBehaviour), MiniPlayerDropBehaviour.Append),
                Strings.MiniPlayerDropBehaviour_Append
            );
            yield return new SelectionConfigurationOption(
                Enum.GetName(typeof(MiniPlayerDropBehaviour), MiniPlayerDropBehaviour.AppendAndPlay),
                Strings.MiniPlayerDropBehaviour_AppendAndPlay
            );
            yield return new SelectionConfigurationOption(
                Enum.GetName(typeof(MiniPlayerDropBehaviour), MiniPlayerDropBehaviour.Replace),
                Strings.MiniPlayerDropBehaviour_Replace
            );
            yield return new SelectionConfigurationOption(
                Enum.GetName(typeof(MiniPlayerDropBehaviour), MiniPlayerDropBehaviour.ReplaceAndPlay),
                Strings.MiniPlayerDropBehaviour_ReplaceAndPlay
            ).Default();
        }

        public static MiniPlayerDropBehaviour GetDropBehaviour(SelectionConfigurationOption option)
        {
            var result = default(MiniPlayerDropBehaviour);
            if (Enum.TryParse<MiniPlayerDropBehaviour>(option.Id, out result))
            {
                return result;
            }
            return MiniPlayerDropBehaviour.Replace;
        }
    }

    public enum MiniPlayerDropBehaviour : byte
    {
        None = 0,
        Append = 1,
        AppendAndPlay = 2,
        Replace = 3,
        ReplaceAndPlay = 4
    }
}
