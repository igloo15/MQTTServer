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

        public MqttServer(IMqttServerOptions options, ILoggerFactory factory, Interceptors interceptors)
        {
            if (options == null)
                throw new ArgumentNullException("options", "Options cannot be null when constructing server");

            if (factory == null)
                throw new ArgumentNullException("factory", "LoggerFactory cannot be null when constructing server");

            _options = options;
            _logger = factory.CreateLogger<MqttLogger>();
            _server = new MqttFactory().CreateMqttServer(new MqttLogger(factory));

            interceptors.SetServer(_server);
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting Server on {IpAddress} and port {Port}", _options.DefaultEndpointOptions.BoundInterNetworkAddress.ToString(), _options.DefaultEndpointOptions.Port);
            await _server.StartAsync(_options);            
        }

        public async Task StopAsync()
        {
            
            await _server.StopAsync();
        }
    }

}