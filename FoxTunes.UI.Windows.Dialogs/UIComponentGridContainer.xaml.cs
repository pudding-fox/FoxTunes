using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for UIComponentGridContainer.xaml
    /// </summary>
    [UIComponent("C1BD2AF7-ACF8-4710-99A3-AB5F34C46A90", UIComponentSlots.NONE, "Grid", role: UIComponentRole.Hidden)]
    public partial class UIComponentGridContainer : UIComponentPanel
    {
        const int MIN_WIDTH = 80;

        const string ADD = "AAAA";

        const string REMOVE = "BBBB";

        const string MOVE_LEFT = "CCCC";

        const string MOVE_RIGHT = "DDDD";

        const string ALIGN_LEFT = "EEEE";

        const string ALIGN_STRETCH = "FFFF";

        const string ALIGN_RIGHT = "GGGG";

        public const string Alignment = "Alignment";

        public const string AlignLeft = "Left";

        public const string AlignStretch = "Stretch";

        public const string AlignRight = "Right";

        public UIComponentGridContainer()
        {
            this.InitializeComponent();
        }

        protected override void OnComponentChanged()
        {
            if (this.Component != null)
            {
                this.UpdateChildren();
            }
            base.OnComponentChanged();
        }

        protected virtual IEnumerable<UIComponentConfiguration> GetComponents(string alignment)
        {
            return this.Component.Children.Where(component =>
            {
                var value = default(string);
                if (component.TryGet(Alignment, out value))
                {
                    return string.Equals(value, alignment, StringComparison.OrdinalIgnoreCase);
                }
                //Align left by default.
                return string.Equals(alignment, AlignLeft, StringComparison.OrdinalIgnoreCase);
            });
        }

        protected virtual void UpdateChildren()
        {
            this.Grid.Children.Clear();
            this.Grid.ColumnDefinitions.Clear();
            if (this.Component.Children != null && this.Component.Children.Count > 0)
            {
                this.AddLeft(this.GetComponents(AlignLeft));
                this.AddStretch(this.GetComponents(AlignStretch));
                this.AddRight(this.GetComponents(AlignRight));
            }
            else
            {
                var component = new UIComponentConfiguration();
                this.AddLeft(new[] { component });
                this.Component.Children = new ObservableCollection<UIComponentConfiguration>()
                {
                    component
                };
            }
        }

        protected virtual void AddLeft(IEnumerable<UIComponentConfiguration> components)
        {
            this.AddContainer(components, HorizontalAlignment.Left, new GridLength(0, GridUnitType.Auto));
        }

        protected virtual void AddStretch(IEnumerable<UIComponentConfiguration> components)
        {
            this.AddContainer(components, HorizontalAlignment.Stretch, new GridLength(1, GridUnitType.Star));
        }

        protected virtual void AddRight(IEnumerable<UIComponentConfiguration> components)
        {
            this.AddContainer(components, HorizontalAlignment.Right, new GridLength(0, GridUnitType.Auto));
        }

        protected virtual void AddContainer(IEnumerable<UIComponentConfiguration> components, HorizontalAlignment alignment, GridLength width)
        {
            if (!components.Any())
            {
                //Create an empty column so other things align correctly.
                this.Grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = width
                });
                return;
            }
            foreach (var component in components)
            {
                var margin = default(Thickness);
                if (this.Grid.ColumnDefinitions.Count > 0)
                {
                    margin = new Thickness(2, 0, 0, 0);
                }
                this.Grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = width
                });
                var container = new UIComponentContainer()
                {
                    Component = component,
                    MinWidth = MIN_WIDTH,
                    Margin = margin,
                    HorizontalAlignment = alignment,
                };
                //TODO: Don't like anonymous event handlers, they can't be unsubscribed.
                container.ComponentChanged += (sender, e) =>
                {
                    this.UpdateComponent(component, container.Component);
                };
                Grid.SetColumn(container, this.Grid.ColumnDefinitions.Count - 1);
                this.Grid.Children.Add(container);
            }
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
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, ADD, "Add");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, REMOVE, "Remove");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, MOVE_LEFT, "Move Left", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, MOVE_RIGHT, "Move Right");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, ALIGN_LEFT, "Align Left", attributes: InvocationComponent.ATTRIBUTE_SEPARATOR);
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, ALIGN_STRETCH, "Align Center");
                yield return new InvocationComponent(InvocationComponent.CATEGORY_GLOBAL, ALIGN_RIGHT, "Align Right");
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
                    case ALIGN_LEFT:
                        return this.SetAlignment(container, AlignLeft);
                    case ALIGN_STRETCH:
                        return this.SetAlignment(container, AlignStretch);
                    case ALIGN_RIGHT:
                        return this.SetAlignment(container, AlignRight);
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
            });
        }

        public Task SetAlignment(UIComponentContainer container, string alignment)
        {
            return Windows.Invoke(() =>
            {
                if (container.Component == null)
                {
                    container.Component = new UIComponentConfiguration();
                }
                container.Component.AddOrUpdate(Alignment, alignment);
                this.UpdateChildren();
            });
        }
    }
}
