namespace FoxTunes.Interfaces
{
    public interface IUIComponentPanel : IInvocableComponent, IUIComponent
    {
        UIComponentConfiguration Component { get; set; }
    }
}
