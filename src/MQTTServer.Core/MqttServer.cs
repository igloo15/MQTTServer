using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    internal class MqttServer
    {
        private ILogger _logger;
        private IMqttServer _server;
        private IMqttServerOptions _options;
        private bool _isRunning;

        public MqttServer(IMqttServerOptions options, ILoggerFactory factory)
        {
            if (options == null)
                throw new ArgumentNullException("options", "Options cannot be null when constructing server");

            if (factory == null)
                throw new ArgumentNullException("factory", "LoggerFactory cannot be null when constructing server");

            _options = options;
            _logger = factory.CreateLogger<MqttLogger>();
            _server = new MqttFactory().CreateMqttServer(new MqttLogger(factory));

            _server.Started += (s, ev) =>
            {
                _isRunning = true;
            };

            _server.Stopped += (s, ev) =>
            {
                _isRunning = false;
            };
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting Server on {IpAddress} and port {Port}", _options.DefaultEndpointOptions.BoundInterNetworkAddress.ToString(), _options.DefaultEndpointOptions.Port);
            await _server.StartAsync(_options);

            while (_isRunning)
            {
                Task.Delay(250).Wait();
            }
        }

        public async Task StopAsync()
        {
            _isRunning = false;
            await _server.StopAsync();
        }
    }

}