using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace FoxTunes.ViewModel
{
    public class Menu : ViewModelBase
    {
        public Menu()
        {
            this.InvocableComponents = new ObservableCollection<IInvocableComponent>();
            this.Items = new ObservableCollection<MenuItem>();
        }

        public ObservableCollection<IInvocableComponent> InvocableComponents { get; set; }

        public ObservableCollection<MenuItem> Items { get; set; }

        protected virtual IEnumerable<MenuItem> GetItems()
        {
            foreach (var component in this.InvocableComponents)
            {
                foreach (var invocation in component.Invocations)
                {
                    var item = new MenuItem(component, invocation);
                    item.Core = this.Core;
                    yield return item;
                }
            }
        }

        protected override void OnCoreChanged()
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
            base.OnCoreChanged();
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

        public MenuItem(IInvocableComponent component, IInvocationComponent invocation) : this()
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

        protected override void OnCoreChanged()
        {
            this.BackgroundTaskRunner = this.Core.Components.BackgroundTaskRunner;
            base.OnCoreChanged();
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
