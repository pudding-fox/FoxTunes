using System;

namespace FoxTunes
{
    public class ScriptingException : Exception
    {
        public ScriptingException(int line, int startColumn, int endColumn, string message) : base(message)
        {
            this.Line = line;
            this.StartColumn = startColumn;
            this.EndColumn = endColumn;
        }

        public int Line { get; private set; }

        public int StartColumn { get; private set; }

        public int EndColumn { get; private set; }
    }
}
