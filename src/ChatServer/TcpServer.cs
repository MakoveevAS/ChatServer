using ChatServer.Interfaces;
using ChatServer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class TcpServer : IHostedService, IDisposable
    {
        private readonly int port;
        private readonly IMessagesService _messagesService;
        static TcpListener tcpListener;

        List<TcpClient> clients = new List<TcpClient>();

        public TcpServer(IMessagesService messagesService,
            IConfiguration configuration)
        {
            int.TryParse(configuration["TcpPort"], out port);
            _messagesService = messagesService;
            _messagesService.NewMessage += onNewMessage;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Connect new client
        /// </summary>
        /// <param name="clientObject"></param>
        public void AddConnection(TcpClient clientObject)
        {
            clients.Add(clientObject);
        }

        /// <summary>
        /// Delete a connection for a client
        /// </summary>
        /// <param name="id"></param>
        public void RemoveConnection(string id)
        {
            var client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                clients.Remove(client);
        }

        /// <summary>
        /// listening for incoming connections
        /// </summary>
        public void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine("Server is running...");

                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();
                    var clientObject = new TcpClient(tcpClient, this, _messagesService);
                    var clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Broadcasting a message to connected clients
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        public void BroadcastMessage(string message, string id, bool saveMessage = true)
        {
            if (saveMessage)
                _messagesService.SaveMessage(message, id);
            var data = Encoding.Unicode.GetBytes(message);
            foreach (var client in clients.Where(c => c.Id != id))
                client.Stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Send messege to client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        public void SendMessage(string message, string id)
        {
            var client = clients.First(c => c.Id == id);
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.Stream.Write(data, 0, data.Length);
        }


        public void Dispose()
        {
            foreach (var client in clients)
                client.Close();

            tcpListener.Stop();
        }


        private void onNewMessage(object sender, MessageEventArgs e)
        {
            BroadcastMessage(e.Message, e.ClientId, false);
        }




    }
}
