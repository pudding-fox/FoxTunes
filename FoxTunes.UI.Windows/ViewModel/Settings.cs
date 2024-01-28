using FoxTunes.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FoxTunes.ViewModel
{
    public class Settings : ViewModelBase
    {
        public Settings()
        {
            this.Sections = new ObservableCollection<ConfigurationSection>();
        }

        public ObservableCollection<ConfigurationSection> Sections { get; private set; }

        public ISignalEmitter SignalEmitter { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private bool _SettingsVisible { get; set; }

        public bool SettingsVisible
        {
            get
            {
                return this._SettingsVisible;
            }
            set
            {
                this._SettingsVisible = value;
                this.OnSettingsVisibleChanged();
            }
        }

        protected virtual void OnSettingsVisibleChanged()
        {
            if (this.SettingsVisible)
            {
                this.SignalEmitter.Send(new Signal(this, CommonSignals.SettingsUpdated));
            }
            if (this.SettingsVisibleChanged != null)
            {
                this.SettingsVisibleChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SettingsVisible");
        }

        public event EventHandler SettingsVisibleChanged = delegate { };

        private ConfigurationSection _SelectedSection { get; set; }

        public ConfigurationSection SelectedSection
        {
            get
            {
                return this._SelectedSection;
            }
            set
            {
                if (object.ReferenceEquals(this._SelectedSection, value))
                {
                    return;
                }
                this.OnSelectedSectionChanged();
                this._SelectedSection = value;
                this.OnSelectedSectionChanged();
            }
        }

        protected virtual void OnSelectedSectionChanging()
        {
            if (this.SelectedSection != null)
            {
                this.SelectedSection.IsSelected = false;
            }
            if (this.SelectedSectionChanging != null)
            {
                this.SelectedSectionChanging(this, EventArgs.Empty);
            }
            this.OnPropertyChanging("SelectedSection");
        }

        public event EventHandler SelectedSectionChanging = delegate { };

        protected virtual void OnSelectedSectionChanged()
        {
            if (this.SelectedSection != null)
            {
                this.SelectedSection.IsSelected = true;
            }
            if (this.SelectedSectionChanged != null)
            {
                this.SelectedSectionChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("SelectedSection");
        }

        public event EventHandler SelectedSectionChanged = delegate { };

        public ICommand ShowCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = true);
            }
        }

        public ICommand HideCommand
        {
            get
            {
                return new Command(() => this.SettingsVisible = false);
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                var command = CommandFactory.Instance.CreateCommand(
#if NET40
                    () => TaskEx.Run(() => this.Configuration.Save()),
#else
                    () => Task.Run(() => this.Configuration.Save()),
#endif
                    () => this.Configuration != null
                );
                command.Tag = CommandHints.DISMISS;
                return command;
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.SignalEmitter = this.Core.Components.SignalEmitter;
            this.Configuration = this.Core.Components.Configuration;
            var task = this.Refresh();
            base.InitializeComponent(core);
        }

        protected virtual Task Refresh()
        {
            return Windows.Invoke(() =>
            {
                this.Sections.Clear();
                foreach (var section in this.Configuration.Sections.OrderBy(section => section.Id))
                {
                    this.Sections.Add(new ConfigurationSection(section));
                }
                this.SelectedSection = this.Sections.FirstOrDefault();
            });
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Settings();
        }
    }

    public class ConfigurationSection : ViewModelBase, ISelectable, IExpandable
    {
        private ConfigurationSection()
        {
            this.Elements = new ObservableCollection<ConfigurationElement>();
            this.Children = new ObservableCollection<ConfigurationSection>();
            this.IsExpanded = true;
        }

        public ConfigurationSection(global::FoxTunes.ConfigurationSection section, string path = null) : this()
        {
            this.Section = section;
            this.Path = path;
            if (string.IsNullOrEmpty(this.Path))
            {
                foreach (var element in this.Section.Elements.OrderBy(element => element.Id))
                {
                    this.AddElement(element);
                }
            }
        }

        public ObservableCollection<ConfigurationElement> Elements { get; private set; }

        public ObservableCollection<ConfigurationSection> Children { get; private set; }

        public global::FoxTunes.ConfigurationSection Section { get; private set; }

        public string Path { get; private set; }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.Path))
                {
                    return this.Section.Name;
                }
                return this.Path;
            }
        }

        private bool _IsExpanded { get; set; }

        public bool IsExpanded
        {
            get
            {
                return this._IsExpanded;
            }
            set
            {
                this._IsExpanded = value;
                this.OnIsExpandedChanged();
            }
        }

        protected virtual void OnIsExpandedChanged()
        {
            if (this.IsExpandedChanged != null)
            {
                this.IsExpandedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsExpanded");
        }

        public event EventHandler IsExpandedChanged = delegate { };

        private bool _IsSelected { get; set; }

        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                this.OnIsSelectedChanged();
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            if (this.IsSelectedChanged != null)
            {
                this.IsSelectedChanged(this, EventArgs.Empty);
            }
            this.OnPropertyChanged("IsSelected");
        }

        public event EventHandler IsSelectedChanged = delegate { };

        protected virtual void AddElement(ConfigurationElement element)
        {
            if (string.IsNullOrEmpty(element.Path))
            {
                this.Elements.Add(element);
                return;
            }
            var path = element.Path.Split(global::System.IO.Path.DirectorySeparatorChar, global::System.IO.Path.AltDirectorySeparatorChar);
            this.GetSection(path).Elements.Add(element);
        }

        protected virtual ConfigurationSection GetSection(params string[] path)
        {
            var section = this;
            foreach (var segment in path)
            {
                section = this.GetSection(section, segment);
            }
            return section;
        }

        protected virtual ConfigurationSection GetSection(ConfigurationSection section, string path)
        {
            foreach (var child in section.Children)
            {
                if (string.Equals(child.Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }
            {
                var child = new ConfigurationSection(this.Section, path);
                section.Children.Add(child);
                return child;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ConfigurationSection();
        }
    }
}
