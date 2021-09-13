using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ChatServer
{
    public class Program
    {
        const string EXIT_COMMAND = "exit";
        const string CONNECTIONS_LIST_COMMAND = "ls";

        public static void Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            host.Start();

            var alive = true;
            while (alive)
            {
                var command = Console.ReadLine();
                if (string.Equals(command, EXIT_COMMAND))
                {
                    alive = false;
                }
                else if (string.Equals(command, CONNECTIONS_LIST_COMMAND))
                    Console.WriteLine($"Connections count: {ClientCounter.ClientsCount}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
