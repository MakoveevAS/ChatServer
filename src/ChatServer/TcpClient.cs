using ChatServer.Interfaces;
using ChatServer.Services;
using System;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class TcpClient : BaseClient
    {
        public NetworkStream Stream { get; private set; }
        TcpServer server;
        IMessagesService _messagesService;
        System.Net.Sockets.TcpClient Client;

        public TcpClient(System.Net.Sockets.TcpClient tcpClient,
            TcpServer serverObject,
            IMessagesService messagesService)
        {
            Client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
            _messagesService = messagesService;
        }

        public void Process()
        {
            try
            {
                Stream = Client.GetStream();
                UserName = GetMessage();

                server.SendMessage(_messagesService.GetLastMessages(), Id);

                var message = $"{UserName} enter the chat";
                server.BroadcastMessage(message, Id);
                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = $"{UserName}: {GetMessage()}";
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, Id);
                    }
                    catch
                    {
                        message = $"{UserName} leave the chat";
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                Close();
            }
        }

        /// <summary>
        /// reading an incoming message and converting it to a string
        /// </summary>
        /// <returns></returns>
        private string GetMessage()
        {
            byte[] data = new byte[64];
            var sb = new StringBuilder();
            do
            {
                var bytes = Stream.Read(data, 0, data.Length);
                sb.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return sb.ToString();
        }

        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (Client != null)
                Client.Close();
            ClientCounter.Remove(Id);
        }
    }
}
