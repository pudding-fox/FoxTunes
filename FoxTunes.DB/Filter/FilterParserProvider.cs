using FoxTunes.Interfaces;
using System.Text;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.Database)]
    public abstract class FilterParserProvider : StandardComponent, IFilterParserProvider
    {
        public IFilterParser FilterParser { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.FilterParser = core.Components.FilterParser;
            this.FilterParser.Register(this);
            base.InitializeComponent(core);
        }

        public abstract bool TryParse(ref string filter, out IFilterParserResultGroup result);

        protected virtual void OnParsed(ref string filter, int position, int length)
        {
            var builder = new StringBuilder();
            if (position > 0)
            {
                builder.Append(filter.Substring(0, position));
            }
            if (length > 0 && length < filter.Length)
            {
                builder.Append(filter.Substring(position + length, filter.Length - (position + length)));
            }
            filter = builder.ToString();
        }
    }
}
