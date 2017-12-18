namespace FoxTunes
{
    public static class SQLiteSyntax
    {
        public const string IDENTIFIER_FORMAT = "\"{0}\"";

        public const string STRING_FORMAT = "'{0}'";

        public static string Identifier(string identifier)
        {
            return string.Format(IDENTIFIER_FORMAT, identifier);
        }

        public static string Column(string table, string column)
        {
            return string.Format("{0}.{1}", Identifier(table), Identifier(column));
        }

        public static string String(string name)
        {
            return string.Format(STRING_FORMAT, name.Replace("'", "''"));
        }
    }
}
