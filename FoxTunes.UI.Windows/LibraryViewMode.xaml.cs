using FoxTunes.Interfaces;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FoxTunes
{
    /// <summary>
    /// Interaction logic for LibraryViewMode.xaml
    /// </summary>
    public partial class LibraryViewMode : UserControl
    {
        public static readonly DependencyProperty SelectedLibraryHierarchyProperty = DependencyProperty.Register(
            "SelectedLibraryHierarchy",
            typeof(LibraryHierarchy),
            typeof(LibraryViewMode),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

        public LibraryHierarchy GetSelectedLibraryHierarchy(LibraryViewMode owner)
        {
            return (LibraryHierarchy)owner.GetValue(SelectedLibraryHierarchyProperty);
        }

        public void SetSelectedLibraryHierarchy(LibraryViewMode owner, LibraryHierarchy value)
        {
            owner.SetValue(SelectedLibraryHierarchyProperty, value);
        }

        public LibraryViewMode()
        {
            InitializeComponent();
        }

        public LibraryHierarchy SelectedLibraryHierarchy
        {
            get
            {
                return GetSelectedLibraryHierarchy(this);
            }
            set
            {
                SetSelectedLibraryHierarchy(this, value);
            }
        }

        public ICore Core
        {
            get
            {
                return this.DataContext as ICore;
            }
        }

        protected virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Core == null)
            {
                return;
            }
            this.SelectedLibraryHierarchy = this.Core.Managers.Data.ReadContext.Queries.LibraryHierarchy.FirstOrDefault();
        }
    }
}
