using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace MQTTServer.Core
{
    internal class Interceptors
    {
        private ILogger _messageLogger = null;
        private ILogger _subscriptionLogger = null;
        private ILogger _connectionLogger = null;
        private ILogger _serverLogger;
        private MqttModel _model;
        private IMqttServer _server;
        private MqttSettings _settings;
        private IObservable<MqttApplicationMessageReceivedEventArgs> _messageStream;

        public Interceptors(ILoggerFactory factory, MqttModel model, MqttSettings settings)
        {
            _serverLogger = factory.CreateLogger("MQTTServer.ServerLogger");

            if (settings.Server.ShowMessages)
                _messageLogger = factory.CreateLogger("MQTTServer.MessageLogger");

            if (settings.Server.ShowSubscriptions)
                _subscriptionLogger = factory.CreateLogger("MQTTServer.SubscriptionLogger");

            if(settings.Server.ShowClientConnections)
                _connectionLogger = factory.CreateLogger("MQTTServer.ConnectionLogger");

            _model = model;
            _settings = settings;
        }

        public void SetServer(IMqttServer server)
        {
            _server = server;
            if (_settings.Server.ShowMessages || _settings.Server.StartDiagWebServer)
            {
                _messageStream = Observable
                    .FromEventPattern<MqttApplicationMessageReceivedEventArgs>(h => _server.ApplicationMessageReceived += h, h => _server.ApplicationMessageReceived -= h)
                    .Select(e => e.EventArgs);

                _messageStream.Subscribe(MessageReceived);
            }
                

            if (_settings.Server.ShowClientConnections || _settings.Server.StartDiagWebServer)
            {
                _server.ClientConnected += ClientConnection;
                _server.ClientDisconnected += ClientDisconnected;
            }

            if (_settings.Server.ShowSubscriptions || _settings.Server.StartDiagWebServer)
            {
                _server.ClientSubscribedTopic += ClientSubscribedTopic;
                _server.ClientUnsubscribedTopic += ClientUnsubscribedTopic;
            }

            if(_settings.Server.StartDiagWebServer)
            {
                _model?.SetServer(_server);
            }
            else
            {
                _model = null;
            }

            _server.Started += ServerStarted;
            _server.Stopped += ServerStopped;
        }

        private void ServerStopped(object sender, EventArgs e)
        {
            _serverLogger?.LogInformation("MqttServer stopped");
            _model?.Stop();
        }

        private void ServerStarted(object sender, EventArgs e)
        {
            _serverLogger?.LogInformation("MqttServer started");
            _model?.Start();
        }

        private void ClientUnsubscribedTopic(object sender, MqttClientUnsubscribedTopicEventArgs e)
        {
            _subscriptionLogger?.LogInformation("Client {ClientId} unsubscribes from Topic {Topic}", e.ClientId, e.TopicFilter);
            _model?.GetService(e.ClientId)?.Subscriptions.RemoveSubscription(e.TopicFilter);
        }

        private void ClientSubscribedTopic(object sender, MqttClientSubscribedTopicEventArgs e)
        {
            _subscriptionLogger?.LogInformation("Client {ClientId} subscribes to Topic {Topic}", e.ClientId, e.TopicFilter.Topic);
            _model?.GetService(e.ClientId)?.Subscriptions.AddSubscription(e.TopicFilter.Topic);
        }

        private void ClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _connectionLogger?.LogInformation("Client {ClientId} has disconnected from server", e.ClientId);
            _model?.RemoveService(e.ClientId);
        }

        private void ClientConnection(object sender, MqttClientConnectedEventArgs e)
        {
            _connectionLogger?.LogInformation("Client {ClientId} has connected to server", e.ClientId);
            _model?.AddService(e.ClientId);
        }

        private void MessageReceived(MqttApplicationMessageReceivedEventArgs next)
        {
            _messageLogger?.LogInformation("Client {ClientId} sent message to Topic {Topic}", next.ClientId, next.ApplicationMessage.Topic);
            string content = null;
            if (_settings.Server.ShowMessageContent)
            {
                try
                {
                    content = Encoding.UTF8.GetString(next.ApplicationMessage.Payload);
                }
                catch (Exception)
                {

                }
            }

            _model?.AddMessage(next.ApplicationMessage.Topic, next.ClientId, content);
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
