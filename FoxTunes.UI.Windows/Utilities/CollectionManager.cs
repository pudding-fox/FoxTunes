using FoxTunes.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    public class CollectionManager<T> : Freezable, INotifyPropertyChanged
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

        public event EventHandler ItemFactoryChanged = delegate { };

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

        public event EventHandler ExchangeHandlerChanged = delegate { };

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
            if (this.ItemsSource != null)
            {
                this.SelectedValue = this.ItemsSource.FirstOrDefault();
            }
            else
            {
                this.SelectedValue = default(T);
            }
            if (this.ItemsSourceChanged != null)
            {
                this.ItemsSourceChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("ItemsSource");
        }

        public event EventHandler ItemsSourceChanged = delegate { };

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

        public event EventHandler SelectedValueChanged = delegate { };

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
            collection.Add(item);
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
            this.SelectedValue = default(T);
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
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected override Freezable CreateInstanceCore()
        {
            return new CollectionManager<T>();
        }
    }
}
