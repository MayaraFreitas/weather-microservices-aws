using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using System.Text.Json;

namespace CloudWeather.Report.BusinessLogic
{
    public interface IWeatherReportAggregator
    {
        /// <summary>
        /// Builds and returns a Weather Report.
        /// Persists WeeklyWeatherReport data
        /// </summary>
        /// <param name="zipCode"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        Task<WeatherReport> BuildReport(string zipCode, int days);
    }
    public class WeatherReportAggregator : IWeatherReportAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(IHttpClientFactory http,
                                       ILogger<WeatherReportAggregator> logger,
                                       WeatherDataConfig weatherDataConfig,
                                       WeatherReportDbContext db)
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig;
            _db = db;
        }

        public async Task<WeatherReport> BuildReport(string zipCode, int days)
        {
            var httpClient = _http.CreateClient();

            var precipData = await FetchPrecipitationData(httpClient, zipCode, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"zip: {zipCode} over last {days}: " +
                $"total snow: {totalSnow}, rain {totalRain}."
            );

            var tempData = await FetchTemperatureData(httpClient, zipCode, days);
            var averageTempHigh = tempData.Average(t => t.TempHighF);
            var averageTempLow = tempData.Average(t => t.TempLowF);

            var weatherReport = new WeatherReport
            {
                AvarageHighF = averageTempHigh,
                AvarageLowF = averageTempLow,
                RainfallTotalInches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zipCode,
                CreatedOn = DateTime.UtcNow,
            };

            // TODO: Use 'cached' weather reports instead of making round trips when possible?
            await _db.AddAsync(weatherReport);
            await _db.SaveChangesAsync();

            return weatherReport;
        }

        private static decimal GetTotalSnow(IEnumerable<PrecipitationModel> precipData)
            => GetTotalByWeatherType(precipData, "snow");
        
        private static decimal GetTotalRain(IEnumerable<PrecipitationModel> precipData)
            => GetTotalByWeatherType(precipData, "rain");

        private static decimal GetTotalByWeatherType(IEnumerable<PrecipitationModel> precipData, string weatherType)
        {
            var total = precipData
                .Where(p => p.WeatherType == weatherType)
                .Sum(p => p.AmountInches);
            return Math.Round(total, 1);
        }

        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zipCode, int days)
        {
            var endpoint = BuildTemperatureServiceEndpoint(zipCode, days);
            var temperatureRecords = await httpClient.GetAsync(endpoint);

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var temperatureData = await temperatureRecords
                .Content.
                ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializerOptions);

            return temperatureData ?? new List<TemperatureModel>();
        }

        private string BuildTemperatureServiceEndpoint(string zipCode, int days)
        {
            var tempServiceProtocol = _weatherDataConfig.TempDataProtocol;
            var tempDataHost = _weatherDataConfig.TempDataHost;
            var tempDataPort = _weatherDataConfig.TempDataPort;
            return $"{tempServiceProtocol}://{tempDataHost}:{tempDataPort}/observation/{zipCode}?days={days}";
        }

        private async Task<List<PrecipitationModel>> FetchPrecipitationData(HttpClient httpClient, string zipCode, int days)
        {
            var endpoint = BuildPrecipitationServiceEndpoint(zipCode, days);
            var precipRecords = await httpClient.GetAsync(endpoint);
            
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var precipData = await precipRecords
                .Content
                .ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializerOptions);

            return precipData ?? new List<PrecipitationModel>();
        }

        private string BuildPrecipitationServiceEndpoint(string zipCode, int days)
        {
            var precipDataProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipDataHost = _weatherDataConfig.PrecipDataHost;
            var precipDataPort = _weatherDataConfig.PrecipDataPort;
            return $"{precipDataProtocol}://{precipDataHost}:{precipDataPort}/observation/{zipCode}?days={days}";
        }
    }
}
