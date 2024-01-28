using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class Picard : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string PICARD = "MMMM";

        public ILibraryManager LibraryManager { get; private set; }

        public IPlaylistManager PlaylistManager { get; private set; }

        public ILibraryHierarchyBrowser LibraryHierarchyBrowser { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Enabled { get; private set; }

        public TextConfigurationElement Path { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.LibraryManager = core.Managers.Library;
            this.PlaylistManager = core.Managers.Playlist;
            this.LibraryHierarchyBrowser = core.Components.LibraryHierarchyBrowser;
            this.Configuration = core.Components.Configuration;
            this.Enabled = this.Configuration.GetElement<BooleanConfigurationElement>(
                PicardConfiguration.SECTION,
                PicardConfiguration.ENABLED_ELEMENT
            );
            this.Path = this.Configuration.GetElement<TextConfigurationElement>(
                PicardConfiguration.SECTION,
                PicardConfiguration.PATH_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                if (this.Enabled.Value)
                {
                    yield return new InvocationComponent(InvocationComponent.CATEGORY_LIBRARY, PICARD, "Picard", path: "Tools");
                }
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case PICARD:
                    return this.Open();
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        protected virtual async Task Open()
        {
            var item = this.LibraryManager.SelectedItem;
            if (item == null || LibraryHierarchyNode.Empty.Equals(item))
            {
                return;
            }
            //TODO: Warning: Buffering a potentially large sequence. It might be better to run the query multiple times.
            var items = this.LibraryHierarchyBrowser
                .GetItems(this.LibraryManager.SelectedItem)
                .ToArray();
            await this.Open(items);
            await this.Refresh(items);
        }

        protected virtual Task Refresh(IEnumerable<LibraryItem> items)
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                if (!roots.Contains(item.DirectoryName))
                {
                    roots.Add(item.DirectoryName);
                }
            }
            return this.LibraryManager.Rescan(roots, items);
        }

        protected virtual Task Open(IEnumerable<IFileData> items)
        {
            var builder = new StringBuilder();
            foreach (var item in items)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }
                builder.AppendFormat("\"{0}\"", item.FileName);
            }
            var process = Process.Start(this.Path.Value, builder.ToString());
            return process.WaitForExitAsync();
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PicardConfiguration.GetConfigurationSections();
        }
    }
}
