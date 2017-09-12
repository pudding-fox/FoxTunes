using System.Collections.Generic;
using System.Data;

namespace FoxTunes
{
    public static partial class Extensions
    {
        public static IDbCommand CreateCommand(this IDbConnection connection, string commandText, IDictionary<string, object> parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            if (parameters != null)
            {
                foreach (var key in parameters.Keys)
                {
                    var value = parameters[key];
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = key;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                }
            }
            return command;
        }
    }
}
