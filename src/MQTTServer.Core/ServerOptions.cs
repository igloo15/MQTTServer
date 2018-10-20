using System;
using CommandLine;
using Microsoft.Extensions.Configuration;

namespace MQTTServer.Core
{
    public class ServerOptions
    {
        [Option('p', "port", Default = 5002, HelpText = "The port to run the mqtt server on")]
        public int Port { get; set; } = 5002;

        [Option('a', "address", Default = "Any", HelpText = "The ip address of the server it defaults to Any")]
        public string IPAddress { get; set; } = "Any";

        [Option("timeout", Default = 15, HelpText = "The amount of time a connection is maintained without communication before dropping")]
        public int CommunicationTimeout { get; set; } = 15;

        [Option("connection-backlog", Default = 10, HelpText = "Amount of connections kept in backlog before dropping")]
        public int ConnectionBacklog { get; set; } = 10;

        [Option("message-backlog", Default = 250, HelpText = "Amount of messages kept in backlog per client before dropping")]
        public int MaxPendingMessagesPerClient { get; set; } = 250;

        [Option("persistent", Default = false, HelpText = "Persistent Sessions")]
        public bool Persistent { get; set; } = false;

        [Option("encrypted", Default = false, HelpText = "Encrypted connections SSL/TLS requires certificate setting")]
        public bool Encrypted { get; set; } = false;

        [Option("cert-location", HelpText = "The location of the certificate")]
        public string CertificateLocation { get; set; }

        [Option("cert-password", HelpText = "The password of the certificate")]
        public string CertificatePassword { get; set; }

        [Option("message-retention-location", HelpText = "If this is defined the server will retian messages in json format at the defined location must be a file location")]
        public string MessageStorageLocation { get; set; }

        [Option("show-subscriptions", Default = false, HelpText = "If defined it will show the subscriptions made by clients")]
        public bool ShowSubscriptions { get; set; } = false;

        [Option("show-messages", Default = false, HelpText = "If defined it will show a message is received, who it was from and on what topic")]
        public bool ShowMessages { get; set; } = false;

        [Option("show-connections", Default = false, HelpText = "If defined it will show when someone connects to server and disconnects")]
        public bool ShowClientConnections { get; set; } = false;

        [Option('c', "config", Default = "./mqttserver.json", HelpText = "Defines the location of the config file. By default it will search for configs in the current directory with mqttserver.json file name. Config settings override command line settings")]
        public string ConfigurationFileLocation { get; set; } = "./mqttserver.json";

        [Option("log-level", Default = "Information", HelpText = "Log Levels are Trace, Debug, Information, Warning, Error, Critical")]
        public string LogLevel { get; set; } = "Information";

        [Option('m', "make-config", Default = false, HelpText = "When this options is defined the server will create a config in current working directory and immediately close")]
        public bool MakeConfig { get; set; }

        [Option("web-server", Default = false, HelpText = "When this option is defined the server will start a diagnostic webserver")]
        public bool StartWebServer { get; set; }

        [Option("web-port", Default = 4504, HelpText = "Defines the port the webserver will run on")]
        public int WebServerPort { get; set; } = 4504;

        [Option('w', "websocket", Default = false, HelpText = "When this option is defined the server will start websocket connection to mqtt")]
        public bool StartWebsockets { get; set; }

        [Option("websocket-port", Default = 4505, HelpText = "Defines the port the websocket accept connections on")]
        public int WebsocketPort { get; set; } = 4505;

        [Option("file-log", Default = false, HelpText = "When this option is defined a log file will be created")]
        public bool LogFiles { get; set; }

        [Option("no-console-log", Default = false, HelpText = "When this option is defined the console log will be turned off")]
        public bool NoLogConsole { get; set; }
    }

}