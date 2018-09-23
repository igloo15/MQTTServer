using System;
using CommandLine;
using CommandLine.Text;
using MQTTnet.Server;
using MQTTnet;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Igloo15.MQTTServer.Tool
{
    class Program
    {
        private static ILogger _logger;
        private static Interceptors _interceptors;
        private static Igloo15MqttServer _server;
        private static LoggerFactory _factory;

        static void Main(string[] args) => Parser.Default.ParseArguments<Options>(args).MapResult(o => Execute(o), _ => 1);

        static int Execute(Options config)
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
                _server?.StopAsync().Wait();
            };

            _factory = new LoggerFactory();
            if (File.Exists(config.ConfigurationFileLocation))
            {

                var builder = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), config.ConfigurationFileLocation))
                    .AddEnvironmentVariables("MQTTSERVER");

                IConfiguration configFile = builder.Build();

                configFile.GetSection("Server").Bind(config);
                _factory.AddConsole(configFile.GetSection("Logging:Console"));
            }
            else
            {
                if(Enum.TryParse<LogLevel>(config.LogLevel, out LogLevel result))
                {
                    _factory.AddConsole(result);
                }
                else
                {
                    _factory.AddConsole(LogLevel.Information);
                }
            }

            _logger = _factory.CreateLogger("MQTTServer");

            if (config.MakeConfig)
            {
                if (File.Exists(config.ConfigurationFileLocation))
                {
                    _logger.LogInformation("Configuration file {configFile} already exists", config.ConfigurationFileLocation);
                    return 0;
                }
                _logger.LogInformation("Copying Default Configuration to Working Directory");
                File.Copy(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), config.ConfigurationFileLocation), config.ConfigurationFileLocation);
                _logger.LogInformation("Config File Copied");
                return 0;
            }


            _interceptors = new Interceptors(_factory);
            _server = InitializeServer(config, _factory);

            if (_server == null)
                return 1;

            var task = _server.StartAsync();

            _logger.LogInformation("Server Started on {Address}:{Port}", config.IPAddress, config.Port);
            _logger.LogInformation("Hit Ctrl-C to Exit");

            task.Wait();
            
            while(_server.IsRunning())
            {
                Task.Delay(250).Wait();
            }

            return 0;
        }

        static Igloo15MqttServer InitializeServer(Options config, ILoggerFactory factory)
        {
            Igloo15MqttServer server = null;
            try
            {
                MqttServerOptionsBuilder serverBuilder = new MqttServerOptionsBuilder();

                if (config.Encrypted && File.Exists(config.CertificateLocation))
                {

                    if (config.IPAddress == "Any")
                        serverBuilder.WithEncryptedEndpoint();
                    else if (IPAddress.TryParse(config.IPAddress, out IPAddress address))
                        serverBuilder.WithEncryptedEndpointBoundIPAddress(address);
                    else
                    {
                        _logger.LogWarning($"Failed to parse provided IP Address : {config.IPAddress} using default");
                        serverBuilder.WithEncryptedEndpoint();
                    }

                    serverBuilder.WithEncryptedEndpointPort(config.Port);

                    var certificate = new X509Certificate2(config.CertificateLocation, config.CertificatePassword);
                    var certBytes = certificate.Export(X509ContentType.Cert);

                    serverBuilder.WithEncryptionCertificate(certBytes);

                }
                else
                {
                    if (config.IPAddress == "Any")
                        serverBuilder.WithDefaultEndpoint();
                    else if (IPAddress.TryParse(config.IPAddress, out IPAddress address))
                        serverBuilder.WithDefaultEndpointBoundIPAddress(address);
                    else
                    {
                        _logger.LogWarning("Failed to parse provided IP Address : {IPAddress} using default", config.IPAddress);
                        serverBuilder.WithDefaultEndpoint();
                    }

                    serverBuilder.WithDefaultEndpointPort(config.Port);
                }

                if (config.Persistent)
                    serverBuilder.WithPersistentSessions();

                if (!String.IsNullOrEmpty(config.MessageStorageLocation))
                    serverBuilder.WithStorage(new RetainedMessageHandler(config.MessageStorageLocation));

                if (config.ShowSubscriptions)
                    serverBuilder.WithSubscriptionInterceptor(_interceptors.SubscriptionInterceptor);

                if (config.ShowMessages)
                    serverBuilder.WithApplicationMessageInterceptor(_interceptors.MessageInterceptor);

                if (config.ShowClientConnections)
                    serverBuilder.WithConnectionValidator(_interceptors.ConnectionInterceptor);

                serverBuilder
                    .WithConnectionBacklog(config.ConnectionBacklog)
                    .WithMaxPendingMessagesPerClient(config.MaxPendingMessagesPerClient)
                    .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(config.CommunicationTimeout));


                server = new Igloo15MqttServer(serverBuilder.Build(), factory);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception occured during Initialization of Server");
                return null;
            }

            return server;
        }
    }
}
