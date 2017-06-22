using System.Collections.Generic;

namespace FoxTunes.Interfaces
{
    public interface IBehaviourLoader
    {
        IEnumerable<IBaseBehaviour> Load();
    }
}
