using Microsoft.Extensions.Logging;
using Serilog.Microsoft.Logger.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    public class MqttSettings
    {
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        public ServerOptions Server { get; set; } = new ServerOptions();

        public WebServerOptions WebServer { get; set; } = new WebServerOptions();
    }

    public class LoggingSettings
    {
        public FileConfiguration File { get; set; } = new FileConfiguration();

        public ConsoleConfiguration Console { get; set; } = new ConsoleConfiguration();
    }

    public class ConsoleConfiguration
    {
        public bool IncludeScopes { get; set; } = false;

        public Dictionary<string, LogLevel> LogLevel { get; set; } = new Dictionary<string, LogLevel> {
            ["Default"] = Microsoft.Extensions.Logging.LogLevel.Information,
            ["IMqttLogger"] = Microsoft.Extensions.Logging.LogLevel.Warning,
            ["Microsoft.AspNetCore"] = Microsoft.Extensions.Logging.LogLevel.Warning
        };
    }

    public class WebServerOptions
    {
        public int BufferSize { get; set; } = 100;
    }

}
