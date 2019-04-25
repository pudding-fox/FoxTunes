using System.Diagnostics;
using System.IO;

namespace FoxTunes
{
    public class ProcessReader
    {
        const int BUFFER_SIZE = 10240;

        public ProcessReader(Process process)
        {
            this.Process = process;
        }

        public Process Process { get; }

        public void CopyTo(ProcessWriter writer)
        {
            var length = default(int);
            var buffer = new byte[BUFFER_SIZE];
            while (!this.Process.HasExited)
            {
                while ((length = this.Process.StandardOutput.BaseStream.Read(buffer, 0, BUFFER_SIZE)) > 0)
                {
                    writer.Write(buffer, length);
                }
            }
            writer.Close();
        }

        public void CopyTo(Stream stream)
        {
            this.Process.StandardOutput.BaseStream.CopyTo(stream);
        }
    }
}
