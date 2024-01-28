using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes
{
    public class DatabaseQuery : IDatabaseQuery
    {
        public DatabaseQuery(string commandText, params string[] parameterNames)
        {
            this.CommandText = commandText;
            this.ParameterNames = parameterNames;
        }

        public string CommandText { get; private set; }

        public IEnumerable<string> ParameterNames { get; private set; }
    }
}
