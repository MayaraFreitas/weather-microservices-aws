using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

Console.WriteLine("Starting Configuration");

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .AddEnvironmentVariables()
    .Build();

var servicesConfig = config.GetSection("Services");

var tempConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempConfig["Host"];
var tempServicePort = tempConfig["Port"];

Console.WriteLine($"Temperature Host: {tempServiceHost}");
Console.WriteLine($"Temperature Port: {tempServicePort}");

var precipConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipConfig["Host"];
var precipServicePort = precipConfig["Port"];

Console.WriteLine($"Precipitation Host: {precipServiceHost}");
Console.WriteLine($"Precipitation Port: {precipServicePort}");

var zipCodes = new List<string>()
{
    "73026",
    "68104",
    "04401",
    "32808",
    "19717",
};

Console.WriteLine("Configuration Completed");

Console.WriteLine("Starting Data load");

var temperatureHttpClient = new HttpClient();
var temperatureUri = $"http://{tempServiceHost}:{tempServicePort}";
Console.WriteLine($"Temperature Uri: {temperatureUri}");
temperatureHttpClient.BaseAddress = new Uri(temperatureUri);

var precipitationHttpClient = new HttpClient();
var precipitationUri = $"http://{precipServiceHost}:{precipServicePort}";
Console.WriteLine($"Precipitation Uri: {precipitationUri}");
precipitationHttpClient.BaseAddress = new Uri(precipitationUri);

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
    else
    {
        Console.WriteLine($"Fail to get precipitation. Zip code {zipCode}. Status Code: {precipResponse.StatusCode} - {precipResponse.Content}");
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
        Console.WriteLine($"Posted Temperature Date: {day:d}" +
            $"Zip: {zipCode}" +
            $"Lo (F): {hiLoTemps[0]}" +
            $"Hi (F): {hiLoTemps[1]}");
    }
    else
    {
        Console.WriteLine($"Fail to get Temperature. Zip code {zipCode}. Status Code: {precipResponse.StatusCode} - {precipResponse.Content}");
    }

    return hiLoTemps;
}