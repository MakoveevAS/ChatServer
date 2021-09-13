using ChatServer.Services;
using System.Net.WebSockets;

namespace ChatServer
{
    public class SocketConnection : BaseClient
    {
        public WebSocket Client { get; set; }
    }
}
