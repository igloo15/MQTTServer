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
        private IMqttServer _server;
        private MqttSettings _settings;

        public Interceptors(ILoggerFactory factory, MqttModel model, MqttSettings settings)
        {
            _messageLogger = factory.CreateLogger("MQTTServer.MessageLogger");
            _subscriptionLogger = factory.CreateLogger("MQTTServer.SubscriptionLogger");
            _connectionLogger = factory.CreateLogger("MQTTServer.ConnectionLogger");
            _model = model;
            _settings = settings;
        }

        public void SetServer(IMqttServer server)
        {
            _server = server;

            if(_settings.Server.ShowMessages)
                _server.ApplicationMessageReceived += MessageReceived;

            if (_settings.Server.ShowClientConnections)
            {
                _server.ClientConnected += ClientConnection;
                _server.ClientDisconnected += ClientDisconnected;
            }

            if (_settings.Server.ShowSubscriptions)
            {
                _server.ClientSubscribedTopic += ClientSubscribedTopic;
                _server.ClientUnsubscribedTopic += ClientUnsubscribedTopic;
            }
            
            if(_settings.Server.StartDiagWebServer)
            {
                _model.SetServer(_server);
            }
        }

        private void ClientUnsubscribedTopic(object sender, MqttClientUnsubscribedTopicEventArgs e)
        {
            _subscriptionLogger.LogInformation("Client {ClientId} unsubscribes from Topic {Topic}", e.ClientId, e.TopicFilter);
        }

        private void ClientSubscribedTopic(object sender, MqttClientSubscribedTopicEventArgs e)
        {
            _subscriptionLogger.LogInformation("Client {ClientId} subscribes to Topic {Topic}", e.ClientId, e.TopicFilter.Topic);
            _model?.GetService(e.ClientId)?.AddSubscription(e.TopicFilter.Topic);
        }

        private void ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _connectionLogger.LogInformation("Client {ClientId} has disconnected from server", e.ClientId);
            _model?.RemoveService(e.ClientId);
        }

        private void ClientConnection(object sender, MqttClientConnectedEventArgs e)
        {
            _connectionLogger.LogInformation("Client {ClientId} has connected to server", e.ClientId);
        }

        private void MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            _messageLogger.LogInformation("Client {ClientId} sent message to Topic {Topic}", e.ClientId, e.ApplicationMessage.Topic);
            
            _model?.AddMessage(e.ApplicationMessage.Topic, e.ClientId);
        }

        public void SubscriptionInterceptor(MqttSubscriptionInterceptorContext context)
        {
            
        }

        public void MessageInterceptor(MqttApplicationMessageInterceptorContext context)
        {
            
        }

        public void ConnectionInterceptor(MqttConnectionValidatorContext context)
        {
        }
    }
}
