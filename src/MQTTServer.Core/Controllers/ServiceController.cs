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
    public class ServiceController : Controller
    {
        private MqttModel _model;

        public ServiceController(MqttModel model)
        {
            _model = model;
        }

        // GET: /<controller>/
        public IEnumerable<ServiceStatus> Index()
        {
            return _model?.GetServices();
        }
    }
}
