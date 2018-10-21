using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    public class MqttModel
    {
        public Guid Id { get; } = Guid.NewGuid();

        private const string DateFormat = "MM/dd h:mm:ss tt";

        private Dictionary<string, ServiceSubscriptions> _services = new Dictionary<string, ServiceSubscriptions>();
        private Queue<Message> _messages = new Queue<Message>();
        private int _bufferLimit = 100;
        private ILogger _logger;
        private IMqttServer _server;

        private Diagnostics _diagMessage = new Diagnostics();

        public MqttModel(ILogger<MqttModel> logger, IConfiguration config)
        {
            _logger = logger;
            _bufferLimit = config.GetValue<int>("WebServer:Buffer");
        }

        public IEnumerable<ServiceStatus> GetServices()
        {
            return _server.GetClientSessionsStatusAsync().ContinueWith(t =>
            {
                return t.Result.Select(s => new ServiceStatus
                {
                    ClientId = s.ClientId,
                    Endpoint = s.Endpoint,
                    IsConnected = s.IsConnected,
                    ProtocolVersion = s.ProtocolVersion.ToString(),
                    TimeSinceLastMessage = (int)s.LastNonKeepAlivePacketReceived.TotalSeconds,
                    TimeSinceLastKeepAlive = (int)s.LastPacketReceived.TotalSeconds,
                    PendingMessages = s.PendingApplicationMessagesCount
                });
            }).Result;
        }

        public ServiceSubscriptions GetService(string name)
        {
            if (_services.TryGetValue(name, out ServiceSubscriptions returnValue))
                return returnValue;

            var service = new ServiceSubscriptions { Name = name };

            _services.Add(name, service);

            return service;
        }

        internal void SetServer(IMqttServer server)
        {
            _server = server;
        }

        public MqttModel RemoveService(string name)
        {
            _services.Remove(name);

            return this;
        }

        public MqttModel AddMessage(string topic, string service)
        {
            _messages.Enqueue(new Message
            {
                Topic = topic,
                ServiceName = service,
                Time = DateTime.Now.ToString(DateFormat)
            });
            _diagMessage.NumMessages++;
            if(_messages.Count > _bufferLimit)
            {
                _messages.Dequeue();
            }
            return this;
        }

        public IEnumerable<Message> GetMessages()
        {
            return _messages.ToArray();
        }

        public ServiceSubscriptions[] GetSubscriptions()
        {
            return _services.Values.ToArray();
        }

        private DateTime _startTime = DateTime.Now;

        public Diagnostics GetDiagnostics()
        {
            _diagMessage.NumClients = GetServices().Count();
            _diagMessage.NumSubscriptions = _services.Values.SelectMany(s => s.Subscriptions).Distinct().Count();

            double totalMinutes = (DateTime.Now - _startTime).TotalMinutes;

            if(totalMinutes > 0)
            {
                _diagMessage.MessagesPerMinute = (int)(_diagMessage.NumMessages / totalMinutes);
                _diagMessage.MinutesSinceStart = Math.Round(totalMinutes, 2);
            }

            
            return _diagMessage;
        }
    }

    public class Diagnostics
    {
        public int NumClients { get; set; }
        public int NumSubscriptions { get; set; }
        public long NumMessages { get; set; }
        public int MessagesPerMinute { get; set; }
        public double MinutesSinceStart { get; set; }

        public List<DiagnosticValue> GetValues()
        {
            return new List<DiagnosticValue>
            {
                new DiagnosticValue("Number of Clients", NumClients.ToString()),
                new DiagnosticValue("Number of Subscriptions", NumSubscriptions.ToString()),
                new DiagnosticValue("Number of Messages", $"{NumMessages} messages"),
                new DiagnosticValue("Message Rate", $"{MessagesPerMinute} per minute"),
                new DiagnosticValue("Server Up Time", $"{MinutesSinceStart} minutes")
            };
        }
    }

    public class DiagnosticValue
    {
        public DiagnosticValue(string name, string value, double numValue = -1)
        {
            Name = name;
            Value = value;
            NumValue = numValue;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public double NumValue { get; set; }
    }

    public class Message
    {
        public string Topic { get; set; }

        public string Time { get; set; }

        public string ServiceName { get; set; }

    }

    public class ServiceStatus
    {
        public string ClientId { get; set; }
        public string Endpoint { get; set; }
        public bool IsConnected { get; set; }
        public string ProtocolVersion { get; set; }
        public int TimeSinceLastMessage { get; set; }
        public int TimeSinceLastKeepAlive { get; set; }
        public int PendingMessages { get; set; }
    }

    public class ServiceSubscriptions
    {
        private List<string> _subscriptions = new List<string>();

        public string Name { get; set; }

        public IEnumerable<string> Subscriptions { get => _subscriptions; }

        public ServiceSubscriptions AddSubscription(string subscription)
        {
            if(!_subscriptions.Any(s => s == subscription))
            {
                _subscriptions.Add(subscription);
            }

            return this;
        }
    }
}
