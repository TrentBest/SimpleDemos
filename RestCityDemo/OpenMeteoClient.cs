using System.Globalization;
using System.Text.Json;
namespace SimpleDemos.RestCityDemo;
public sealed class OpenMeteoClient(HttpClient? httpClient = null) : IDisposable
{
    private readonly HttpClient _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    private readonly bool _ownsClient = httpClient is null;
    public async Task<WeatherSnapshot> GetTacomaAsync(CancellationToken cancellationToken = default)
    {
        const double latitude=47.2529, longitude=-122.4443;
        var url="https://api.open-meteo.com/v1/forecast?latitude="+latitude.ToString(CultureInfo.InvariantCulture)+"&longitude="+longitude.ToString(CultureInfo.InvariantCulture)+"&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&timezone=America%2FLos_Angeles";
        using var request=new HttpRequestMessage(HttpMethod.Get,url);
        using var response=await _http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream=await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document=await JsonDocument.ParseAsync(stream,cancellationToken:cancellationToken);
        var root=document.RootElement; var current=root.GetProperty("current");
        var time=current.GetProperty("time").GetString() ?? throw new InvalidDataException("Open-Meteo response omitted current.time.");
        var timezone=root.GetProperty("timezone").GetString() ?? "unknown";
        var offset=TimeSpan.FromSeconds(root.GetProperty("utc_offset_seconds").GetInt32());
        var observed=DateTimeOffset.Parse(time,CultureInfo.InvariantCulture,DateTimeStyles.None);
        observed=new DateTimeOffset(observed.DateTime,offset);
        return new WeatherSnapshot("Tacoma, Washington",root.GetProperty("latitude").GetDouble(),root.GetProperty("longitude").GetDouble(),current.GetProperty("temperature_2m").GetDouble(),current.GetProperty("apparent_temperature").GetDouble(),current.GetProperty("relative_humidity_2m").GetInt32(),current.GetProperty("wind_speed_10m").GetDouble(),current.GetProperty("weather_code").GetInt32(),observed,timezone,response.RequestMessage?.RequestUri?.ToString()??url);
    }
    public void Dispose(){if(_ownsClient)_http.Dispose();}
}
