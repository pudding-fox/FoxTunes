using System.IO;

namespace FoxTunes
{
    public static class TestInfo
    {
        public static string CurrentDirectory
        {
            get
            {
                return Path.GetDirectoryName(typeof(TestInfo).Assembly.Location);
            }
        }

        public static string[] AudioFileNames
        {
            get
            {
                return new[]
                {
                    Path.Combine(CurrentDirectory, "Audio", "A.mp3"),
                    Path.Combine(CurrentDirectory, "Audio", "B.ogg"),
                    Path.Combine(CurrentDirectory, "Audio", "C.mp3"),
                    Path.Combine(CurrentDirectory, "Audio", "D.mp3")
                };
            }
        }
    }
}
