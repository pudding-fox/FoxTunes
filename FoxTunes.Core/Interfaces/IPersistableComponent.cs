namespace FoxTunes.Interfaces
{
    public interface IPersistableComponent<T> where T : IPersistableComponent<T>
    {
        T ToPersistable();
    }
}
