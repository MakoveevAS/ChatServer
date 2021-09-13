using ChatServer.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatServer.Services
{
    public class MessagesService : IMessagesService
    {
        private Stack<string> messages = new Stack<string>();
        private readonly int msgHistoryLimit;
        public event EventHandler<MessageEventArgs> NewMessage;

        public MessagesService(IConfiguration configuration)
        {
            int.TryParse(configuration["MessageHistoryLimit"], out msgHistoryLimit);
        }

        public void SaveMessage(string message, string clientId)
        {
            messages.Push(message);
            NewMessage(this, new MessageEventArgs(message, clientId));
        }


        public string GetLastMessages()
        {
            var sb = new StringBuilder();
            foreach (var m in messages.Take(msgHistoryLimit))
                sb.AppendLine(m);
            return sb.ToString();
        }
    }


    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string ClientId { get; set; }

        public MessageEventArgs(string message, string clientId)
        {
            Message = message;
            ClientId = clientId;
        }
    }
}
