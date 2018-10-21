using System;
using CommandLine;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MQTTServer.Core;

namespace MqttServer.Tool
{
    class Program
    {
        private static ILogger _logger;
        private static LoggerFactory _factory;
        private static MqttCore _core;

        static void Main(string[] args) => Parser.Default.ParseArguments<ServerOptions>(args).MapResult(o => Execute(o), _ => 1);

        static int Execute(ServerOptions config)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                _logger?.LogInformation("MQTTServer Closed");
                _factory?.Dispose();
            };

            Console.CancelKeyPress += (s, ev) =>
            {
                _logger?.LogInformation("Ctrl-C pressed");
                _logger?.LogInformation("MQTTServer Closing");
                ev.Cancel = true;
                _core?.StopAsync().GetAwaiter().GetResult();
            };


            _core = new MqttCore(config);

            if (config.MakeConfig)
            {
                _core.CreateSettings();
                return 0;
            }


            _logger = _core.GetLoggerFactory().CreateLogger<Program>();

            var task = _core.StartAsync();

            _logger.LogInformation("Server Started on {Address}:{Port}", config.IPAddress, config.Port);
            _logger.LogInformation("Hit Ctrl-C to Exit");

            task.Wait();

            return 0;
        }

    }
}
