using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using SSBackendApp.Cache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SSBackendApp.Controllers
{

    public class Rootobject
    {
        public City city { get; set; }
        public string cod { get; set; }
        public float message { get; set; }
        public int cnt { get; set; }
        public List[] list { get; set; }
    }

    public class City
    {
        public int id { get; set; }
        public string name { get; set; }
        public Coord coord { get; set; }
        public string country { get; set; }
        public int population { get; set; }
        public int timezone { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class List
    {
        public int dt { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public Temp temp { get; set; }
        public Feels_Like feels_like { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public Weather[] weather { get; set; }
        public float speed { get; set; }
        public int deg { get; set; }
        public int clouds { get; set; }
        public int pop { get; set; }
    }

    public class Temp
    {
        public float day { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public float night { get; set; }
        public float eve { get; set; }
        public float morn { get; set; }
    }

    public class Feels_Like
    {
        public float day { get; set; }
        public float night { get; set; }
        public float eve { get; set; }
        public float morn { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }


    [Controller]
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly List<FeaturesCache> _features;
        private readonly StringBuilder _weatherKey;
        private readonly StringBuilder _energyKey;
        private readonly DateTime _startTime = DateTime.Parse("2010-06-22");

        private DateTime _currentDate;
        private NumberFormatInfo numberFormatInfo;

        public DataController(
            IEnumerable<FeaturesCache> features)
        {
            _features = (List<FeaturesCache>)features;
            _weatherKey = new StringBuilder();
            _energyKey = new StringBuilder();

            numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";
        }

        //[HttpGet]
        //[Route("[action]")]
        //public async Task<IActionResult> GetWeather([FromQuery] string startData, string endDate, string step)
        //{

        //}

        List<string> SelectEachNElement(List<string> collection, int frequency)
        {
            int iterations = (collection.Count() / frequency) - 1;
            List<string> result = new List<string>();

            for (int i = 0; i < iterations; i++)
                result.Add(collection[(frequency * i)]);

            return result;
        }

        async Task<Rootobject> GetWeather()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://community-open-weather-map.p.rapidapi.com/forecast/daily?q=Perm"),
                Headers =
                {
                    { "x-rapidapi-key", "7d08d6e3f5mshf73ceec396f1ea0p1ee665jsn4a16378d35f7" },
                    { "x-rapidapi-host", "community-open-weather-map.p.rapidapi.com" },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Rootobject>(body);
            }
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetPrediction([FromQuery] string startDate, string endDate, string terr, string step, bool covidEnabled = true)
        {
            if (startDate is null || endDate is null)
                return BadRequest(new { message = "Set startDate, endDate" });

            var _startDate = DateTime.Parse(startDate);
            var _endDate = DateTime.Parse(endDate);
            var _step = step == "mounth" ? 30 : 1;

            var timedelta = (_endDate - _startDate).Days;

            var startIndex = (_startDate - _startTime).Days - 1;

            var prediction = new List<double>();
            var timeFrames = new List<string>();
            var actual = new List<string>();

            var diff_after_now = (_endDate - DateTime.Now).Days; // > 7
            var diff_before_now = (DateTime.Now - _startDate).Days;
            var diff_sum = diff_after_now + diff_before_now;

            for (int i = 0; i < diff_sum; i += _step)
            {
                _currentDate = _startDate.Add(TimeSpan.FromDays(i));
                timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");
            }

            // if predict more than 7 days
            if (_endDate > DateTime.Now && (_endDate - DateTime.Now).Days > 7)
            {
                var weatherPredict = (await GetWeather()).list;

                var first7daysCounter = 0;

                var night = _features.Select(f => double.Parse(f.NightDuration, numberFormatInfo)).ToList<double>();
                var newYear = _features.Select(f => double.Parse(f.NewYear, numberFormatInfo)).ToList<double>();
                var holiday = _features.Select(f => double.Parse(f.Holliday, numberFormatInfo)).ToList<double>();
                var sunday = _features.Select(f => double.Parse(f.Sunday, numberFormatInfo)).ToList<double>();
                var saturday = _features.Select(f => double.Parse(f.Saturday, numberFormatInfo)).ToList<double>();

                for (int i = startIndex; i < startIndex + diff_before_now; i += _step)
                {
                    prediction.Add((int.Parse(_features[i].NightDuration, numberFormatInfo) * PredictionModelConfiguration.NightDurationWeight) +
                               (double.Parse(_features[i].Weather, numberFormatInfo) * PredictionModelConfiguration.WeatherWeight) +
                               (double.Parse(_features[i].NewYear, numberFormatInfo) * PredictionModelConfiguration.NewYearWeight) +
                               (double.Parse(_features[i].Holliday, numberFormatInfo) * PredictionModelConfiguration.HolidayWeight) +
                               (int.Parse(_features[i].Sunday, numberFormatInfo) * PredictionModelConfiguration.SundayWeight) +
                               (int.Parse(_features[i].Saturday, numberFormatInfo) * PredictionModelConfiguration.SaturdayWeight) +
                               (double.Parse(_features[i].CovidCases, numberFormatInfo) * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);

                    actual.Add(_features[i]?.Target);
                }
                for (int i = startIndex + diff_before_now; i < startIndex + diff_sum; i += _step)
                {
                    var avg_night = (night[i - 365] + night[i - (365 * 2)] + night[i - (365 * 3)] + night[i - (365 * 4)] + night[i - (365 * 5)] + night[i - (365 * 6)] + night[i - (365 * 7)] + night[i - (365 * 8)]) / 8;
                    var avg_newYear = (newYear[i - 365] + newYear[i - (365 * 2)] + newYear[i - (365 * 3)] + newYear[i - (365 * 4)] + newYear[i - (365 * 5)] + newYear[i - (365 * 6)] + newYear[i - (365 * 7)] + newYear[i - (365 * 8)]) / 8;
                    var avg_holiday = (holiday[i - 365] + holiday[i - (365 * 2)] + holiday[i - (365 * 3)] + holiday[i - (365 * 4)] + holiday[i - (365 * 5)] + holiday[i - (365 * 6)] + holiday[i - (365 * 7)] + holiday[i - (365 * 8)]) / 8;
                    var avg_sunday = (sunday[i - 365] + sunday[i - (365 * 2)] + sunday[i - (365 * 3)] + sunday[i - (365 * 4)] + sunday[i - (365 * 5)] + sunday[i - (365 * 6)] + sunday[i - (365 * 7)] + sunday[i - (365 * 8)]) / 8;
                    var avg_saturday = (saturday[i - 365] + saturday[i - (365 * 2)] + saturday[i - (365 * 3)] + saturday[i - (365 * 4)] + saturday[i - (365 * 5)] + saturday[i - (365 * 6)] + saturday[i - (365 * 7)] + saturday[i - (365 * 8)]) / 8;
                    double covid_enabled = covidEnabled ? 1f : 0f;

                    if (first7daysCounter < 7)
                    {
                        prediction.Add(
                               (avg_night * PredictionModelConfiguration.NightDurationWeight) +
                               ((weatherPredict[first7daysCounter].temp.day - 273) * PredictionModelConfiguration.WeatherWeight) +
                               (avg_newYear * PredictionModelConfiguration.NewYearWeight) +
                               (avg_holiday * PredictionModelConfiguration.HolidayWeight) +
                               (avg_sunday * PredictionModelConfiguration.SundayWeight) +
                               (avg_saturday * PredictionModelConfiguration.SaturdayWeight) +
                               (covid_enabled * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);
                        ++first7daysCounter;
                        actual.Add(_features[i]?.Target);
                        continue;
                    }

                    double avg_weather = SelectEachNElement(_features.Select(el => el.Weather).ToList<string>(), 365).Select(el => double.Parse(el, numberFormatInfo)).Average();

                    prediction.Add((avg_night * PredictionModelConfiguration.NightDurationWeight) +
                                (avg_weather * PredictionModelConfiguration.WeatherWeight) +
                                (avg_newYear * PredictionModelConfiguration.NewYearWeight) +
                                (avg_holiday * PredictionModelConfiguration.HolidayWeight) +
                                (avg_sunday * PredictionModelConfiguration.SundayWeight) +
                                (avg_saturday * PredictionModelConfiguration.SaturdayWeight) +
                                (covid_enabled * PredictionModelConfiguration.CovidCasesWeight) +
                                PredictionModelConfiguration.Bies);
                    actual.Add(_features[i]?.Target);
                }

                return new JsonResult(new { x = timeFrames, y = prediction, actual_y = actual });
            }
            // if less than 7 days
            else if (_endDate > DateTime.Now && (_endDate - DateTime.Now).Days <= 7)
            {
                var weatherPredict = (await GetWeather()).list;

                var first7daysCounter = 0;

                var night = _features.Select(f => double.Parse(f.NightDuration, numberFormatInfo)).ToList<double>();
                var newYear = _features.Select(f => double.Parse(f.NewYear, numberFormatInfo)).ToList<double>();
                var holiday = _features.Select(f => double.Parse(f.Holliday, numberFormatInfo)).ToList<double>();
                var sunday = _features.Select(f => double.Parse(f.Sunday, numberFormatInfo)).ToList<double>();
                var saturday = _features.Select(f => double.Parse(f.Saturday, numberFormatInfo)).ToList<double>();


                // before now
                for (int i = startIndex; i < startIndex + diff_before_now; i += _step)
                {
                    prediction.Add((int.Parse(_features[i].NightDuration, numberFormatInfo) * PredictionModelConfiguration.NightDurationWeight) +
                               (double.Parse(_features[i].Weather, numberFormatInfo) * PredictionModelConfiguration.WeatherWeight) +
                               (double.Parse(_features[i].NewYear, numberFormatInfo) * PredictionModelConfiguration.NewYearWeight) +
                               (double.Parse(_features[i].Holliday, numberFormatInfo) * PredictionModelConfiguration.HolidayWeight) +
                               (int.Parse(_features[i].Sunday, numberFormatInfo) * PredictionModelConfiguration.SundayWeight) +
                               (int.Parse(_features[i].Saturday, numberFormatInfo) * PredictionModelConfiguration.SaturdayWeight) +
                               (double.Parse(_features[i].CovidCases, numberFormatInfo) * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);

                    actual.Add(_features[i]?.Target);
                }

                // after now but less than 7
                for (int i = startIndex + diff_before_now; i <= startIndex + diff_sum; i += _step)
                {
                    var avg_night = (night[i - 365] + night[i - (365 * 2)] + night[i - (365 * 3)] + night[i - (365 * 4)] + night[i - (365 * 5)] + night[i - (365 * 6)] + night[i - (365 * 7)] + night[i - (365 * 8)]) / 8;
                    var avg_newYear = (newYear[i - 365] + newYear[i - (365 * 2)] + newYear[i - (365 * 3)] + newYear[i - (365 * 4)] + newYear[i - (365 * 5)] + newYear[i - (365 * 6)] + newYear[i - (365 * 7)] + newYear[i - (365 * 8)]) / 8;
                    var avg_holiday = (holiday[i - 365] + holiday[i - (365 * 2)] + holiday[i - (365 * 3)] + holiday[i - (365 * 4)] + holiday[i - (365 * 5)] + holiday[i - (365 * 6)] + holiday[i - (365 * 7)] + holiday[i - (365 * 8)]) / 8;
                    var avg_sunday = (sunday[i - 365] + sunday[i - (365 * 2)] + sunday[i - (365 * 3)] + sunday[i - (365 * 4)] + sunday[i - (365 * 5)] + sunday[i - (365 * 6)] + sunday[i - (365 * 7)] + sunday[i - (365 * 8)]) / 8;
                    var avg_saturday = (saturday[i - 365] + saturday[i - (365 * 2)] + saturday[i - (365 * 3)] + saturday[i - (365 * 4)] + saturday[i - (365 * 5)] + saturday[i - (365 * 6)] + saturday[i - (365 * 7)] + saturday[i - (365 * 8)]) / 8;
                    double covid_enabled = covidEnabled ? 1f : 0f;

                    if (first7daysCounter < diff_after_now)
                    {
                        prediction.Add(
                               (avg_night * PredictionModelConfiguration.NightDurationWeight) +
                               ((weatherPredict[first7daysCounter].temp.day - 273) * PredictionModelConfiguration.WeatherWeight) +
                               (avg_newYear * PredictionModelConfiguration.NewYearWeight) +
                               (avg_holiday * PredictionModelConfiguration.HolidayWeight) +
                               (avg_sunday * PredictionModelConfiguration.SundayWeight) +
                               (avg_saturday * PredictionModelConfiguration.SaturdayWeight) +
                               (covid_enabled * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);
                        ++first7daysCounter;
                        actual.Add(_features[i]?.Target);
                    }
                    else
                    {
                        return new JsonResult(new { x = timeFrames, y = prediction, actual_y = actual });
                    }
                }
            }

            for (int i = startIndex; i < startIndex + timedelta; i += _step)
            {
                prediction.Add((int.Parse(_features[i].NightDuration, numberFormatInfo) * PredictionModelConfiguration.NightDurationWeight) +
                               (double.Parse(_features[i].Weather, numberFormatInfo) * PredictionModelConfiguration.WeatherWeight) +
                               (double.Parse(_features[i].NewYear, numberFormatInfo) * PredictionModelConfiguration.NewYearWeight) +
                               (double.Parse(_features[i].Holliday, numberFormatInfo) * PredictionModelConfiguration.HolidayWeight) +
                               (int.Parse(_features[i].Sunday, numberFormatInfo) * PredictionModelConfiguration.SundayWeight) +
                               (int.Parse(_features[i].Saturday, numberFormatInfo) * PredictionModelConfiguration.SaturdayWeight) +
                               (double.Parse(_features[i].CovidCases, numberFormatInfo) * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);

                actual.Add(_features[i].Target);
            }

            return new JsonResult(new { x = timeFrames, y = prediction, actual_y = actual });

        }   
    }

    public class Frame
    {
        public string Date { get; set; }

        public double X { get; set; }
    }
}
