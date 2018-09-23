using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;

namespace Igloo15.MQTTServer.Tool
{
    internal class Settings
    {
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        public Options Server { get; set; } = new Options();
    }

    internal class LoggingSettings
    {
        public ConsoleSettings Console { get; set; } = new ConsoleSettings();
    }

    internal class ConsoleSettings
    {
        public bool IncludeScopes { get; set; } = false;

        public Dictionary<string, string> LogLevel { get; set; } = new Dictionary<string, string>
        {
            ["Default"] = "Information",
            ["IMqttLogger"] = "Warning"
        };
    }

}
