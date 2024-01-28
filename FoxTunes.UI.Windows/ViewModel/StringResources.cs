namespace FoxTunes.ViewModel
{
    /// <summary>
    /// This class just exposes the internal properties of <see cref="Strings"/> so we don't need to use PublicResXFileCodeGenerator.
    /// PublicResXFileCodeGenerator causes many type import conflict warnings.
    /// </summary>
    public static class StringResources
    {
        public static string EqualizerWindow_Title
        {
            get
            {
                return Strings.EqualizerWindow_Title;
            }
        }

        public static string General_Cancel
        {
            get
            {
                return Strings.General_Cancel;
            }
        }

        public static string General_OK
        {
            get
            {
                return Strings.General_OK;
            }
        }

        public static string General_Search
        {
            get
            {
                return Strings.General_Search;
            }
        }

        public static string SettingsDialog_EmptyPage
        {
            get
            {
                return Strings.SettingsDialog_EmptyPage;
            }
        }

        public static string SettingsDialog_GroupHeader
        {
            get
            {
                return Strings.SettingsDialog_GroupHeader;
            }
        }

        public static string SettingsDialog_ResetAll
        {
            get
            {
                return Strings.SettingsDialog_ResetAll;
            }
        }

        public static string SettingsDialog_ResetPage
        {
            get
            {
                return Strings.SettingsDialog_ResetPage;
            }
        }

        public static string SettingsDialog_Save
        {
            get
            {
                return Strings.SettingsDialog_Save;
            }
        }

        public static string SettingsWindow_Title
        {
            get
            {
                return Strings.SettingsWindow_Title;
            }
        }
    }
}
