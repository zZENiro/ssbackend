namespace SSBackendApp.Controllers
{
    public class Rootobject
    {
        public City city { get; set; }
        public string cod { get; set; }
        public float? message { get; set; }
        public int? cnt { get; set; }
        public List[] list { get; set; }
    }
}

// http://localhost:5000/api/data/GetWeather?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetCovid?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetDaylight?startDate=2020-11-01&endDate=2020-12-15