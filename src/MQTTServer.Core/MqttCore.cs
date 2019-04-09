using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using MQTTnet.AspNetCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;

namespace MQTTServer.Core
{
    public class MqttCore
    {
        internal const string MqttEmbeddedBase = "MQTTServer.Core.ClientApp.dist";

        private ILoggerFactory _factory;
        private ILogger<MqttCore> _logger;
        private Interceptors _interceptors;
        private MqttServer _server;
        private IWebHost _webHost;
        private MqttSettings _settings;
        private bool _isRunning = true;

        public MqttCore(ServerOptions options)
        {
            var useKestrel = GetConfiguration(options).GetValue<bool>("Server:UseKestrelServer");
            var useDiag = GetConfiguration(options).GetValue<bool>("Server:StartDiagWebServer");
            if (useKestrel || useDiag)
            {
                _webHost = CreateWebHostBuilder(new string[] { }, options).Build();
                _factory = _webHost.Services.GetService<ILoggerFactory>();
                _settings = _webHost.Services.GetService<MqttSettings>();
            }
            
            if(!useKestrel)
            {
                _server = InitializeServer(options, _webHost);
            }

            _logger = _factory.CreateLogger<MqttCore>();
        }

        public async Task StartAsync()
        {
            if(_server != null)
                await _server.StartAsync();
            if(_webHost != null)
                await _webHost?.StartAsync();

            while (_isRunning)
            {
                Task.Delay(250).Wait();
            }
        }

        public async Task StopAsync()
        {
            _isRunning = false;
            if(_webHost != null)
                await _webHost?.StopAsync();
            if(_server != null)
                await _server?.StopAsync();
        }

        public void CreateSettings()
        {
            if (File.Exists(_settings.Server.ConfigurationFileLocation))
            {
                _logger?.LogInformation("Configuration file {configFile} already exists", _settings.Server.ConfigurationFileLocation);
                return;
            }
            File.WriteAllText(_settings.Server.ConfigurationFileLocation, JsonConvert.SerializeObject(new MqttSettings(), Formatting.Indented));
        }

        public ILoggerFactory GetLoggerFactory()
        {
            return _factory;
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

                    if (settings.Server.UseKestrelServer)
                    {
                        if (string.Equals(settings.Server.IPAddress, "Any", System.StringComparison.OrdinalIgnoreCase))
                            o.ListenAnyIP(settings.Server.Port, l => l.UseMqtt());
                        else if (IPAddress.TryParse(settings.Server.IPAddress, out IPAddress address))
                            o.Listen(address, settings.Server.Port, l => l.UseMqtt());
                        else
                            o.ListenLocalhost(settings.Server.Port, l => l.UseMqtt());
                    }
                    

                    if(settings.Server.StartDiagWebServer)
                        o.ListenAnyIP(settings.Server.DiagWebServerPort);
                })
                .UseStartup<Startup>();


        private MqttServer InitializeServer(ServerOptions options, IWebHost host = null)
        {
            if(_settings == null)
            {
                var config = GetConfiguration(options);
                _settings = new MqttSettings();
                config.Bind(_settings);
                if (_factory == null)
                {
                    _factory = new LoggerFactory();

                    if (!_settings.Server.NoLogConsole)
                        _factory.AddConsole(config.GetSection("Logging:Console"));

                    if (_settings.Server.LogFiles)
                        _factory.AddFile(_settings.Logging.File);
                }
            }

            if(host != null)
            {
                _interceptors = host.Services.GetService<Interceptors>();
            }
            else
            {
                _interceptors = new Interceptors(_factory, null, _settings);
            }
            
            return new MqttServer(MqttServerUtility.BuildOptions(_settings.Server, _factory, _interceptors).Build(), _factory, _interceptors);
        }

        private IConfiguration GetConfiguration(ServerOptions options)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(MqttServerUtility.GetOptions(new MqttSettings(), ""))
                .AddInMemoryCollection(MqttServerUtility.GetOptions(options, "Server"))
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), options.ConfigurationFileLocation), optional: true)
                .AddEnvironmentVariables("MQTTSERVER")
                .Build();
        }
    }

}
