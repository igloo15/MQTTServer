using Microsoft.Extensions.Logging;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTServer.Core
{
    internal class Interceptors
    {
        private ILogger _messageLogger;
        private ILogger _subscriptionLogger;
        private ILogger _connectionLogger;
        private MqttModel _model;

        public Interceptors(ILoggerFactory factory, MqttModel model)
        {
            _messageLogger = factory.CreateLogger("MQTTServer.MessageLogger");
            _subscriptionLogger = factory.CreateLogger("MQTTServer.SubscriptionLogger");
            _connectionLogger = factory.CreateLogger("MQTTServer.ConnectionLogger");
            _model = model;
        }

        public void SubscriptionInterceptor(MqttSubscriptionInterceptorContext context)
        {
            _subscriptionLogger.LogInformation("Client {ClientId} subscribes to Topic {Topic}", context.ClientId, context.TopicFilter.Topic);

            _model?.GetService(context.ClientId)?.AddSubscription(context.TopicFilter.Topic);
        }

        public void MessageInterceptor(MqttApplicationMessageInterceptorContext context)
        {
            _messageLogger.LogInformation("Client {ClientId} sent message to Topic {Topic}", context.ClientId, context.ApplicationMessage.Topic);

            _model?.AddMessage(context.ApplicationMessage.Topic, context.ClientId);
        }

        public void ConnectionInterceptor(MqttConnectionValidatorContext context)
        {
            if(context.ReturnCode == MQTTnet.Protocol.MqttConnectReturnCode.ConnectionAccepted)
            {
                _connectionLogger.LogInformation("Client {ClientId} has connected to server from {Address}", context.ClientId, context.Endpoint);
                _model?.AddService(context.ClientId, context.Endpoint);
            }
        }
    }
}
