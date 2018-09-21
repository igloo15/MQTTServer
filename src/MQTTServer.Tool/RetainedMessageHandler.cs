using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Igloo15.MQTTServer.Tool
{
    internal class RetainedMessageHandler : IMqttServerStorage
    {
        private string _location;

        public RetainedMessageHandler(string location)
        {
            _location = location;
        }

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            return Task.Run(() =>
            {
                File.WriteAllText(_location, JsonConvert.SerializeObject(messages));
            });
        }

        public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            return Task.Run(() => 
            {
                IList<MqttApplicationMessage> messages;
                if(File.Exists(_location))
                {
                    var json = File.ReadAllText(_location);
                    messages = JsonConvert.DeserializeObject<List<MqttApplicationMessage>>(json);
                }
                else
                {
                    return new List<MqttApplicationMessage>();
                }
                return messages;

            });
        }
    }
}
