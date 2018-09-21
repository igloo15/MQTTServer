using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

namespace Igloo15.MQTTServer.Tool
{
    internal class Igloo15MqttServer
    {
        ILogger _logger;
        IMqttServer _server;
        IMqttServerOptions _options;

        public Igloo15MqttServer(IMqttServerOptions options, ILoggerFactory factory)
        {
            if (options == null)
                throw new ArgumentNullException("options", "Options cannot be null when constructing server");

            if (factory == null)
                throw new ArgumentNullException("factory", "LoggerFactory cannot be null when constructing server");

            _options = options;
            _logger = factory.CreateLogger<Igloo15MqttLogger>();
            _server = new MqttFactory().CreateMqttServer(new Igloo15MqttLogger(factory));
        }

        public Task StartAsync()
        {
            return _server.StartAsync(_options);
        }

        public Task StopAsync()
        {
            return _server.StopAsync();
        }
    }

}