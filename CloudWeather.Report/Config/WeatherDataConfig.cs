namespace CloudWeather.Report.Config
{
    public class WeatherDataConfig
    {
        // Precipitation
        public string PrecipDataProtocol { get; set; }
        public string PrecipDataHost { get; set; }
        public string PrecipDataPort { get; set; }
        
        // Temperature
        public string TempDataProtocol { get; set; }
        public string TempDataHost { get; set; }
        public string TempDataPort { get; set; }
    }
}
