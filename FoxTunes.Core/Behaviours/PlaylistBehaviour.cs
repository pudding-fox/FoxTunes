using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class PlaylistBehaviour : StandardBehaviour, IInvocableComponent, IConfigurableComponent
    {
        public const string SHUFFLE = "AAAA";

        public IConfiguration Configuration { get; private set; }

        public BooleanConfigurationElement Shuffle { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.Configuration = core.Components.Configuration;
            this.Shuffle = this.Configuration.GetElement<BooleanConfigurationElement>(
                PlaylistBehaviourConfiguration.SECTION,
                PlaylistBehaviourConfiguration.SHUFFLE_ELEMENT
            );
            base.InitializeComponent(core);
        }

        public IEnumerable<IInvocationComponent> Invocations
        {
            get
            {
                yield return new InvocationComponent(InvocationComponent.CATEGORY_PLAYBACK, SHUFFLE, "Shuffle", attributes: this.Shuffle.Value ? InvocationComponent.ATTRIBUTE_SELECTED : InvocationComponent.ATTRIBUTE_NONE);
            }
        }

        public Task InvokeAsync(IInvocationComponent component)
        {
            switch (component.Id)
            {
                case SHUFFLE:
                    this.Shuffle.Toggle();
#if NET40
                    return TaskEx.Run(() => this.Configuration.Save());
#else
                    return Task.Run(() => this.Configuration.Save());
#endif
            }
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.CompletedTask;
#endif
        }

        public IEnumerable<ConfigurationSection> GetConfigurationSections()
        {
            return PlaylistBehaviourConfiguration.GetConfigurationSections();
        }
    }
}
