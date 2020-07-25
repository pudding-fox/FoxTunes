using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    [UIComponent("3451DAA4-C643-4CB2-8105-B441F0277559", "Stack Grid")]
    public class UIComponentStackContainer : UIComponentPanel
    {
        const string ADD = "AAAA";

        const string REMOVE = "BBBB";

        const string MOVE_UP = "CCCC";

        const string MOVE_DOWN = "DDDD";

        const string FILL = "EEEE";

        const string ALIGN_LEFT = "FFFF";

        const string ALIGN_RIGHT = "GGGG";

        const string ALIGN_TOP = "HHHH";

        const string ALIGN_BOTTOM = "IIII";

        new public const string HorizontalAlignment = nameof(global::System.Windows.HorizontalAlignment);

        new public const string VerticalAlignment = nameof(global::System.Windows.VerticalAlignment);

        public static readonly string Fill = Enum.GetName(typeof(global::System.Windows.HorizontalAlignment), global::System.Windows.HorizontalAlignment.Stretch);

        public static readonly string AlignLeft = Enum.GetName(typeof(global::System.Windows.HorizontalAlignment), global::System.Windows.HorizontalAlignment.Left);

        public static readonly string AlignRight = Enum.GetName(typeof(global::System.Windows.HorizontalAlignment), global::System.Windows.HorizontalAlignment.Right);

        public static readonly string AlignTop = Enum.GetName(typeof(global::System.Windows.VerticalAlignment), global::System.Windows.VerticalAlignment.Top);

        public static readonly string AlignBottom = Enum.GetName(typeof(global::System.Windows.VerticalAlignment), global::System.Windows.VerticalAlignment.Bottom);

        public UIComponentStackContainer()
        {
            this.Grid = new Grid();
            this.Content = this.Grid;
        }

        public Grid Grid { get; private set; }

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
            this.Grid.Children.Clear();
            this.Grid.ColumnDefinitions.Clear();
            if (this.Component.Children != null && this.Component.Children.Count > 0)
            {
                foreach (var component in this.Component.Children)
                {
                    var horizontalAlignment = default(string);
                    var verticalAlignment = default(string);
                    if (component == null || !component.TryGet(HorizontalAlignment, out horizontalAlignment))
                    {
                        horizontalAlignment = Fill;
                    }
                    if (component == null || !component.TryGet(VerticalAlignment, out verticalAlignment))
                    {
                        verticalAlignment = Fill;
                    }
                    this.AddComponent(component, horizontalAlignment, verticalAlignment);
                }
            }
            else
            {
                var component = new UIComponentConfiguration();
                this.AddComponent(component, Fill, Fill);
                this.Component.Children = new ObservableCollection<UIComponentConfiguration>()
                {
                    component
                };
            }
        }

        protected virtual void AddComponent(UIComponentConfiguration component, string horizontalAlignment, string verticalAlignment)
        {
            var container = new UIComponentContainer()
            {
                Component = component,
                HorizontalAlignment = (HorizontalAlignment)Enum.Parse(typeof(HorizontalAlignment), horizontalAlignment),
                VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), verticalAlignment),
            };
            //TODO: Don't like anonymous event handlers, they can't be unsubscribed.
            container.ComponentChanged += (sender, e) =>
            {
                this.UpdateComponent(component, container.Component);
            };
            this.Grid.Children.Add(container);
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
                break;
            }
        }

        protected virtual void OnComponentChanged(object sender, EventArgs e)
        {
            var container = sender as UIComponentContainer;
            if (container == null)
            {
                return;
            }
        }

        public override IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                var horizontalAlignment = default(string);
                var verticalAlignment = default(string);
                var container = UIComponentDesignerOverlay.Container;

                if (container != null && container.Component != null)
                {
                    if (!container.Component.TryGet(HorizontalAlignment, out horizontalAlignment))
                    {
                        horizontalAlignment = Fill;
                    }
                    if (!container.Component.TryGet(VerticalAlignment, out verticalAlignment))
                    {
                        verticalAlignment = Fill;
                    }
                }
                else
                {
                    //Don't know.
                }

                var isFill = string.Equals(horizontalAlignment, Fill, StringComparison.OrdinalIgnoreCase) && string.Equals(verticalAlignment, Fill, StringComparison.OrdinalIgnoreCase);

                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ADD,
                    "Add"
                    );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    REMOVE,
                    "Remove"
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    MOVE_UP,
                    "Move Up",
                    attributes: InvocationComponent.ATTRIBUTE_SEPARATOR
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    MOVE_DOWN,
                    "Move Down"
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    FILL,
                    "Fill",
                    attributes: (byte)((isFill ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ALIGN_LEFT,
                    "Align Left",
                    attributes: (byte)((string.Equals(horizontalAlignment, AlignLeft, StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ALIGN_RIGHT,
                    "Align Right",
                    attributes: string.Equals(horizontalAlignment, AlignRight, StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ALIGN_TOP,
                    "Align Top",
                    attributes: (byte)((string.Equals(verticalAlignment, AlignTop, StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE) | InvocationComponent.ATTRIBUTE_SEPARATOR)
                );
                yield return new InvocationComponent(
                    InvocationComponent.CATEGORY_GLOBAL,
                    ALIGN_BOTTOM,
                    "Align Bottom",
                    attributes: string.Equals(verticalAlignment, AlignBottom, StringComparison.OrdinalIgnoreCase) ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE
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
                    case MOVE_UP:
                        return this.MoveUp(container);
                    case MOVE_DOWN:
                        return this.MoveDown(container);
                    case FILL:
                        return this.SetFill(container);
                    case ALIGN_LEFT:
                        return this.SetHorizontalAlignment(container, AlignLeft);
                    case ALIGN_RIGHT:
                        return this.SetHorizontalAlignment(container, AlignRight);
                    case ALIGN_TOP:
                        return this.SetVerticalAlignment(container, AlignTop);
                    case ALIGN_BOTTOM:
                        return this.SetVerticalAlignment(container, AlignBottom);
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
            });
        }

        public Task MoveUp(UIComponentContainer container)
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
            });
        }

        public Task MoveDown(UIComponentContainer container)
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
            });
        }

        public Task SetFill(UIComponentContainer container)
        {
            return Windows.Invoke(() =>
            {
                if (container.Component == null)
                {
                    container.Component = new UIComponentConfiguration();
                }
                container.Component.AddOrUpdate(HorizontalAlignment, Fill);
                container.Component.AddOrUpdate(VerticalAlignment, Fill);
                this.UpdateChildren();
            });
        }

        public Task SetHorizontalAlignment(UIComponentContainer container, string alignment)
        {
            return Windows.Invoke(() =>
            {
                if (container.Component == null)
                {
                    container.Component = new UIComponentConfiguration();
                }
                container.Component.AddOrUpdate(HorizontalAlignment, alignment);
                this.UpdateChildren();
            });
        }

        public Task SetVerticalAlignment(UIComponentContainer container, string alignment)
        {
            return Windows.Invoke(() =>
            {
                if (container.Component == null)
                {
                    container.Component = new UIComponentConfiguration();
                }
                container.Component.AddOrUpdate(VerticalAlignment, alignment);
                this.UpdateChildren();
            });
        }
    }
}
