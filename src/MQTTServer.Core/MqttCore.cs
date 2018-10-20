using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MQTTnet.AspNetCore;
using Microsoft.Extensions.Configuration;

namespace MQTTServer.Core
{
    public class MqttCore
    {
        private ILogger _logger;
        private Interceptors _interceptors;
        private MqttServer _server;
        private IWebHost _webHost;

        public MqttCore(ServerOptions options)
        {
            
            if (options.StartWebServer)
            {
                _webHost = CreateWebHostBuilder(new string[] { }, options).Build();
            }
            else
            {
                _server = InitializeServer(options);
            }
        }



        public Task Start()
        {
            if (_server == null)
            {
                return _webHost.RunAsync();
            }
            else
            {
                return _server.StartAsync();
            }
        }

        public Task Stop()
        {
            if(_server == null)
            {
                return _webHost.StopAsync();
            }
            else
            {
                return _server.StopAsync();
            }
        }

        private IWebHostBuilder CreateWebHostBuilder(string[] args, ServerOptions options) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder
                        .AddInMemoryCollection(MqttServerUtility.GetOptions(new MqttSettings(), ""))
                        .AddInMemoryCollection(MqttServerUtility.GetOptions(options, "Server"))
                        .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), options.ConfigurationFileLocation), optional: true)
                        .AddEnvironmentVariables("MQTTSERVER");
                })
                .ConfigureLogging((c,l) =>
                {
                    MqttSettings settings = new MqttSettings();
                    c.Configuration.Bind(settings);

                    l.AddConfiguration(c.Configuration.GetSection("Logging"));

                    switch (settings.Server.LogLevel)
                    {
                        case "Critical":
                            l.SetMinimumLevel(LogLevel.Critical);
                            break;
                        case "Error":
                            l.SetMinimumLevel(LogLevel.Error);
                            break;
                        case "Warning":
                            l.SetMinimumLevel(LogLevel.Warning);
                            break;
                        case "Information":
                            l.SetMinimumLevel(LogLevel.Information);
                            break;
                        case "Debug":
                            l.SetMinimumLevel(LogLevel.Debug);
                            break;
                        case "Trace":
                            l.SetMinimumLevel(LogLevel.Trace);
                            break;

                    }

                    if (!settings.Server.NoLogConsole)
                        l.AddConsole();

                    if (settings.Server.LogFiles)
                        l.AddFile(c.Configuration.GetSection("Logging:File"));
                })
                .ConfigureServices((c,s) =>
                {
                    MqttSettings settings = new MqttSettings();
                    c.Configuration.Bind(settings);

                    s.AddSingleton(settings);
                    s.AddSingleton<MqttModel>();
                    s.AddSingleton<Interceptors>();
                })
                .UseKestrel((c, o) =>
                {
                    MqttSettings settings = new MqttSettings();
                    c.Configuration.Bind(settings);

                    o.ListenAnyIP(settings.Server.Port, l => l.UseMqtt());
                    o.ListenAnyIP(settings.Server.WebServerPort);
                })
                .UseStartup<Startup>();


        private MqttServer InitializeServer(ServerOptions options)
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(MqttServerUtility.GetOptions(new MqttSettings(), ""))
                .AddInMemoryCollection(MqttServerUtility.GetOptions(options, "Server"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), options.ConfigurationFileLocation), optional: true)
                .AddEnvironmentVariables("MQTTSERVER");

            var config = builder.Build();
            MqttSettings settings = new MqttSettings();
            config.Bind(settings);

            var factory = new LoggerFactory();

            if (!settings.Server.NoLogConsole)
                factory.AddConsole(config.GetSection("Logging:Console"));

            if (settings.Server.LogFiles)
                factory.AddFile(settings.Logging.File);

            _interceptors = new Interceptors(factory, null);

            MqttServer server = new MqttServer(MqttServerUtility.BuildOptions(settings.Server, factory, _interceptors).Build(), factory);



            return server;
        }
    }

}
