using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxTunes
{
    public class Server : IDisposable
    {
        public static readonly string Id = typeof(Server).Assembly.FullName;

        public Server()
        {
            var success = default(bool);
            this.Mutex = new Mutex(true, Id, out success);
            if (success)
            {
                Task.Factory.StartNew(() => this.OnStart());
            }
            else
            {
                this.Dispose();
            }
        }

        public Mutex Mutex { get; private set; }

        protected virtual async Task OnStart()
        {
            using (var server = new NamedPipeServerStream(Id))
            {
                using (var reader = new StreamReader(server))
                {
                    while (!this.IsDisposed)
                    {
#if NET40
                        server.WaitForConnection();
#else
                        await server.WaitForConnectionAsync();
#endif
                        await this.OnConnection(server, reader);
                    }
                }
            }
        }

        protected virtual async Task OnConnection(NamedPipeServerStream server, StreamReader reader)
        {
            var builder = new StringBuilder();
            while (server.IsConnected)
            {
                builder.AppendLine(await reader.ReadLineAsync());
            }
            this.OnMessage(builder.ToString());
            server.Disconnect();
        }

        protected virtual void OnMessage(string message)
        {
            if (this.Message == null)
            {
                return;
            }
            this.Message(this, new ListenerEventArgs(message));
        }

        public event ListenerEventHandler Message;

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Mutex.Dispose();
        }

        ~Server()
        {
            this.Dispose(true);
        }
    }

    public delegate void ListenerEventHandler(object sender, ListenerEventArgs e);

    public class ListenerEventArgs : EventArgs
    {
        public ListenerEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
