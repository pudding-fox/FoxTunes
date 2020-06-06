using FoxTunes.Interfaces;
using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    public class CollectionManager<T> : Freezable, INotifyPropertyChanged where T : class
    {
        private Func<T> _ItemFactory { get; set; }

        public Func<T> ItemFactory
        {
            get
            {
                return this._ItemFactory;
            }
            set
            {
                this._ItemFactory = value;
                this.OnItemFactoryChanged();
            }
        }

        protected virtual void OnItemFactoryChanged()
        {
            if (this.ItemFactoryChanged != null)
            {
                this.ItemFactoryChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ItemFactory");
        }

        public event EventHandler ItemFactoryChanged;

        private Action<T, T> _ExchangeHandler { get; set; }

        public Action<T, T> ExchangeHandler
        {
            get
            {
                return this._ExchangeHandler;
            }
            set
            {
                this._ExchangeHandler = value;
                this.OnExchangeHandlerChanged();
            }
        }

        protected virtual void OnExchangeHandlerChanged()
        {
            if (this.ExchangeHandlerChanged != null)
            {
                this.ExchangeHandlerChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ExchangeHandler");
        }

        public event EventHandler ExchangeHandlerChanged;

        private IEnumerable<T> _ItemsSource { get; set; }

        public IEnumerable<T> ItemsSource
        {
            get
            {
                return this._ItemsSource;
            }
            set
            {
                this._ItemsSource = value;
                this.OnItemsSourceChanged();
            }
        }

        protected virtual void OnItemsSourceChanged()
        {
            if (this._ItemsSource != null && typeof(ISequenceableComponent).IsAssignableFrom(typeof(T)))
            {
                this.OrderedItemsSource = this._ItemsSource
                    .OfType<ISequenceableComponent>()
                    .OrderBy(element => element.Sequence)
                    .OfType<T>();
            }
            else
            {
                this.OrderedItemsSource = this._ItemsSource;
            }
            //TODO: This is sketchy because T is not properly equatable.
            //TODO: We need to always set SelectedValue to null and then try to "restore" it from the new ItemsSource.
            //TODO: If it isn't IPersistableComponent then there's not much we can do.
            if (this.SelectedValue is IPersistableComponent persistable)
            {
                if (this._ItemsSource != null && typeof(IPersistableComponent).IsAssignableFrom(typeof(T)))
                {
                    this.SelectedValue = default(T);
                    this.SelectedValue = this._ItemsSource
                        .OfType<IPersistableComponent>()
                        .FirstOrDefault(element => element.Id == persistable.Id) as T;
                }
            }
            else
            {
                this.SelectedValue = default(T);
            }
            if (this.SelectedValue == default(T))
            {
                this.SelectedValue = (this.OrderedItemsSource ?? this.ItemsSource).FirstOrDefault();
            }
            if (this.ItemsSourceChanged != null)
            {
                this.ItemsSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ItemsSource");
        }

        public event EventHandler ItemsSourceChanged;

        private IEnumerable<T> _OrderedItemsSource { get; set; }

        public IEnumerable<T> OrderedItemsSource
        {
            get
            {
                return this._OrderedItemsSource;
            }
            set
            {
                this._OrderedItemsSource = value;
                this.OnOrderedItemsSourceChanged();
            }
        }

        protected virtual void OnOrderedItemsSourceChanged()
        {
            if (this.OrderedItemsSourceChanged != null)
            {
                this.OrderedItemsSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("OrderedItemsSource");
        }

        public event EventHandler OrderedItemsSourceChanged;

        private T _SelectedValue { get; set; }

        public T SelectedValue
        {
            get
            {
                return this._SelectedValue;
            }
            set
            {
                this._SelectedValue = value;
                this.OnSelectedValueChanged();
            }
        }

        protected virtual void OnSelectedValueChanged()
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedValue");
        }

        public event EventHandler SelectedValueChanged;

        public ICommand AddCommand
        {
            get
            {
                return new Command(this.Add, () => this.CanAdd);
            }
        }

        public bool CanAdd
        {
            get
            {
                if (this.ItemFactory == null)
                {
                    return false;
                }
                var collection = this.ItemsSource as ICollection<T>;
                return collection != null && !collection.IsReadOnly;
            }
        }

        public void Add()
        {
            var collection = this.ItemsSource as ICollection<T>;
            var item = this.ItemFactory();
            if (item is ISequenceableComponent sequenceable)
            {
                sequenceable.Sequence = this.ItemsSource.Count();
            }
            collection.Add(item);
            this.Refresh();
            this.SelectedValue = item;
        }

        public ICommand RemoveCommand
        {
            get
            {
                return new Command(this.Remove, () => this.CanRemove);
            }
        }

        public bool CanRemove
        {
            get
            {
                var collection = this.ItemsSource as ICollection<T>;
                return collection != null && !collection.IsReadOnly && this.SelectedValue != null;
            }
        }

        public void Remove()
        {
            var collection = this.ItemsSource as ICollection<T>;
            collection.Remove(this.SelectedValue);
            if (collection.Count == 0 && this.CanAdd)
            {
                this.Add();
            }
            else
            {
                var sequence = 0;
                foreach (var element in collection.OfType<ISequenceableComponent>())
                {
                    element.Sequence = sequence++;
                }
                this.Refresh();
            }
        }

        public ICommand ExchangeCommand
        {
            get
            {
                return new Command<object[]>(items => this.Exchange(items), items => this.CanExchange(items));
            }
        }

        public bool CanExchange(object[] items)
        {
            if (this.ExchangeHandler == null)
            {
                return false;
            }
            if (items == null || items.Length != 2)
            {
                return false;
            }
            if (!(items[0] is T) || !(items[1] is T))
            {
                return false;
            }
            return true;
        }

        public void Exchange(object[] items)
        {
            this.ExchangeHandler((T)items[0], (T)items[1]);
            this.Refresh();
        }

        public void Refresh()
        {
            this.OnItemsSourceChanged();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override Freezable CreateInstanceCore()
        {
            return new CollectionManager<T>();
        }
    }
}
