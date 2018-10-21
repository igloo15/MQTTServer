using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTServer.Core;
using System;
using System.Threading.Tasks;

namespace MqttServer.Tester
{
    class Program
    {
        private static bool _isRunning = true;
        private static IMqttClient _client;
        private static IMqttClientOptions _options;
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
                                .Build();

            _client = new MqttFactory().CreateMqttClient();

            _client.ApplicationMessageReceived += Client_ApplicationMessageReceived;

            Console.WriteLine("Ready to Connect");

            Console.ReadLine();

            Console.WriteLine("Connecting");

            var task = _client.ConnectAsync(_options);

            _client.Disconnected += Client_Disconnected;
            _client.Connected += Client_Connected;

            task.Wait();

            Console.WriteLine("Ready to Send Data");
            Console.ReadLine();
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
            _client.DisconnectAsync().Wait();
            Console.WriteLine("Disconnected");
            Console.WriteLine("Hit Enter to Shutdown");
            Console.ReadLine();
        }

        private static void MessageRunner()
        {
            while (_isRunning)
            {
                if (_messageCount == 0)
                    Task.Delay(250).Wait();
                else
                {
                    _messageCount--;
                    _client.PublishAsync("my/dance/stuff", "{ \"DanceMove\":\"Break Dancing" + _messageCount + "\" }");
                }
            }
        }

        private static void Client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected");
            Console.WriteLine("Subscribing");
            _client.SubscribeAsync("my/amazing/topic");
            _client.SubscribeAsync("my/+/stuff");
            _isRunning = true;
        }

        private static void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (_isRunning)
            {
                _client.ConnectAsync(_options);
                Console.WriteLine("Reconnecting");
            }
        }

        private static long messageCount = 0;

        private static void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            messageCount++;

            if(messageCount % 50000 == 0)
                Console.WriteLine($"Received Message on {e.ApplicationMessage.Topic} from {e.ClientId}");
        }
    }
}
