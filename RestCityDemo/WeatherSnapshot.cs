namespace SimpleDemos.RestCityDemo;
public sealed record WeatherSnapshot(string City,double Latitude,double Longitude,double TemperatureC,double ApparentTemperatureC,int RelativeHumidityPercent,double WindSpeedKmh,int WeatherCode,DateTimeOffset ObservedAt,string TimeZone,string Source)
{
    public static WeatherSnapshot Fixture { get; } = new("Tacoma, Washington",47.2529,-122.4443,14.2,13.1,68,11.5,3,new DateTimeOffset(2026,10,30,9,0,0,TimeSpan.FromHours(-7)),"America/Los_Angeles","Deterministic CI fixture — not live weather data");
}
