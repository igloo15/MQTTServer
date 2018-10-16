using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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
        public Diagnostics Index()
        {
            return _model.GetDiagnostics();
        }
    }
}
