using System.Threading.Tasks;

namespace FoxTunes.Interfaces
{
    public interface IActionable
    {
        string Description { get; }

        Task<bool> Task { get; }
    }
}
