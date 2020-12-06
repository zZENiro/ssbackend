namespace SSBackendApp.Controllers
{
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
}

// http://localhost:5000/api/data/GetWeather?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetCovid?startDate=2020-11-01&endDate=2020-12-15
// http://localhost:5000/api/data/GetDaylight?startDate=2020-11-01&endDate=2020-12-15