using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    [Component("ECF542D9-ABB3-4E82-8045-7E13F1727695", ComponentSlots.Database)]
    public class SqlServerDatabaseFactory : DatabaseFactory
    {
        protected override IDatabaseComponent OnCreate()
        {
            throw new NotImplementedException();
        }
    }
}
