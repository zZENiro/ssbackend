using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSBackendApp.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class EconomyController : Controller
    {
        private readonly IDistributedCache _cache;

        public EconomyController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetGDP([FromQuery] string year, string part)
        {
            var cacheKey = $"{year}:{part}";

            var result = await _cache.GetStringAsync(cacheKey);

            return new JsonResult(new { Result = result });
        }
    }

    public class GDP
    {
        public string Year { get; set; }

        public string Part { get; set; }

        public string Value { get; set; }
    }
}
