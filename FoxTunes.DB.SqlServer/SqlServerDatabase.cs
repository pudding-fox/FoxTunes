using FoxDb;
using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace FoxTunes
{
    public class SqlServerDatabase : Database
    {
        const string CONNECTION_STRING = "FoxTunes";

        public SqlServerDatabase()
            : base(GetProvider())
        {

        }

        private static Lazy<string> _ConnectionString = new Lazy<string>(() =>
        {
            var connectionString = ConfigurationManager.ConnectionStrings[CONNECTION_STRING];
            if (connectionString != null)
            {
                return connectionString.ConnectionString;
            }
            CreateConnectionString();
            var userInterface = ComponentRegistry.Instance.GetComponent<IUserInterface>();
            if (userInterface != null)
            {
                userInterface.Warn(string.Format("No connection string found.\nEdit {0} to resolve the create one.", ComponentResolver.FILE_NAME.GetName()));
            }
            throw new InvalidOperationException("No connection string specified.");
        });

        private static void CreateConnectionString()
        {
            var document = XDocument.Load(ComponentResolver.FILE_NAME);
            var connectionStrings = document.Root.Element("connectionStrings");
            if (connectionStrings == null)
            {
                connectionStrings = new XElement("connectionStrings");
                document.Root.AddFirst(connectionStrings);
            }
            if (connectionStrings.ToString().Contains(CONNECTION_STRING))
            {
                return;
            }
            connectionStrings.Add(new XComment("Uncomment the next element to use the default connection string."));
            connectionStrings.Add(new XComment(new XElement("add", new XAttribute("name", CONNECTION_STRING), new XAttribute("connectionString", "Data Source=localhost;Integrated Security=true;Initial Catalog=FoxTunes")).ToString()));
            document.Save(ComponentResolver.FILE_NAME);
        }

        public static string ConnectionString
        {
            get
            {
                return _ConnectionString.Value;
            }
        }

        public override IsolationLevel PreferredIsolationLevel
        {
            get
            {
                return IsolationLevel.ReadUncommitted;
            }
        }

        protected override IDatabaseQueries CreateQueries()
        {
            return new SqlServerDatabaseQueries(this);
        }

        private static IProvider GetProvider()
        {
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            return new SqlServer2012Provider(builder);
        }
    }
}
