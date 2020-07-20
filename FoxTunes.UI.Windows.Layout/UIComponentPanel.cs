using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FoxTunes
{
    public abstract class UIComponentPanel : UIComponentBase, IUIComponentPanel
    {
        public static readonly DependencyProperty IsInDesignModeProperty = DependencyProperty.Register(
            "IsInDesignMode",
            typeof(bool),
            typeof(UIComponentPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnIsInDesignModeChanged))
        );

        public static bool GetIsInDesignMode(UIComponentPanel source)
        {
            return (bool)source.GetValue(IsInDesignModeProperty);
        }

        public static void SetIsInDesignMode(UIComponentPanel source, bool value)
        {
            source.SetValue(IsInDesignModeProperty, value);
        }

        public static void OnIsInDesignModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentPanel;
            if (container == null)
            {
                return;
            }
            container.OnIsInDesignModeChanged();
        }

        public bool IsInDesignMode
        {
            get
            {
                return (bool)this.GetValue(IsInDesignModeProperty);
            }
            set
            {
                this.SetValue(IsInDesignModeProperty, value);
            }
        }

        protected virtual void OnIsInDesignModeChanged()
        {
            if (this.IsInDesignModeChanged != null)
            {
                this.IsInDesignModeChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsInDesignMode");
        }

        public event EventHandler IsInDesignModeChanged;

        public static readonly DependencyProperty ComponentProperty = DependencyProperty.Register(
            "Component",
            typeof(UIComponentConfiguration),
            typeof(UIComponentPanel),
            new PropertyMetadata(new PropertyChangedCallback(OnComponentChanged))
        );

        public static UIComponentConfiguration GetComponent(UIComponentPanel source)
        {
            return (UIComponentConfiguration)source.GetValue(ComponentProperty);
        }

        public static void SetComponent(UIComponentPanel source, UIComponentConfiguration value)
        {
            source.SetValue(ComponentProperty, value);
        }

        public static void OnComponentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var container = sender as UIComponentPanel;
            if (container == null)
            {
                return;
            }
            container.OnComponentChanged();
        }

        public UIComponentConfiguration Component
        {
            get
            {
                return this.GetValue(ComponentProperty) as UIComponentConfiguration;
            }
            set
            {
                this.SetValue(ComponentProperty, value);
            }
        }

        protected virtual void OnComponentChanged()
        {
            if (this.ComponentChanged != null)
            {
                this.ComponentChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("Component");
        }

        public event EventHandler ComponentChanged;

        protected UIComponentPanel()
        {
            this.CreateBindings();
        }

        protected virtual void CreateBindings()
        {
            this.SetBinding(
                IsInDesignModeProperty,
                new Binding()
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UIComponentPanel), 1),
                    Path = new PropertyPath(nameof(this.IsInDesignMode))
                }
            );
        }

        public virtual IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                return Enumerable.Empty<IInvocationComponent>();
            }
        }

        public virtual Task InvokeAsync(IInvocationComponent component)
        {
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
