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

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetWeather([FromQuery] string startDate, string endDate, string step) =>
            await Task.Factory.StartNew<IActionResult>(() =>
            {
                if (startDate is null || endDate is null)
                    return BadRequest(new { message = "Set startDate, endDate" });

                var _startDate = DateTime.Parse(startDate);
                var _endDate = DateTime.Parse(endDate);
                var _step = step == "mounth" ? 30 : 1;

                var timedelta = (_endDate - _startDate).Days;

                var startIndex = (_startDate - _startTime).Days - 1;

                var weatherGlobal = _features.Select(feature => double.Parse(feature.Weather, numberFormatInfo)).ToList<double>();
                var weatherCollection = new List<double>();
                var timeFrames = new List<string>();


                var diff_after_now = (_endDate - DateTime.Now).Days; // now-->
                var diff_before_now = (DateTime.Now - _startDate).Days; // <--now
                var diff_sum = diff_after_now + diff_before_now;

                for (int i = 0; i < diff_sum; i += _step)
                {
                    _currentDate = _startDate.Add(TimeSpan.FromDays(i));
                    timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");
                }

                // if predict more than 7 days
                if (_endDate > DateTime.Now && (_endDate - DateTime.Now).Days > 7)
                {
                    int first7daysCounter = 0;
                    var weatherPredict = GetWeather().list;

                    for (int i = startIndex; i < startIndex + diff_before_now; i += _step)
                    {
                        weatherCollection.Add(weatherGlobal[i]);
                    }
                    for (int i = startIndex + diff_before_now; i < startIndex + diff_sum; i += _step)
                    {
                        if (first7daysCounter < 7)
                        {
                            weatherCollection.Add(weatherPredict[first7daysCounter].temp.day.Value - 273);

                            ++first7daysCounter;
                            continue;
                        }

                        weatherCollection.Add(weatherGlobal[i]);
                    }
                }
                // if less than 7 days
                else if (_endDate > DateTime.Now && (_endDate - DateTime.Now).Days <= 7)
                {
                    var weatherPredict = GetWeather().list;

                    var first7daysCounter = 0;

                    // before now
                    for (int i = startIndex; i < startIndex + diff_before_now; i += _step)
                    {
                        weatherCollection.Add(weatherGlobal[i]);
                    }

                    // after now but less than 7
                    for (int i = startIndex + diff_before_now; i <= startIndex + diff_sum; i += _step)
                    {
                        if (first7daysCounter < diff_after_now)
                        {
                            weatherCollection.Add(weatherPredict[first7daysCounter].temp.day.Value - 273);
                            ++first7daysCounter;
                        }
                        else
                        {
                            return new JsonResult(new { x = timeFrames, y = weatherCollection });
                        }
                    }
                }

                for (int i = startIndex; i < startIndex + timedelta; i += _step)
                {
                    weatherCollection.Add(weatherGlobal[i]);
                }

                return new JsonResult(new { x = timeFrames, y = weatherCollection });
            });

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetCovid([FromQuery] string startDate, string endDate, string terr, string step) =>
            await Task.Factory.StartNew<IActionResult>(() =>
            {
                if (startDate is null || endDate is null)
                    return BadRequest(new { message = "Set startDate, endDate" });

                var _startDate = DateTime.Parse(startDate);
                var _endDate = DateTime.Parse(endDate);
                var _step = step == "mounth" ? 30 : 1;

                var timedelta = (_endDate - _startDate).Days;

                var startIndex = (_startDate - _startTime).Days - 1;

                var covidGlobal = _features.Select(feature => double.Parse(feature.CovidCases, numberFormatInfo)).ToList<double>();
                var covidCollection = new List<double>();
                var timeFrames = new List<string>();


                var diff_after_now = (_endDate - DateTime.Now).Days; // now-->
                var diff_before_now = (DateTime.Now - _startDate).Days; // <--now
                var diff_sum = diff_after_now + diff_before_now;

                for (int i = 0; i < diff_sum; i += _step)
                {
                    _currentDate = _startDate.Add(TimeSpan.FromDays(i));
                    timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");
                }

                for (int i = startIndex; i < startIndex + diff_sum; i += _step)
                    covidCollection.Add(covidGlobal[i]);

                return new JsonResult(new { x = timeFrames, y = covidCollection });
            });

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetDaylight([FromQuery] string startDate, string endDate, string terr, string step) =>
            await Task.Factory.StartNew<IActionResult>(() =>
            {
                if (startDate is null || endDate is null)
                    return BadRequest(new { message = "Set startDate, endDate" });

                var _startDate = DateTime.Parse(startDate);
                var _endDate = DateTime.Parse(endDate);
                var _step = step == "mounth" ? 30 : 1;

                var timedelta = (_endDate - _startDate).Days;

                var startIndex = (_startDate - _startTime).Days - 1;

                var nightDurationGlobal = _features.Select(feature => double.Parse(feature.NightDuration, numberFormatInfo)).ToList<double>();
                var nightDurationCollection = new List<double>();
                var timeFrames = new List<string>();

                var diff_after_now = (_endDate - DateTime.Now).Days; // now-->
                var diff_before_now = (DateTime.Now - _startDate).Days; // <--now
                var diff_sum = diff_after_now + diff_before_now;

                for (int i = 0; i < diff_sum; i += _step)
                {
                    _currentDate = _startDate.Add(TimeSpan.FromDays(i));
                    timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");
                }

                for (int i = startIndex; i < startIndex + diff_sum; i += _step)
                    nightDurationCollection.Add(nightDurationGlobal[i]);

                return new JsonResult(new { x = timeFrames, y = nightDurationCollection });
            });

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetPrediction([FromQuery] string startDate, string endDate, string terr, string step, bool covidEnabled = true)
        {
            return await Task.Factory.StartNew<IActionResult>(() =>
            {
                if (startDate is null || endDate is null)
                    return BadRequest(new { message = "Set startDate, endDate" });

                var _startDate = DateTime.Parse(startDate);
                var _endDate = DateTime.Parse(endDate);
                var _step = step == "mounth" ? 30 : 1;

                var timedelta = (_endDate - _startDate).Days;

                var startIndex = (_startDate - _startTime).Days;

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
                    var weatherPredict = GetWeather().list;

                    var first7daysCounter = 0;

                    var weather = _features.Select(f => double.Parse(f.Weather, numberFormatInfo)).ToList<double>();
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
                        if (first7daysCounter < 7)
                        {
                            prediction.Add(
                                   (night[i] * PredictionModelConfiguration.NightDurationWeight) +
                                   ((weatherPredict[first7daysCounter].temp.day.Value - 273) * PredictionModelConfiguration.WeatherWeight) +
                                   (newYear[i] * PredictionModelConfiguration.NewYearWeight) +
                                   (holiday[i] * PredictionModelConfiguration.HolidayWeight) +
                                   (sunday[i] * PredictionModelConfiguration.SundayWeight) +
                                   (saturday[i] * PredictionModelConfiguration.SaturdayWeight) +
                                   (covidEnabled ? 1f : 0f * PredictionModelConfiguration.CovidCasesWeight) +
                                   PredictionModelConfiguration.Bies);

                            ++first7daysCounter;
                            actual.Add(_features[i]?.Target);
                            continue;
                        }

                        prediction.Add(
                                    (night[i] * PredictionModelConfiguration.NightDurationWeight) +
                                    (weather[i] * PredictionModelConfiguration.WeatherWeight) +
                                    (newYear[i] * PredictionModelConfiguration.NewYearWeight) +
                                    (holiday[i] * PredictionModelConfiguration.HolidayWeight) +
                                    (sunday[i] * PredictionModelConfiguration.SundayWeight) +
                                    (saturday[i] * PredictionModelConfiguration.SaturdayWeight) +
                                    (covidEnabled ? 1f : 0f * PredictionModelConfiguration.CovidCasesWeight) +
                                    PredictionModelConfiguration.Bies);

                        actual.Add(_features[i]?.Target);
                    }

                    return new JsonResult(new { x = timeFrames, y = prediction, actual_y = actual });
                }

                // if less than 7 days
                else if (_endDate > DateTime.Now && (_endDate - DateTime.Now).Days <= 7)
                {
                    var weatherPredict = GetWeather().list;

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
                        if (first7daysCounter < diff_after_now)
                        {
                            prediction.Add(
                                   (night[i] * PredictionModelConfiguration.NightDurationWeight) +
                                   ((weatherPredict[first7daysCounter].temp.day.Value - 273) * PredictionModelConfiguration.WeatherWeight) +
                                   (newYear[i] * PredictionModelConfiguration.NewYearWeight) +
                                   (holiday[i] * PredictionModelConfiguration.HolidayWeight) +
                                   (sunday[i] * PredictionModelConfiguration.SundayWeight) +
                                   (saturday[i] * PredictionModelConfiguration.SaturdayWeight) +
                                   (covidEnabled ? 1f : 0f * PredictionModelConfiguration.CovidCasesWeight) +
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

            });


        }

        Rootobject GetWeather()
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
            using (var response = client.SendAsync(request).Result)
            {
                response.EnsureSuccessStatusCode();
                var body = response.Content.ReadAsStringAsync().Result;

                return JsonSerializer.Deserialize<Rootobject>(body);
            }
        }
    }

    public class Frame
    {
        public string Date { get; set; }

        public double X { get; set; }
    }
}

// http://localhost:5000/api/data/GetWeather?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetCovid?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetDaylight?startDate=2020-11-01&endDate=2020-12-15