using FoxTunes.Interfaces;
using System;
using System.Windows;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            this.InitializeComponent();
        }

        public ICore Core
        {
            get
            {
                return this.DataContext as ICore;
            }
        }

        public Log Log { get; private set; }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Log = new Log() { DataContext = this.Core };
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            this.Log.PreventClose = false;
            this.Log.Close();
        }
    }
}
