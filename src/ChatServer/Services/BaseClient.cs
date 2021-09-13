using System;

namespace ChatServer.Services
{
    public class BaseClient
    {
        public string Id { get; set; }
        public string UserName { get; set; }


        public BaseClient()
        {
            Id = Guid.NewGuid().ToString();
            ClientCounter.Add(Id);
        }
    }
}
