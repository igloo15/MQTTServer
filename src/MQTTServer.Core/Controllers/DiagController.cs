using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MQTTServer.Core.Model;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MQTTServer.Core.Controllers
{
    [Route("api/[controller]")]
    public class DiagController : Controller
    {
        private MqttModel _model;

        public DiagController(MqttModel model)
        {
            _model = model;
        }

        // GET: /<controller>/
        [HttpGet("Status")]
        public DiagnosticValue[] GetStatus()
        {
            return _model.GetDiagnostics().GetValues().ToArray();
        }

        // GET: /<controller>/Version
        [HttpGet("Version")]
        public string GetVersion()
        {
            var assembly = typeof(DiagController).Assembly;
            var assemblyName = assembly.GetName().Name;
            var gitVersionInformationType = assembly.GetType("GitVersionInformation");
            var versionField = gitVersionInformationType?.GetField("FullSemVer");

            return versionField?.GetValue(null)?.ToString();
        }

        [HttpGet("Subscriptions")]
        public ServiceSubscriptions[] GetSubscriptions()
        {
            return _model.GetSubscriptions();
        }

        [HttpGet("Endpoint")]
        public string GetEndpoint()
        {
            return _model.GetEndpoint();
        }
    }
}
