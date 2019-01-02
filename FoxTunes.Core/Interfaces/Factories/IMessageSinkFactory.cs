using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IMessageSinkFactory : IBaseFactory
    {
        IMessageSink Create(uint id);
    }
}
