namespace FoxTunes.Interfaces
{
    public interface IBassStreamInput : IBassStreamComponent
    {
        bool CheckFormat(int rate, int channels);

        bool Contains(int channelHandle);

        int Position(int channelHandle);

        bool Add(int channelHandle);

        bool Remove(int channelHandle);

        void Reset();
    }
}
