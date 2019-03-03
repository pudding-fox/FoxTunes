using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Menu : ViewModelBase
    {
        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            "Category",
            typeof(string),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnCategoryChanged))
        );

        public static string GetCategory(Menu source)
        {
            return (string)source.GetValue(CategoryProperty);
        }

        public static void SetCategory(Menu source, string value)
        {
            source.SetValue(CategoryProperty, value);
        }

        public static void OnCategoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnCategoryChanged();
        }

        public static readonly DependencyProperty MenuVisibleProperty = DependencyProperty.Register(
            "MenuVisible",
            typeof(bool),
            typeof(Menu),
            new PropertyMetadata(new PropertyChangedCallback(OnMenuVisibleChanged))
        );

        public static bool GetMenuVisible(Menu source)
        {
            return (bool)source.GetValue(MenuVisibleProperty);
        }

        public static void SetMenuVisible(Menu source, bool value)
        {
            source.SetValue(MenuVisibleProperty, value);
        }

        public static void OnMenuVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var menu = sender as Menu;
            if (menu == null)
            {
                return;
            }
            menu.OnMenuVisibleChanged();
        }

        public Menu()
        {
            this.InvocableComponents = new ObservableCollection<IInvocableComponent>(ComponentRegistry.Instance.GetComponents<IInvocableComponent>());
            this.Items = new ObservableCollection<MenuItem>();
        }

        public string Category
        {
            get
            {
                return this.GetValue(CategoryProperty) as string;
            }
            set
            {
                this.SetValue(CategoryProperty, value);
            }
        }

        protected virtual void OnCategoryChanged()
        {
            if (this.CategoryChanged != null)
            {
                this.CategoryChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Category");
        }

        public event EventHandler CategoryChanged;

        private bool _MenuVisible { get; set; }

        public bool MenuVisible
        {
            get
            {
                return (bool)this.GetValue(MenuVisibleProperty);
            }
            set
            {
                this.SetValue(MenuVisibleProperty, value);
            }
        }

        protected virtual void OnMenuVisibleChanged()
        {
            if (this.MenuVisible)
            {
                this.Refresh();
            }
            if (this.MenuVisibleChanged != null)
            {
                this.MenuVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("MenuVisible");
        }

        public event EventHandler MenuVisibleChanged;

        public ObservableCollection<IInvocableComponent> InvocableComponents { get; set; }

        public ObservableCollection<MenuItem> Items { get; set; }

        protected virtual IEnumerable<MenuItem> GetItems()
        {
            foreach (var component in this.InvocableComponents)
            {
                foreach (var invocation in component.Invocations)
                {
                    if (!string.IsNullOrEmpty(this.Category) && !string.Equals(this.Category, invocation.Category, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var item = new MenuItem(component, invocation);
                    item.Core = this.Core;
                    yield return item;
                }
            }
        }

        protected virtual void Refresh()
        {
            this.Items.Clear();
            foreach (var item in this.GetItems().OrderBy(item => item.Invocation.Category).ThenBy(item => item.Invocation.Id))
            {
                if (item.Separator)
                {
                    this.Items.Add(null);
                }
                this.Items.Add(item);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Menu();
        }
    }

    public class MenuItem : ViewModelBase
    {
        private MenuItem()
        {

        }

        public MenuItem(IInvocableComponent component, IInvocationComponent invocation)
            : this()
        {
            this.Component = component;
            this.Invocation = invocation;
        }

        public IInvocableComponent Component { get; private set; }

        public IInvocationComponent Invocation { get; private set; }

        public ICommand Command
        {
            get
            {
                return CommandFactory.Instance.CreateCommand(
                    new Func<Task>(this.OnInvoke)
                );
            }
        }

        public bool Separator
        {
            get
            {
                return (this.Invocation.Attributes & InvocationComponent.ATTRIBUTE_SEPARATOR) == InvocationComponent.ATTRIBUTE_SEPARATOR;
            }
        }

        public bool Selected
        {
            get
            {
                return (this.Invocation.Attributes & InvocationComponent.ATTRIBUTE_SELECTED) == InvocationComponent.ATTRIBUTE_SELECTED;
            }
        }

        protected virtual Task OnInvoke()
        {
            return this.Component.InvokeAsync(this.Invocation);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new MenuItem();
        }
    }
}
