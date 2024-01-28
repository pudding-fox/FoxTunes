using FoxTunes.Interfaces;
using System.Text;

namespace FoxTunes
{
    public abstract class FilterParserProvider : BaseComponent, IFilterParserProvider
    {
        public const byte PRIORITY_HIGH = 0;

        public const byte PRIORITY_NORMAL = 100;

        public const byte PRIORITY_LOW = 255;

        public abstract byte Priority { get; }

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
