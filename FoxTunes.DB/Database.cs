using FoxTunes.Interfaces;
using System.Data.Common;

namespace FoxTunes
{
    public abstract class Database : StandardComponent, IDatabase
    {
        public abstract DbConnection CreateConnection();

        public abstract IDatabaseContext CreateContext();
    }
}
