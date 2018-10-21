using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core.Model
{
    public class Diagnostics
    {
        public int NumClients { get; set; }
        public int NumSubscriptions { get; set; }
        public long NumMessages { get; set; }
        public double MessagesPerSecond { get; set; }
        public double MinutesSinceStart { get; set; }

        public List<DiagnosticValue> GetValues()
        {
            return new List<DiagnosticValue>
            {
                new DiagnosticValue("Number of Clients", NumClients.ToString(), NumClients),
                new DiagnosticValue("Number of Subscriptions", NumSubscriptions.ToString(), NumSubscriptions),
                new DiagnosticValue("Number of Messages", $"{NumMessages} messages", NumMessages),
                new DiagnosticValue("Message Rate", $"{MessagesPerSecond} per second", MessagesPerSecond),
                MinutesSinceStart < 60 ? new DiagnosticValue("Server Up Time", $"{MinutesSinceStart} minutes", MinutesSinceStart) :
                MinutesSinceStart < (60 * 24) ? new DiagnosticValue("Server Up Time", $"{Math.Round(MinutesSinceStart/60, 2)} hours", MinutesSinceStart/60) :
                new DiagnosticValue("Server Up Time", $"{Math.Round(MinutesSinceStart/(60*24), 2)} days", MinutesSinceStart/(60*24))
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
        public string ClientId { get; set; }
        public string Content { get; set; }
    }

    public class Service
    {
        public Service(string clientId)
        {
            Status = new ServiceStatus { ClientId = clientId, TimeConnected = DateTime.Now.ToString(MqttModel.DateFormat) };
            Subscriptions = new ServiceSubscriptions { ClientId = clientId };
        }

        public ServiceStatus Status { get; } = new ServiceStatus();

        public ServiceSubscriptions Subscriptions { get; } = new ServiceSubscriptions();
    }

    public class ServiceStatus
    {
        public string ClientId { get; set; }
        public string TimeConnected { get; set; }
        public string Endpoint { get; set; }
        public bool IsConnected { get; set; }
        public string ProtocolVersion { get; set; }
        public int TimeSinceLastMessage { get; set; }
        public int TimeSinceLastNonKeepAlive { get; set; }
        public int PendingMessages { get; set; }

        public ServiceStatus Update(IMqttClientSessionStatus status)
        {
            IsConnected = status.IsConnected;
            TimeSinceLastMessage = (int)status.LastPacketReceived.TotalSeconds;
            TimeSinceLastNonKeepAlive = (int)status.LastNonKeepAlivePacketReceived.TotalSeconds;
            PendingMessages = status.PendingApplicationMessagesCount;

            if (ProtocolVersion == null)
                ProtocolVersion = status.ProtocolVersion.ToString();

            if(Endpoint == null)
                Endpoint = status.Endpoint;

            return this;
        }
    }

    public class ServiceSubscriptions
    {
        private List<string> _subscriptions = new List<string>();

        public string ClientId { get; set; }

        public IEnumerable<string> Subscriptions { get => _subscriptions; }

        public ServiceSubscriptions AddSubscription(string subscription)
        {
            if (!_subscriptions.Any(s => s == subscription))
                _subscriptions.Add(subscription);

            return this;
        }

        public ServiceSubscriptions RemoveSubscription(string subscription)
        {
            _subscriptions.Remove(subscription);
            return this;
        }
    }
}
