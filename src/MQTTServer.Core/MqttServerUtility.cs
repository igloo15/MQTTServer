using Microsoft.Extensions.Logging;
using MQTTnet.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MQTTServer.Core
{
    internal static class MqttServerUtility
    {
        public static MqttServerOptionsBuilder BuildOptions(ServerOptions config, ILoggerFactory factory, Interceptors interceptors)
        {
            var logger = factory.CreateLogger("MqttServerUtility");

            MqttServerOptionsBuilder serverBuilder = new MqttServerOptionsBuilder();
            try
            {

                if (config.Encrypted && File.Exists(config.CertificateLocation))
                {

                    if (config.IPAddress == "Any")
                        serverBuilder.WithEncryptedEndpoint();
                    else if (IPAddress.TryParse(config.IPAddress, out IPAddress address))
                        serverBuilder.WithEncryptedEndpointBoundIPAddress(address);
                    else
                    {
                        logger.LogWarning($"Failed to parse provided IP Address : {config.IPAddress} using default");
                        serverBuilder.WithEncryptedEndpoint();
                    }

                    serverBuilder.WithEncryptedEndpointPort(config.Port);

                    var certificate = new X509Certificate2(config.CertificateLocation, config.CertificatePassword);
                    var certBytes = certificate.Export(X509ContentType.Cert);

                    serverBuilder.WithEncryptionCertificate(certBytes);

                }
                else
                {
                    if (config.IPAddress == "Any")
                        serverBuilder.WithDefaultEndpoint();
                    else if (IPAddress.TryParse(config.IPAddress, out IPAddress address))
                        serverBuilder.WithDefaultEndpointBoundIPAddress(address);
                    else
                    {
                        logger.LogWarning("Failed to parse provided IP Address : {IPAddress} using default", config.IPAddress);
                        serverBuilder.WithDefaultEndpoint();
                    }

                    serverBuilder.WithDefaultEndpointPort(config.Port);
                }

                if (config.Persistent)
                    serverBuilder.WithPersistentSessions();

                if (!String.IsNullOrEmpty(config.MessageStorageLocation))
                    serverBuilder.WithStorage(new RetainedMessageHandler(config.MessageStorageLocation));

                if (config.ShowSubscriptions)
                    serverBuilder.WithSubscriptionInterceptor(interceptors.SubscriptionInterceptor);

                if (config.ShowMessages)
                    serverBuilder.WithApplicationMessageInterceptor(interceptors.MessageInterceptor);

                if (config.ShowClientConnections)
                    serverBuilder.WithConnectionValidator(interceptors.ConnectionInterceptor);

                serverBuilder
                    .WithConnectionBacklog(config.ConnectionBacklog)
                    .WithMaxPendingMessagesPerClient(config.MaxPendingMessagesPerClient)
                    .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(config.CommunicationTimeout));


            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception occured during Initialization of Server");
            }

            return serverBuilder;
        }

        public static Dictionary<string, string> GetOptions(object value, string baseName)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            Type myType = value.GetType();
            if(value is IDictionary myDictionary)
            {
                return GetOptionsDictionary(myDictionary, baseName);
            }


            foreach(var prop in myType.GetPublicProperties())
            {
                var propValue = prop.GetValue(value);
                if (propValue == null)
                    continue;

                var propName = !string.IsNullOrEmpty(baseName) ? $"{baseName}:{prop.Name}" : prop.Name;

                var propOptions = GetOptions(propValue, propName);

                if(propOptions.Count > 0)
                {
                    foreach(var kvp in propOptions)
                    {
                        options.Add(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    options[propName] = propValue.ToString();
                }
            }

            return options;
        }

        public static Dictionary<string, string> GetOptionsDictionary(IDictionary value, string baseName)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (var key in value.Keys)
            {
                var item = value[key];

                var propName = !string.IsNullOrEmpty(baseName) ? $"{baseName}:{key.ToString()}" : key.ToString();

                var propOptions = GetOptions(item, propName);

                if (propOptions.Count > 0)
                {
                    foreach (var kvp in propOptions)
                    {
                        options.Add(kvp.Key, kvp.Value);
                    }
                }
                else
                {
                    options[propName] = item.ToString();
                }

            }
            return options;
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type == typeof(string))
                return Enumerable.Empty<PropertyInfo>().ToArray();

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Where(x => !x.IsSpecialName && x.FilterMemberInfo())
                .Where(FilterPropertyInfo)
                .ToArray();
        }

        private static bool FilterMemberInfo(this MemberInfo info)
        {
            return !info.GetCustomAttributes<ObsoleteAttribute>().Any();
        }

        private static bool FilterPropertyInfo(this PropertyInfo info)
        {
            var get = info.GetGetMethod(true);
            var set = info.GetSetMethod(true);
            if (get != null && set != null)
            {
                return !(get.IsPrivate && set.IsPrivate);
            }
            else if (get != null)
            {
                return !get.IsPrivate;
            }
            else if (set != null)
            {
                return !set.IsPrivate;
            }
            else
            {
                return false;
            }
        }
    }
}
