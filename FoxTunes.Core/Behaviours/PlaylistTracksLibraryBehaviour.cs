using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
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
                PlaylistTracksLibraryBehaviourConfiguration.SECTION,
                PlaylistTracksLibraryBehaviourConfiguration.ENABLED_ELEMENT
            ).ConnectValue(value => this.Enabled = value);
            base.InitializeComponent(core);
        }

        protected virtual void Enable()
        {
            this.LibraryManager.SelectedNodeChanged += this.OnSelectedNodeChanged;
        }

        protected virtual void Disable()
        {
            this.LibraryManager.SelectedNodeChanged -= this.OnSelectedNodeChanged;
        }

        protected virtual void OnSelectedNodeChanged(object sender, EventArgs e)
        {
#if NET40
            var task = TaskEx.Run(async () =>
#else
            var task = Task.Run(async () =>
#endif
            {
                if (this.LibraryManager.SelectedNode != null)
                {
                    await this.PlaylistManager.Add(this.LibraryManager.SelectedNode, true);
                }
                else
                {
                    await this.PlaylistManager.Clear();
                }
            });
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistTracksLibraryBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
