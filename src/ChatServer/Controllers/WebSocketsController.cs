using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;
        private IWebsocketHandler _websocketHandler;

        public WebSocketsController(IWebsocketHandler websocketHandler,
            ILogger<WebSocketsController> logger)
        {
            _websocketHandler = websocketHandler;
            _logger = logger;
        }


        [HttpGet("/ws")]
        public async Task Get()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await _websocketHandler.HandleAsync(webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}