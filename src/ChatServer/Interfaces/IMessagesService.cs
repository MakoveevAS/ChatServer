using ChatServer.Services;
using System;

namespace ChatServer.Interfaces
{
    public interface IMessagesService
    {
        public event EventHandler<MessageEventArgs> NewMessage;
        public void SaveMessage(string message, string clientId);
        public string GetLastMessages();
    }
}
