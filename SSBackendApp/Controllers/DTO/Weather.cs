namespace SSBackendApp.Controllers
{
    public class Weather
    {
        public int? id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
}

// http://localhost:5000/api/data/GetWeather?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetCovid?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetDaylight?startDate=2020-11-01&endDate=2020-12-15