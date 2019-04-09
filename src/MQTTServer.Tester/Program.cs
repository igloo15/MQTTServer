using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttServer.Tester
{
    class Program
    {
        private static bool _isRunning = true;
        private static IManagedMqttClient _client;
        private static IMqttClientOptions _options;
        private static IManagedMqttClientOptions _managedOptions;
        private static int _messageCount;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //MqttCore core = new MqttCore(new ServerOptions {
            //    LogLevel = "Information",
            //    ShowSubscriptions = true,
            //    ShowClientConnections = true,
            //    ShowMessages = true,
            //    UseKestrelServer = true,
            //    UseWebSocket = true,
            //    StartDiagWebServer = true,
            //    LogFiles = true
            //});

            //core.StartAsync();

            string name = Guid.NewGuid().ToString();

            if (args.Length > 0)
                name = args[0];

            

            _options = new MqttClientOptionsBuilder()
                                .WithTcpServer("localhost", 5002) // Port is optional
                                .WithClientId(name)
                                .WithCleanSession()
                                .Build();

            _managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(2))
                .WithClientOptions(_options)
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();

            _client.ApplicationMessageReceived += Client_ApplicationMessageReceived;

            Console.WriteLine("Ready to Connect");

            Console.ReadLine();

            Console.WriteLine("Connecting");

            var task = _client.StartAsync(_managedOptions);

            _client.Disconnected += Client_Disconnected;
            _client.Connected += Client_Connected;

            task.Wait();

            Console.WriteLine("Subscribing");
            _client.SubscribeAsync("my/amazing/topic");
            _client.SubscribeAsync("my/+/stuff");
            _isRunning = true;

            Console.WriteLine("Ready to Send Data");
            var messageRunner = Task.Factory.StartNew(() => MessageRunner(), TaskCreationOptions.LongRunning);
            Console.WriteLine("Hit Enter to Disconnect or a number to publish that many messages");
            while (_isRunning)
            {
                string number = Console.ReadLine();
                if (Int32.TryParse(number, out int messageCount))
                {
                    Console.WriteLine($"Sending {messageCount} more messages");
                    _messageCount += messageCount;
                }
                else
                {
                    _isRunning = false;
                }
            }
            Console.WriteLine("Finished Sending Data");
            messageRunner.Wait();
            _client.StopAsync().Wait();
            Console.WriteLine("Disconnected");
            Console.WriteLine("Hit Enter to Shutdown");
            Console.ReadLine();
        }

        private static void MessageRunner()
        {

            var builder = new MqttApplicationMessageBuilder()
                .WithAtMostOnceQoS()
                .WithTopic("my/dance/stuff");

            while (_isRunning)
            {
                if (_messageCount == 0)
                    Task.Delay(250).Wait();
                else
                {
                    Thread.Sleep(1);
                    for(var i = 0; (i < 100 && _messageCount > 0); i++)
                    {
                        _messageCount--;
                        var message = builder
                            .WithPayload("{ \"DanceMove\":\"Break Dancing" + _messageCount + "\" }")
                            .Build();

                        _client.PublishAsync(message);
                    }

                }
            }
        }

        private static void Client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected");
            
        }

        private static void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (_isRunning)
            {
            }
        }

        private static long messageCount = 0;

        private static void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            messageCount++;

            if(messageCount % 50000 == 0)
                Console.WriteLine($"Total Messages Received {messageCount}");
        }
    }
}
