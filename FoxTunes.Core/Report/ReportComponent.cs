using FoxTunes.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class ReportComponent : BaseComponent, IReportComponent
    {
        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string[] Headers { get; }

        public abstract IEnumerable<IReportComponentRow> Rows { get; }

        public virtual string ActionName
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual Task<bool> Action()
        {
#if NET40
            return TaskEx.FromResult(false);
#else
            return Task.FromResult(false);
#endif
        }

        public abstract class ReportComponentRow : BaseComponent, IReportComponentRow
        {
            public abstract string[] Values { get; }

            public virtual string ActionName
            {
                get
                {
                    return string.Empty;
                }
            }
            public virtual Task<bool> Action()
            {
#if NET40
                return TaskEx.FromResult(false);
#else
                return Task.FromResult(false);
#endif
            }

        }
    }
}
