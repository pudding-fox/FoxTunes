namespace FoxTunes
{
    public class CancellationToken
    {
        public bool IsCancellationRequested { get; private set; }

        public void Cancel()
        {
            this.IsCancellationRequested = true;
        }

        public void Reset()
        {
            this.IsCancellationRequested = false;
        }

        public static CancellationToken None
        {
            get
            {
                return new CancellationToken();
            }
        }
    }
}
