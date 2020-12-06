namespace SSBackendApp.Controllers
{
    public class City
    {
        public int id { get; set; }
        public string name { get; set; }
        public Coord coord { get; set; }
        public string country { get; set; }
        public int population { get; set; }
        public int timezone { get; set; }
    }
}

// http://localhost:5000/api/data/GetWeather?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetCovid?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetDaylight?startDate=2020-11-01&endDate=2020-12-15