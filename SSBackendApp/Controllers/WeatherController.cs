using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SSBackendApp.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class WeatherController : Controller
    {
       
    }

    public class WeatherFrame
    {
        public string Date { get; set; }

        public string Temperature { get; set; }
    }
}
