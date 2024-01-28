using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FoxTunes
{
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

        protected override void OnConfigurationChanged()
        {
            this.UpdateChildren();
            base.OnConfigurationChanged();
        }

        protected virtual void UpdateChildren()
        {
            var index = this.TabControl.SelectedIndex;
            this.TabControl.Items.Clear(UIDisposerFlags.All);
            if (this.Configuration.Children.Count > 0)
            {
                foreach (var component in this.Configuration.Children)
                {
                    this.AddComponent(component);
                }
                if (index >= 0 && this.TabControl.Items.Count > index)
                {
                    this.TabControl.SelectedIndex = index;
                }
                else
                {
                    this.TabControl.SelectedIndex = 0;
                }
            }
            else
            {
                var component = new UIComponentConfiguration();
                this.AddComponent(component);
                this.Configuration.Children.Add(component);
            }
        }

        protected virtual void AddComponent(UIComponentConfiguration component)
        {
            var container = new UIComponentContainer()
            {
                Configuration = component
            };
            //TODO: Don't like anonymous event handlers, they can't be unsubscribed.
            container.ConfigurationChanged += (sender, e) =>
            {
                this.UpdateComponent(component, container.Configuration);
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
            for (var a = 0; a < this.Configuration.Children.Count; a++)
            {
                if (!object.ReferenceEquals(this.Configuration.Children[a], originalComponent))
                {
                    continue;
                }
                this.Configuration.Children[a] = newComponent;
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
                this.Configuration.Children.Add(new UIComponentConfiguration());
                this.UpdateChildren();
            });
        }

        public Task Remove(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                for (var a = 0; a < this.Configuration.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Configuration.Children[a], container.Configuration))
                    {
                        continue;
                    }
                    this.Configuration.Children.RemoveAt(a);
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
                for (var a = 0; a < this.Configuration.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Configuration.Children[a], container.Configuration))
                    {
                        continue;
                    }
                    if (a > 0)
                    {
                        this.Configuration.Children[a] = this.Configuration.Children[a - 1];
                        this.Configuration.Children[a - 1] = container.Configuration;
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
                for (var a = 0; a < this.Configuration.Children.Count; a++)
                {
                    if (!object.ReferenceEquals(this.Configuration.Children[a], container.Configuration))
                    {
                        continue;
                    }
                    if (this.Configuration.Children.Count - 1 > a)
                    {
                        this.Configuration.Children[a] = this.Configuration.Children[a + 1];
                        this.Configuration.Children[a + 1] = container.Configuration;
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
                if (container.Configuration == null)
                {
                    container.Configuration = new UIComponentConfiguration();
                }
                if (!string.IsNullOrEmpty(header))
                {
                    container.Configuration.MetaData.AddOrUpdate(Header, header);
                }
                else
                {
                    container.Configuration.MetaData.TryRemove(Header);
                }
                this.UpdateChildren();
            });
        }

        public string GetHeader(UIComponentContainer container)
        {
            var header = default(string);
            if (container.Configuration.MetaData.TryGetValue(Header, out header) && !string.IsNullOrEmpty(header))
            {
                return header;
            }
            if (!container.Configuration.Component.IsEmpty)
            {
                return container.Configuration.Component.Name;
            }
            return Strings.UIComponentTabContainer_NewTab;
        }
    }
}
