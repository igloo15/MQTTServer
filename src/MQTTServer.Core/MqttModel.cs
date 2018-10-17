using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    public class MqttModel
    {
        private const string DateFormat = "MM/dd h:mm:ss tt";

        private Dictionary<string, Service> _services = new Dictionary<string, Service>();
        private Queue<Message> _messages = new Queue<Message>();
        private int _bufferLimit = 100;
        private ILogger _logger;

        private Diagnostics _diagMessage = new Diagnostics();

        public MqttModel(ILogger<MqttModel> logger, IConfiguration config)
        {
            _logger = logger;
            _bufferLimit = config.GetValue<int>("WebServer:Buffer");
        }

        public IEnumerable<Service> GetServices()
        {
            return _services.Values;
        }

        public MqttModel AddService(string name, string endpoint)
        {
            if(!_services.ContainsKey(name))
            {
                _services.Add(name, new Service { Name = name, Connected = DateTime.Now.ToString(DateFormat), EndPoint = endpoint });
                _logger.LogDebug("Service {serviceName} added", name);
            }
            else
            {
                _logger.LogWarning("Service {serviceName} already exists can't be added again", name);
            }

            return this;
        }

        public Service GetService(string name)
        {
            if (_services.TryGetValue(name, out Service returnValue))
                return returnValue;

            return null;
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

        
        private DateTime _startTime = DateTime.Now;

        public Diagnostics GetDiagnostics()
        {
            _diagMessage.NumClients = _services.Count;
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
        public int NumMessages { get; set; }
        public int MessagesPerMinute { get; set; }
        public double MinutesSinceStart { get; set; }
    }

    public class Message
    {
        public string Topic { get; set; }

        public string Time { get; set; }

        public string ServiceName { get; set; }

    }

    public class Service
    {
        private List<string> _subscriptions = new List<string>();

        public string Name { get; set; }

        public string Connected { get; set; }

        public string EndPoint { get; set; }

        public IEnumerable<string> Subscriptions { get => _subscriptions; }

        public Service AddSubscription(string subscription)
        {
            if(!_subscriptions.Any(s => s == subscription))
            {
                _subscriptions.Add(subscription);
            }

            return this;
        }
    }
}
