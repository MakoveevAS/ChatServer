using ChatServer.Interfaces;
using ChatServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class WebsocketHandler : IWebsocketHandler
    {
        public List<SocketConnection> websocketConnections = new List<SocketConnection>();
        IMessagesService _messagesService;

        public WebsocketHandler(IMessagesService messagesService)
        {
            _messagesService = messagesService;
            SetupCleanUpTask();
            _messagesService.NewMessage += onNewMessage;
        }

        public async Task HandleAsync(WebSocket webSocket)
        {
            var userName = await ReceiveMessageAsync(webSocket);
            var message = $"{userName} enter the chat";
            var newConnection = new SocketConnection
            {
                Client = webSocket,
                UserName = userName
            };

            lock (websocketConnections)
            {
                websocketConnections.Add(newConnection);
            }


            await BroadcastMessageAsync(message, newConnection.Id);
            Console.WriteLine(message);

            while (webSocket.State == WebSocketState.Open)
            {
                message = await ReceiveMessageAsync(webSocket);
                if (!string.IsNullOrEmpty(message))
                {
                    message = $"{userName}: {message}";
                    await BroadcastMessageAsync(message, newConnection.Id);
                    Console.WriteLine(message);
                }
            }

        }

        private async Task<string> ReceiveMessageAsync(WebSocket webSocket)
        {
            var arraySegment = new ArraySegment<byte>(new byte[4096]);
            var receivedMessage = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            if (receivedMessage.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.Default.GetString(arraySegment).TrimEnd('\0');
                if (!string.IsNullOrWhiteSpace(message))
                    return message;
            }
            return string.Empty;
        }

        private async Task BroadcastMessageAsync(string message, string id, bool saveMessage = true)
        {
            if (saveMessage)
                _messagesService.SaveMessage(message, id);

            IEnumerable<SocketConnection> toSentTo;

            lock (websocketConnections)
            {
                toSentTo = websocketConnections.Where(c => c.Id != id).ToList();
            }

            var tasks = toSentTo.Select(async websocketConnection =>
            {
                var bytes = Encoding.Default.GetBytes(message);
                var arraySegment = new ArraySegment<byte>(bytes);
                await websocketConnection.Client.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            });
            await Task.WhenAll(tasks);
        }

        private void SetupCleanUpTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    IEnumerable<SocketConnection> openSockets;
                    IEnumerable<SocketConnection> closedSockets;

                    lock (websocketConnections)
                    {
                        openSockets = websocketConnections.Where(x => x.Client.State == WebSocketState.Open || x.Client.State == WebSocketState.Connecting);
                        closedSockets = websocketConnections.Where(x => x.Client.State != WebSocketState.Open && x.Client.State != WebSocketState.Connecting);

                        websocketConnections = openSockets.ToList();
                    }

                    foreach (var closedWebsocketConnection in closedSockets)
                    {
                        var message = $"{closedWebsocketConnection.UserName} has left the chat";
                        Console.WriteLine(message);
                        ClientCounter.Remove(closedWebsocketConnection.Id);
                        await BroadcastMessageAsync(message, closedWebsocketConnection.Id);
                    }

                    await Task.Delay(5000);
                }

            });
        }

        private void onNewMessage(object sender, MessageEventArgs e)
        {
            Task.Run(async () =>
            {
                await BroadcastMessageAsync(e.Message, e.ClientId, false);
            });
        }
    }
}
