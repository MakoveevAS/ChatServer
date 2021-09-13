using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChatServer
{
    public interface IWebsocketHandler
    {
        public Task HandleAsync(WebSocket webSocket);

    }
}
