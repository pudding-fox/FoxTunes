using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class BassSacdBehaviourConfiguration
    {
        public const string SECTION = "5A90BCBE-3F74-462B-9646-3C88EA8DC3C8";

        public const string ENABLED = "AAAA6B82-EE43-4B07-B764-690CE65E88DD";

        public const string AREA = "BBBB51AB-A920-436E-B3F5-9A54CECE7244";

        public static IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return Enumerable.Empty<ConfigurationSection>();
        }
    }
}
