using SimpleDemos.RestCityDemo;

var fixture = args.Contains("--fixture", StringComparer.Ordinal);
var outputOption = Array.IndexOf(args, "--output");
var output = outputOption >= 0 && outputOption + 1 < args.Length ? args[outputOption + 1] : Path.Combine("artifacts", "tacoma-weather-visualize.svg");
try
{
    using var client = new OpenMeteoClient();
    var weather = fixture ? WeatherSnapshot.Fixture : await client.GetTacomaAsync();
    await CityWeatherVisualizer.WriteSvgAsync(weather, output);
    Console.WriteLine($"Visualize-only REST path: {weather.City}");
    Console.WriteLine($"Source: {weather.Source}");
    Console.WriteLine($"SVG artifact: {Path.GetFullPath(output)}");
    Console.WriteLine("This executable references the shared REST contract and Visualize; it does not reference Renderer or GUI.");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Visualize-only demonstration failed: {exception.GetType().Name}: {exception.Message}");
    return 1;
}
return 0;
