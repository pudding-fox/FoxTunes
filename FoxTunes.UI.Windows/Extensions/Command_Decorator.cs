using FoxTunes.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes
{
    public static partial class CommandExtensions
    {
        public static readonly DependencyProperty DecoratorProperty = DependencyProperty.RegisterAttached(
            "Decorator",
            typeof(CommandDecorator),
            typeof(CommandExtensions),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDecoratorPropertyChanged))
        );

        public static CommandDecorator GetDecorator(DependencyObject source)
        {
            return (CommandDecorator)source.GetValue(DecoratorProperty);
        }

        public static void SetDecorator(DependencyObject source, CommandDecorator value)
        {
            source.SetValue(DecoratorProperty, value);
        }

        private static void OnDecoratorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                RemoveDecorator(e.OldValue as CommandDecorator);
            }
            if (e.NewValue != null)
            {
                AddDecorator(e.NewValue as CommandDecorator);
            }
        }

        private static void AddDecorator(CommandDecorator commandDecorator)
        {
            commandDecorator.Attach();
        }

        private static void RemoveDecorator(CommandDecorator commandDecorator)
        {
            commandDecorator.Detach();
        }
    }

    public class CommandDecorator : Freezable
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(CommandDecorator)
        );

        public static ICommand GetCommand(CommandDecorator source)
        {
            return (ICommand)source.GetValue(CommandProperty);
        }

        public static void SetCommand(CommandDecorator source, ICommand value)
        {
            source.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty PhaseProperty = DependencyProperty.Register(
            "Phase",
            typeof(CommandPhase),
            typeof(CommandDecorator)
        );

        public static CommandPhase GetPhase(CommandDecorator source)
        {
            return (CommandPhase)source.GetValue(PhaseProperty);
        }

        public static void SetPhase(CommandDecorator source, CommandPhase value)
        {
            source.SetValue(PhaseProperty, value);
        }

        public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
            "Tag",
            typeof(string),
            typeof(CommandDecorator)
        );

        public static string GetTag(CommandDecorator source)
        {
            return (string)source.GetValue(TagProperty);
        }

        public static void SetTag(CommandDecorator source, string value)
        {
            source.SetValue(TagProperty, value);
        }

        public void Attach()
        {
            CommandBase.Phase += this.OnPhase;
        }

        public void Detach()
        {
            CommandBase.Phase -= this.OnPhase;
        }

        protected virtual void OnPhase(object sender, CommandPhaseEventArgs e)
        {
            Windows.Invoke(() =>
            {
                var tag = GetTag(this);
                if (!string.Equals(tag, e.Tag, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                var phase = GetPhase(this);
                if (!phase.HasFlag(e.Phase))
                {
                    return;
                }
                var command = GetCommand(this);
                if (object.ReferenceEquals(command, sender))
                {
                    return;
                }
                command.Execute(e.Parameter);
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CommandDecorator();
        }
    }
}
