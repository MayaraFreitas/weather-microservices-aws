#Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /source
COPY . .
RUN dotnet restore "./CloudWeather.DataLoader.csproj" --disable-parallel
RUN dotnet publish "./CloudWeather.DataLoader.csproj" -c release -o /app --no-restore

# Service Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /App/out .

ENTRYPOINT ["dotnet", "CloudWeather.DataLoader.dll"]