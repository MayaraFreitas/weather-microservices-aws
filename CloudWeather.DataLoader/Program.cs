using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var servicesConfig = config.GetSection("Services");

var tempConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempConfig["Host"];
var tempServicePort = tempConfig["Port"];

var precipConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipConfig["Host"];
var precipServicePort = precipConfig["Port"];

var zipCodes = new List<string>()
{
    "73026",
    "68104",
    "04401",
    "32808",
    "19717",
};

Console.WriteLine("Starting Data locad");

var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipitationHttpClient = new HttpClient();
precipitationHttpClient.BaseAddress = new Uri($"http://{precipServiceHost}:{precipServicePort}");

foreach(var zipCode in zipCodes)
{
    Console.WriteLine($"Processing zip code {zipCode}");
    var from = DateTime.Now.AddYears(-2);
    var thru = DateTime.Now;

    for(var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
    {
        var temps = PostTemp(zipCode, day, temperatureHttpClient);
        PostPrecip(temps[0], zipCode, day, precipitationHttpClient);
    }
}

void PostPrecip(int lowTemp, string zipCode, DateTime day, HttpClient httpclient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1;

    PrecipitationModel precipitation;
    if (isPrecip)
    {
        var precipInches = rand.Next(1, 16);
        var wheatherType = lowTemp < 32 ? "snow" : "rain";

        precipitation = new PrecipitationModel
        {
            AmountInches = precipInches,
            WeatherType = wheatherType,
            ZipCode = zipCode,
            CreatedOn = day
        };
    }
    else
    {
        precipitation = new PrecipitationModel
        {
            AmountInches = 0,
            WeatherType = "none",
            ZipCode = zipCode,
            CreatedOn = day
        };
    }

    var precipResponse = httpclient
        .PostAsJsonAsync("observation", precipitation)
        .Result;

    if(precipResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Precipitation Date: {day:d}" +
            $"Zip: {zipCode}" +
            $"WeatherType: {precipitation.WeatherType}" +
            $"Amound (in.): {precipitation.AmountInches}");
    }
}

List<int> PostTemp(string zipCode, DateTime day, HttpClient httpclient)
{
    var rand = new Random();
    var temp1 = rand.Next(0, 100);
    var temp2 = rand.Next(0, 100);
    
    var hiLoTemps = new List<int>() { temp1, temp2};
    hiLoTemps.Sort();

    var temperatureObservation = new TemperatureModel
    {
        TempLowF = hiLoTemps[0],
        TempHighF = hiLoTemps[1],
        ZipCode = zipCode,
        CreatedOn = day
    };

    var precipResponse = httpclient
        .PostAsJsonAsync("observation", temperatureObservation)
        .Result;

    if (precipResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Posted Precipitation Date: {day:d}" +
            $"Zip: {zipCode}" +
            $"Lo (F): {hiLoTemps[0]}" +
            $"Hi (F): {hiLoTemps[1]}");
    }

    return hiLoTemps;
}