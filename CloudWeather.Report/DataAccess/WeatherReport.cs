namespace CloudWeather.Report.DataAccess
{
    public class WeatherReport
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal AvarageHighF { get; set; }
        public decimal AvarageLowF { get; set; }
        public decimal RainfallTotalInches { get; set; }
        public decimal SnowTotalInches { get; set; }
        public string ZipCode { get; set; }
    }
}
