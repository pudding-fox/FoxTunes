namespace FoxTunes.Interfaces
{
    public interface IBaseComponent : IObservable
    {
        void InitializeComponent(ICore core);
    }
}
