using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Output)]
    public class PlaylistTracksLibraryBehaviour : StandardBehaviour, IConfigurableComponent
    {
        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryManager LibraryManager { get; private set; }

        public IConfiguration Configuration { get; private set; }

        private bool _Enabled { get; set; }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }
            set
            {
                this._Enabled = value;
                this.OnEnabledChanged();
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (this.Enabled)
            {
                this.Enable();
            }
            else
            {
                this.Disable();
            }
        }

        public override void InitializeComponent(ICore core)
        {
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryManager = core.Managers.Library;
            this.Configuration = core.Components.Configuration;
            this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistTracksLibraryBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        protected virtual void Enable()
        {
            this.LibraryManager.SelectedItemChanged += this.OnSelectedItemChanged;
        }

        protected virtual void Disable()
        {
            this.LibraryManager.SelectedItemChanged -= this.OnSelectedItemChanged;
        }

        protected virtual void OnSelectedItemChanged(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                if (this.LibraryManager.SelectedItem != null && !LibraryHierarchyNode.Empty.Equals(this.LibraryManager.SelectedItem))
                {
                    await this.PlaylistManager.Add(this.LibraryManager.SelectedItem, true).ConfigureAwait(false);
                }
            });
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistTracksLibraryBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
