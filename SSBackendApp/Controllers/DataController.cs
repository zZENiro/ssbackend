using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using SSBackendApp.Cache;
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

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetPrediction([FromQuery] string startDate, string endDate, string terr, string step)
        {
            var _startDate = DateTime.Parse(startDate);
            var _endDate = DateTime.Parse(endDate);
            var _step = step == "mounth" ? 30 : 1;

            //if (_endDate > DateTime.Now && (_endDate - DateTime.Now) > 10)

            var timedelta = (_endDate - _startDate).Days;

            var startIndex = (_startDate - _startTime).Days - 1; 

            var prediction = new List<double>();
            var timeFrames = new List<string>();
            var actual = new List<string>();

            for (int i = 0; i < timedelta; i += _step)
            {
                _currentDate = _startDate.Add(TimeSpan.FromDays(i));
                timeFrames.Add($"{_currentDate.Year}-{_currentDate.Month.ToString("00")}-{_currentDate.Day.ToString("00")}");

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
