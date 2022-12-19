using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FoxTunes
{
    //TODO: This control causes freezes on XP when visualizations (specifically WriteableBitmaps) are hosted in it.
    //TODO: For this reason, it is disabled.
    [PlatformDependency(Major = 6, Minor = 0)]
    [UIComponent("67A0F63C-DC86-4B4E-91E1-290B71822853", role: UIComponentRole.Container)]
    public class UIComponentTabContainer : UIComponentPanel
    {
        const string ADD = "AAAA";

        const string REMOVE = "BBBB";

        const string MOVE_LEFT = "CCCC";

        const string MOVE_RIGHT = "DDDD";

        const string RENAME = "EEEE";

        public const string Header = nameof(global::System.Windows.Controls.TabItem.Header);

        public UIComponentTabContainer()
        {
            this.TabControl = new global::System.Windows.Controls.TabControl()
            {
                AllowDrop = true
            };
            TabControlExtensions.SetDragOverSelection(this.TabControl, true);
            this.Content = this.TabControl;
        }

        public global::System.Windows.Controls.TabControl TabControl { get; private set; }

        protected override void OnComponentChanged()
        {
            if (this.Component != null)
            {
                this.UpdateChildren();
            }
            base.OnComponentChanged();
        }

        protected virtual void UpdateChildren()
        {
            var index = this.TabControl.SelectedIndex;
            this.TabControl.Items.Clear(UIDisposerFlags.All);
            if (this.Component.Children != null && this.Component.Children.Count > 0)
            {
                foreach (var component in this.Component.Children)
                {
                    this.AddComponent(component);
                }
                if (index > 0 && this.TabControl.Items.Count > index)
                {
                    this.TabControl.SelectedIndex = index;
                }
            }
            else
            {
                var component = new UIComponentConfiguration();
                this.AddComponent(component);
                this.Component.Children = new ObservableCollection<UIComponentConfiguration>()
                {
                    component
                };
            }
        }

        protected virtual void AddComponent(UIComponentConfiguration component)
        {
            var container = new UIComponentContainer()
            {
                Component = component
            };
            //TODO: Don't like anonymous event handlers, they can't be unsubscribed.
            container.ComponentChanged += (sender, e) =>
            {
                this.UpdateComponent(component, container.Component);
            };
            var item = new TabItem()
            {
                Header = this.GetHeader(container),
                Content = container
            };
            this.TabControl.Items.Add(item);
        }

        protected virtual void UpdateComponent(UIComponentConfiguration originalComponent, UIComponentConfiguration newComponent)
        {
            for (var a = 0; a < this.Component.Children.Count; a++)
            {
                if (!object.ReferenceEquals(this.Component.Children[a], originalComponent))
                {
                    continue;
                }
                this.Component.Children[a] = newComponent;
                this.UpdateChildren();
                return;
            }
            //TODO: Component was not found.
            throw new NotImplementedException();
        }

        protected virtual void OnComponentChanged(object sender, EventArgs e)
        {
            var container = sender as UIComponentContainer;
            if (container == null)
            {
                return;
            }
        }

        public override IEnumerable<string> InvocationCategories
        {
            get
            {
                yield return InvocationComponent.CATEGORY_GLOBAL;
            }
        }

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ADD,
                    Strings.UIComponentTabContainer_Add
                    );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    REMOVE,
                    Strings.UIComponentTabContainer_Remove
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    MOVE_LEFT,
                    Strings.UIComponentTabContainer_MoveLeft,
                    attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    MOVE_RIGHT,
                    Strings.UIComponentTabContainer_MoveRight
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    RENAME,
                    Strings.UIComponentTabContainer_Rename,
                    attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                );
            }
        }

        public override Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case ADD:
                    return this.Add();
            }
            if (component.Source is UIComponentContainer container)
            {
                switch (component.Id)
                {
                    case REMOVE:
                        return this.Remove(container);
                    case MOVE_LEFT:
                        return this.MoveLeft(container);
                    case MOVE_RIGHT:
                        return this.MoveRight(container);
                    case RENAME:
                        return this.Rename(container);
                }
            }
            return base.InvokeAsync(component);
        }

        public Task Add()
        {
            return Windows.Invoke(() =>
            {
                this.Component.Children.Add(new UIComponentConfiguration());
                this.UpdateChildren();
            });
        }

        public Task Remove(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                for (var a = 0; a < this.Component.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Component.Children[a], container.Component))
                    {
                        continue;
                    }
                    this.Component.Children.RemoveAt(a);
                    this.UpdateChildren();
                    return;
                }
                //TODO: Component was not found.
                throw new NotImplementedException();
            });
        }

        public Task MoveLeft(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                for (var a = 0; a < this.Component.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Component.Children[a], container.Component))
                    {
                        continue;
                    }
                    if (a > 0)
                    {
                        this.Component.Children[a] = this.Component.Children[a - 1];
                        this.Component.Children[a - 1] = container.Component;
                        this.UpdateChildren();
                    }
                    return;
                }
                //TODO: Component was not found.
                throw new NotImplementedException();
            });
        }

        public Task MoveRight(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                for (var a = 0; a < this.Component.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Component.Children[a], container.Component))
                    {
                        continue;
                    }
                    if (this.Component.Children.Count - 1 > a)
                    {
                        this.Component.Children[a] = this.Component.Children[a + 1];
                        this.Component.Children[a + 1] = container.Component;
                        this.UpdateChildren();
                    }
                    return;
                }
                //TODO: Component was not found.
                throw new NotImplementedException();
            });
        }

        public Task Rename(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                var header = InputBox.ShowDialog(Strings.UIComponentTabContainer_Rename, this.GetHeader(container));
                if (container.Component == null)
                {
                    container.Component = new UIComponentConfiguration();
                }
                if (string.IsNullOrEmpty(header))
                {
                    container.Component.MetaData.AddOrUpdate(Header, header);
                }
                else
                {
                    container.Component.MetaData.TryRemove(Header);
                }
                this.UpdateChildren();
            });
        }

        public static readonly UIComponentFactory Factory = ComponentRegistry.Instance.GetComponent<UIComponentFactory>();

        public string GetHeader(UIComponentContainer container)
        {
            if (container.Component != null && !string.IsNullOrEmpty(container.Component.Component))
            {
                var header = default(string);
                if (container.Component.MetaData.TryGetValue(Header, out header) && !string.IsNullOrEmpty(header))
                {
                    return header;
                }
                //TODO: This is really bad, creating a component (FrameworkElement) just to read a property?
                var component = Factory.CreateComponent(container.Component);
                if (component != null)
                {
                    return component.Name;
                }
            }
            return Strings.UIComponentTabContainer_NewTab;
        }
    }
}
