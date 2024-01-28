using FoxTunes.Interfaces;
using System;

namespace FoxTunes
{
    public abstract class UILayoutProviderBase : StandardComponent, IUILayoutProvider
    {
        public abstract string Id { get; }

        public bool Active
        {
            get
            {
                return object.ReferenceEquals(LayoutManager.Instance.Provider, this);
            }
        }

        public override void InitializeComponent(ICore core)
        {
            LayoutManager.Instance.Register(this);
            base.InitializeComponent(core);
        }

        public abstract bool IsComponentActive(string id);

        public abstract UIComponentBase Load(UILayoutTemplate template);

        protected virtual void OnUpdated()
        {
            if (this.Updated == null)
            {
                return;
            }
            this.Updated(this, EventArgs.Empty);
        }

        public event EventHandler Updated;
    }
}
