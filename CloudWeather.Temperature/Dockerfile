#Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /source
COPY . ./
# Restore as distinct layers
RUN dotnet restore "./CloudWeather.Temperature.csproj" --disable-parallel
# Build and publish a release
RUN dotnet publish "./CloudWeather.Temperature.csproj" -c release -o /app --no-restore

# Service Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app .

EXPOSE 5000

ENTRYPOINT ["dotnet", "CloudWeather.Temperature.dll"]