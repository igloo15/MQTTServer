﻿using Microsoft.AspNetCore.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTServer.Core;
using System;
using System.Threading.Tasks;

namespace MQTTServer.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            

            Console.WriteLine("Hello World!");

            MqttCore core = new MqttCore();

            core.CreateWebHostBuilder(new string[] { }).Build().Run();



            var options = new MqttClientOptionsBuilder()
                                .WithTcpServer("localhost", 5002) // Port is optional
                                .Build();

            var client = new MqttFactory().CreateMqttClient();

            Console.WriteLine("Ready to Connect");
            
            Console.ReadLine();

            Console.WriteLine("Connecting");

            var task = client.ConnectAsync(options);

            task.ContinueWith(t =>
            {

                Console.WriteLine("Subscribing");
                client.SubscribeAsync("my/amazing/topic");
                client.SubscribeAsync("my/+/stuff");

                return true;
            })
            .ContinueWith(t => 
            {
                if(t.Result)
                {
                    Task.Delay(10000).Wait();
                    Console.WriteLine("Sending Data");
                    client.PublishAsync("my/dance/stuff", "{ \"DanceMove\":\"Break Dancing\" }");
                }
            });


            Console.WriteLine("Hit Enter to Disconnect");
            Console.ReadLine();
            client.DisconnectAsync().Wait();
            Console.WriteLine("Disconnected");
            Console.WriteLine("Hit Enter to Shutdown");
            Console.ReadLine();
        }
    }
}
