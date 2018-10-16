using Microsoft.Extensions.Logging;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Igloo15.MQTTServer.Tool
{
    internal class Interceptors
    {
        private ILogger _messageLogger;
        private ILogger _subscriptionLogger;
        private ILogger _connectionLogger;

        public Interceptors(ILoggerFactory factory)
        {
            _messageLogger = factory.CreateLogger("MQTTServer.MessageLogger");
            _subscriptionLogger = factory.CreateLogger("MQTTServer.SubscriptionLogger");
            _connectionLogger = factory.CreateLogger("MQTTServer.ConnectionLogger");
        }

        public void SubscriptionInterceptor(MqttSubscriptionInterceptorContext context)
        {
            _subscriptionLogger.LogInformation("Client {ClientId} subscribes to Topic {Topic}", context.ClientId, context.TopicFilter.Topic);
        }

        public void MessageInterceptor(MqttApplicationMessageInterceptorContext context)
        {
            _messageLogger.LogInformation("Client {ClientId} sent message to Topic {Topic}", context.ClientId, context.ApplicationMessage.Topic);
        }

        public void ConnectionInterceptor(MqttConnectionValidatorContext context)
        {
            
            _connectionLogger.LogInformation("Client {ClientId} has connected to server from {Address}", context.ClientId, context.Endpoint);
        }
    }
}
