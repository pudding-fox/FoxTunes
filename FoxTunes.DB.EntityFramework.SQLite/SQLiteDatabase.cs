using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace FoxTunes
{
    public class SQLiteDatabase : EntityFrameworkDatabase
    {
        private static readonly Type[] References = new[]
        {
            typeof(global::System.Data.SQLite.EF6.SQLiteProviderFactory),
            typeof(global::System.Data.SQLite.Linq.SQLiteProviderFactory)
        };

        private static readonly string DatabaseFileName = Path.Combine(
            Path.GetDirectoryName(typeof(SQLiteDatabase).Assembly.Location),
            "Database.dat"
        );

        static SQLiteDatabase()
        {
            StageInteropAssemblies();
        }

        private static void StageInteropAssemblies()
        {
            var assemblies = new[]{
                new
                {
                    DirectoryName = "x86",
                    FileName = "SQLite.Interop.dll",
                    Content = Resources.SQLite_Interop_x86
                },
                new
                {
                    DirectoryName = "x64",
                    FileName = "SQLite.Interop.dll",
                    Content = Resources.SQLite_Interop_x64
                }
            };
            var location = typeof(SQLiteDatabase).Assembly.Location;
            var baseDirectoryName = Path.GetDirectoryName(location);
            foreach (var assembly in assemblies)
            {
                var directoryName = Path.Combine(baseDirectoryName, assembly.DirectoryName);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                var fileName = Path.Combine(directoryName, assembly.FileName);
                if (!File.Exists(fileName))
                {
                    File.WriteAllBytes(fileName, assembly.Content);
                }
            }
        }

        protected virtual string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = DatabaseFileName;
                return builder.ToString();
            }
        }

        public override DbConnection CreateConnection()
        {
            if (!File.Exists(DatabaseFileName))
            {
                throw new FileNotFoundException("Failed to locate the database.", DatabaseFileName);
            }
            return new SQLiteConnection(this.ConnectionString);
        }
    }
}
