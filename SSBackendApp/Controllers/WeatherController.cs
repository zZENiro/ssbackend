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
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetWeather([FromQuery] string startDate, string endDate, string city)
        {
            var queryLink = $"http://api.worldweatheronline.com/premium/v1/past-weather.ashx?key=2bd4075751474daca7c145854202711&q={city}&date={startDate}&enddate={endDate}";

            var request = (HttpWebRequest)WebRequest.Create(queryLink);

            request.Method = "GET";

            var result = request.GetResponse();

            string response = string.Empty;

            var dates = new List<string>();
            var avgTemps = new List<string>();
            var weatherFrames = new List<WeatherFrame>();

            using (var rs = result.GetResponseStream())
            using (var sr = new StreamReader(rs))
            {
                response = await sr.ReadToEndAsync();

                var xml = XDocument.Parse(response);

                foreach (var root in xml.Elements())
                {
                    foreach (var weatherRoot in root.Elements())
                    {
                        foreach (var el in weatherRoot.Elements())
                        {
                            if (el.Name == "date")
                                dates.Add(el.Value);

                            if (el.Name == "avgtempC")
                                avgTemps.Add(el.Value);
                        }
                    }
                }
            }

            for (int i = 0; i < avgTemps.Count; ++i)
                weatherFrames.Add(new WeatherFrame() { Date = dates[i], Temperature = avgTemps[i] });

            return new ContentResult() { Content = JsonSerializer.Serialize<List<WeatherFrame>>(weatherFrames), ContentType = "application/json" };
        }
    }

    public class WeatherFrame
    {
        public string Date { get; set; }

        public string Temperature { get; set; }
    }
}
