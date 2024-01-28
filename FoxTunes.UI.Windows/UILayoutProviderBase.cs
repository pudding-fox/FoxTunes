using FoxTunes.Interfaces;

namespace FoxTunes
{
    public abstract class UILayoutProviderBase : StandardComponent, IUILayoutProvider
    {
        public abstract string Id { get; }

        public override void InitializeComponent(ICore core)
        {
            LayoutManager.Instance.Register(this);
            base.InitializeComponent(core);
        }

        public abstract bool IsComponentActive(string id);

        public abstract UIComponentBase Load(UILayoutTemplate template);
    }
}
