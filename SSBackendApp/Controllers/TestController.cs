using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSBackendApp.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        public TestController()
        {

        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetValue() => new JsonResult(new { result = "Hello, World" });
    }
}
