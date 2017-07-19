using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace FoxTunes
{
    internal class InternalDbContext : DbContext
    {
        static InternalDbContext()
        {
            global::System.Data.Entity.Database.SetInitializer<InternalDbContext>(new NullDatabaseInitializer<InternalDbContext>());
        }

        public InternalDbContext(DbConnection connection, DbCompiledModel model)
            : base(connection, model, false)
        {

        }
    }
}
