using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TestSignalRClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hubConnection = new HubConnectionBuilder()
                                 .WithUrl("https://localhost:5001/testhub")
                                 .AddNewtonsoftJsonProtocol()
                                 .WithAutomaticReconnect()
                                 .Build();

            hubConnection.StartAsync().GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                var channel = Channel.CreateBounded<string>(10);
                await hubConnection.SendAsync("TestUpstream", channel.Reader);
                await channel.Writer.WriteAsync("some data");
                await channel.Writer.WriteAsync("some more data");
                channel.Writer.Complete(new Exception("an error occurred"));
            }).Wait();

            Console.ReadKey();

            hubConnection.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
