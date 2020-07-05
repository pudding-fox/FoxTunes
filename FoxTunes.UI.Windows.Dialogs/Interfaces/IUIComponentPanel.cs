namespace FoxTunes.Interfaces
{
    public interface IUIComponentPanel : IUIComponent
    {
        UIComponentConfiguration Component { get; set; }
    }
}
