using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FoxTunes.Functions
{
    [SQLiteFunction("EXECUTE_SCRIPT", -1, FunctionType.Scalar)]
    public class ExecuteScriptFunction : SQLiteFunction
    {
        public ExecuteScriptFunction()
        {

        }

        public static IScriptingContext ScriptingContext { get; set; }

        public override object Invoke(object[] args)
        {
            if (ScriptingContext == null)
            {
                ScriptingContext = StandardComponents.Instance.ScriptingRuntime.CreateContext();
            }
            var script = args[0] as string;
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            var metaData = new Dictionary<string, string>();
            for (var a = 1; a < args.Length; a += 2)
            {
                var name = (args[a] as string).ToLower();
                if (metaData.ContainsKey(name))
                {
                    //Not sure what to do. Doesn't happen often.
                    continue;
                }
                var value = args[a + 1] as string;
                metaData.Add(name, value);
            }
            ScriptingContext.SetValue("tag", metaData);
            return ScriptingContext.Run(script);
        }
    }
}
