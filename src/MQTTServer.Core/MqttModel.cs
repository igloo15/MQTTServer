using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet.Server;
using MQTTServer.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    public class MqttModel
    {
        public Guid Id { get; } = Guid.NewGuid();

        internal const string DateFormat = "MM/dd h:mm:ss tt";

        private Dictionary<string, Service> _services = new Dictionary<string, Service>();
        private Queue<Message> _messages = new Queue<Message>();
        private int _bufferLimit = 100;
        private ILogger _logger;
        private IMqttServer _server;
        private Stopwatch sw;

        private Diagnostics _diagMessage = new Diagnostics();

        public MqttModel(ILogger<MqttModel> logger, MqttSettings settings)
        {
            _logger = logger;
            _bufferLimit = settings.WebServer.BufferSize;

        }

        internal void Start()
        {
            sw = Stopwatch.StartNew();
        }

        internal void Stop()
        {
            sw?.Stop();
            sw = null;
            _services = new Dictionary<string, Service>();
            _messages = new Queue<Message>();
            _diagMessage = new Diagnostics();
        }

        internal void SetServer(IMqttServer server)
        {
            _server = server;
        }

        public IEnumerable<ServiceStatus> GetServices()
        {
            var results = _server.GetClientSessionsStatusAsync().Result;

            foreach(var result in results)
            {
                var service = GetService(result.ClientId);
                service?.Status.Update(result);
            }
            
            return _services.Values.Select(s => s.Status);
        }

        public Service GetService(string name)
        {
            if (_services.TryGetValue(name, out Service returnValue))
                return returnValue;

            return null;
        }

        public MqttModel AddService(string name)
        {
            lock (_services)
            {
                if (_services.ContainsKey(name))
                    _logger.LogWarning("{ClientId} Service already exists in model", name);
                else
                    _services.Add(name, new Service(name));
            }
            

            return this;
        }

        public MqttModel RemoveService(string name)
        {
            lock (_services)
            {
                _services.Remove(name);
            }
            return this;
        }

        public MqttModel AddMessage(string topic, string service, string message = null)
        {
            _messages.Enqueue(new Message
            {
                Topic = topic,
                ClientId = service,
                Time = DateTime.Now.ToString(DateFormat),
                Content = message
            });
            _diagMessage.NumMessages++;
            if (_messages.Count > _bufferLimit)
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
            return _services.Values.Select(s => s.Subscriptions).ToArray();
        }

        public Diagnostics GetDiagnostics()
        {
            _diagMessage.NumClients = _services.Count;
            _diagMessage.NumSubscriptions = _services.Values.SelectMany(s => s.Subscriptions.Subscriptions).Distinct().Count();

            double totalSeconds = sw.Elapsed.TotalSeconds;

            if (totalSeconds > 0)
            {
                _diagMessage.MessagesPerSecond = Math.Round(_diagMessage.NumMessages / totalSeconds, 2);
                _diagMessage.MinutesSinceStart = Math.Round(totalSeconds/60, 2);
            }

            return _diagMessage;
        }

        public string GetEndpoint()
        {
            return $"{_server.Options.DefaultEndpointOptions.BoundInterNetworkAddress}:{_server.Options.DefaultEndpointOptions.Port}";
        }
    }

}
