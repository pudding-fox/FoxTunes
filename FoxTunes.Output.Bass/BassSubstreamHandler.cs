using ManagedBass;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class BassSubstreamHandler
    {
        const string DllName = "bass_substream_handler";

        [DllImport(DllName)]
        static extern bool BASS_SUBSTREAM_HANDLER_Init();

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            return BASS_SUBSTREAM_HANDLER_Init();
        }

        [DllImport(DllName)]
        static extern bool BASS_SUBSTREAM_HANDLER_Free();

        /// <summary>
        /// Free.
        /// </summary>
        /// <returns></returns>
        public static bool Free()
        {
            return BASS_SUBSTREAM_HANDLER_Free();
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_SUBSTREAM_HANDLER_StreamCreate(int handle, long offset, long length, BassFlags flags);

        public static int CreateStream(int Handle, long Offset, long Length, BassFlags Flags = BassFlags.Default)
        {
            return BASS_SUBSTREAM_HANDLER_StreamCreate(Handle, Offset, Length, Flags | BassFlags.Unicode);
        }
    }
}
