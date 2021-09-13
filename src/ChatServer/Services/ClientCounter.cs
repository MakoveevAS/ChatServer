using System.Collections.Generic;
using System.Linq;

namespace ChatServer
{
    public static class ClientCounter
    {
        static List<string> clients = new List<string>();

        public static int ClientsCount
        {
            get => clients.Count();
        }

        public static void Add(string id) => clients.Add(id);
        public static void Remove(string id) => clients.Remove(id);

    }
}
