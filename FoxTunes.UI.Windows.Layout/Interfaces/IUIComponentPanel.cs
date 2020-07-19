namespace FoxTunes.Interfaces
{
    public interface IUIComponentPanel : IInvocableComponent, IUIComponent
    {
        bool IsInDesignMode { get; set; }

        UIComponentConfiguration Component { get; set; }
    }
}
