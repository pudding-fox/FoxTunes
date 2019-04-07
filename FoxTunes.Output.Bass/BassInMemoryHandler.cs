using ManagedBass;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    public static class BassInMemoryHandler
    {
        const string DllName = "bass_inmemory_handler";

        [DllImport(DllName)]
        static extern bool BASS_INMEMORY_HANDLER_Init();

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <returns></returns>
        public static bool Init()
        {
            return BASS_INMEMORY_HANDLER_Init();
        }

        [DllImport(DllName)]
        static extern bool BASS_INMEMORY_HANDLER_Free();

        /// <summary>
        /// Free.
        /// </summary>
        /// <returns></returns>
        public static bool Free()
        {
            return BASS_INMEMORY_HANDLER_Free();
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_INMEMORY_HANDLER_StreamCreateFile(bool mem, string file, long offset, long length, BassFlags flags);

        public static int CreateStream(string File, long Offset = 0, long Length = 0, BassFlags Flags = BassFlags.Default)
        {
            return BASS_INMEMORY_HANDLER_StreamCreateFile(false, File, Offset, Length, Flags | BassFlags.Unicode);
        }
    }
}
