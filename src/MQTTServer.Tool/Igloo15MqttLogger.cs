using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Igloo15.MQTTServer.Tool
{
    internal class Igloo15MqttLogger : IMqttNetLogger
    {
        private ILogger _logger;
        private ILoggerFactory _factory;

        public Igloo15MqttLogger(ILoggerFactory factory, string category = "IMqttLogger")
        {
            _factory = factory;

            category = category ?? "IMqttLogger";

            _logger = factory.CreateLogger(category);
        }

        public IMqttNetChildLogger CreateChildLogger(string source = null)
        {
            return new MqttNetChildLogger(new Igloo15MqttLogger(_factory, source), source);
        }

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    _logger.LogDebug(message, parameters);
                    break;
                case MqttNetLogLevel.Info:
                    _logger.LogInformation(message, parameters);
                    break;
                case MqttNetLogLevel.Warning:
                    _logger.LogWarning(exception, message, parameters);
                    break;
                case MqttNetLogLevel.Error:
                    _logger.LogError(exception, message, parameters);
                    break;
                default:
                    _logger.LogDebug(message, parameters);
                    break;
            }

        }

        public event EventHandler<MqttNetLogMessagePublishedEventArgs> LogMessagePublished;
    }
}
