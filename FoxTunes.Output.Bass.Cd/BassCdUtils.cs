using System;

namespace FoxTunes
{
    public static class BassCdUtils
    {
        public const string SCHEME = "cda";

        public static string CreateUrl(int drive, string id, int track)
        {
            return string.Format("{0}://{1}/{2}/{3}", SCHEME, id, drive, track);
        }

        public static bool ParseUrl(string url, out int drive, out string id, out int track)
        {
            return ParseUrl(new Uri(url), out drive, out id, out track);
        }

        public static bool ParseUrl(Uri url, out int drive, out string id, out int track)
        {
            drive = default(int);
            id = default(string);
            track = default(int);
            if (!string.Equals(url.Scheme, SCHEME, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            var host = url.GetComponents(UriComponents.Host, UriFormat.Unescaped);
            var path = url.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var parts = path.Split('/');
            return
                !string.IsNullOrEmpty(id = host) &&
                parts.Length > 0 && int.TryParse(parts[0], out drive) &&
                parts.Length > 1 && int.TryParse(parts[1], out track);
        }
    }
}
