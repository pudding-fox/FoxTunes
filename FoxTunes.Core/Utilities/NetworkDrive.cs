using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FoxTunes
{
    public static class NetworkDrive
    {
        public static bool IsRemotePath(string path)
        {
            var letter = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(letter))
            {
                return false;
            }
            var info = new DriveInfo(letter);
            return info.DriveType == DriveType.Network || info.DriveType == DriveType.NoRootDirectory;
        }

        /// <summary>
        /// There are many obscure native functions for reconnecting a network drive.
        /// They're all extremely complicated, requiring the UNC path, credentials (or a window handle to prompt for them).
        /// This routine simple opens explorer.exe on the drive letter then closes any resulting windows.
        /// The process created exits immidiately; a different process belongs to the window.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<bool> ConnectRemotePath(string path)
        {
            var letter = Path.GetPathRoot(path);
            var info = new DriveInfo(letter);

            if (info.IsReady)
            {
                return true;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = letter,
                WindowStyle = ProcessWindowStyle.Minimized
            });

            for (var a = 0; a < 10; a++)
            {
                if (info.IsReady)
                {
                    break;
                }
#if NET40
                await TaskEx.Delay(TimeSpan.FromSeconds(1));
#else
                await Task.Delay(TimeSpan.FromSeconds(1));
#endif
            }

            foreach (var process in Process.GetProcessesByName("explorer"))
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.EndsWith(string.Format("({0}:)", letter[0])))
                {
                    process.Kill();
                    break;
                }
            }

            return info.IsReady;
        }
    }
}
