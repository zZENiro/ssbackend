using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSBackendApp.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class EconomyController : Controller
    {
        private readonly IConnectionMultiplexer _cache;
        private readonly IDatabase _db;

        public EconomyController(IConnectionMultiplexer cache)
        {
            _cache = cache;
            _db = _cache.GetDatabase();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetGDP([FromQuery] string year, string part)
        {
            var cacheKey = $"{year}:{part}";

            var res = await _db.StringGetAsync(cacheKey);

            return new JsonResult(new { Result = new GDP { Year = year, Part = part, Value = res } });
        }
    }

    public class GDP
    {
        public string Year { get; set; }

        public string Part { get; set; }

        public string Value { get; set; }
    }
}
