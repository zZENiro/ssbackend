using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using SSBackendApp.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

        private DateTime _currentDate;
        NumberFormatInfo numberFormatInfo;

        public DataController(
            IConnectionMultiplexer multiplexer,
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
        public async Task<IActionResult> GetPrediction([FromQuery] string startDate, string endDate, string terr, string step)
        {
            var _startDate = DateTime.Parse(startDate);
            var _endDate = DateTime.Parse(endDate);
            var _step = step == "mounth" ? 30 : 1;

            var timedelta = _endDate - _startDate;

            CacheFlags.MaxQueriedDaysCount = timedelta.Days > CacheFlags.MaxQueriedDaysCount ? timedelta.Days : CacheFlags.MaxQueriedDaysCount;

            var prediction = new List<double>();
            var timeFrames = new List<string>();

            for (int i = 0; i < timedelta.Days; ++i)
            {
                _currentDate = _startDate.Add(TimeSpan.FromDays(i));

                prediction.Add((int.Parse(_features[i].NightDuration, numberFormatInfo) * PredictionModelConfiguration.NightDurationWeight) +
                               (double.Parse(_features[i].Weather, numberFormatInfo) * PredictionModelConfiguration.WeatherWeight) +
                               (double.Parse(_features[i].NewYear, numberFormatInfo) * PredictionModelConfiguration.NewYearWeight) +
                               (double.Parse(_features[i].Holliday, numberFormatInfo) * PredictionModelConfiguration.HolidayWeight) +
                               (int.Parse(_features[i].Sunday, numberFormatInfo) * PredictionModelConfiguration.SundayWeight) +
                               (int.Parse(_features[i].Saturday, numberFormatInfo) * PredictionModelConfiguration.SaturdayWeight) +
                               (double.Parse(_features[i].CovidCases, numberFormatInfo) * PredictionModelConfiguration.CovidCasesWeight) +
                               PredictionModelConfiguration.Bies);

                timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");
            }

            return new JsonResult(new { x = timeFrames, y = prediction, actual_y = _features.Select(el => el.Target) });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetEnergyByWeather([FromQuery] string startDate, string endDate, string terr, string step)
        {
            var _startDate = DateTime.Parse(startDate);
            var _endDate = DateTime.Parse(endDate);
            var _step = step == "mounth" ? 30 : 1;

            var timedelta = _endDate - _startDate;

            var energyUsage = new Frame[timedelta.Days];
            var weatherParams = new Frame[timedelta.Days];

           

            return new JsonResult(new { energy = energyUsage, weather = weatherParams });
        }
    }

    public class Frame
    {
        public string Date { get; set; }

        public double X { get; set; } 
    }
}
