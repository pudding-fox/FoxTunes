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
        public Menu()
        {
            this.InvocableComponents = new ObservableCollection<IInvocableComponent>();
            this.Items = new ObservableCollection<MenuItem>();
        }

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

        public event EventHandler CategoryChanged = delegate { };

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

        public override void InitializeComponent(ICore core)
        {
            this.InvocableComponents.AddRange(ComponentRegistry.Instance.GetComponents<IInvocableComponent>());
            foreach (var item in this.GetItems().OrderBy(item => item.Invocation.Category).ThenBy(item => item.Invocation.Id))
            {
                if (item.Separator)
                {
                    this.Items.Add(null);
                }
                this.Items.Add(item);
            }
            base.InitializeComponent(core);
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

        public IBackgroundTaskRunner BackgroundTaskRunner { get; private set; }

        public ICommand Command
        {
            get
            {
                return new AsyncCommand(this.BackgroundTaskRunner, this.OnInvoke);
            }
        }

        public bool Separator
        {
            get
            {
                return (this.Invocation.Attributes & InvocationComponent.ATTRIBUTE_SEPARATOR) == InvocationComponent.ATTRIBUTE_SEPARATOR;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            base.InitializeComponent(core);
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
