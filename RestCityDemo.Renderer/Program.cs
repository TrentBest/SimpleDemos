using SimpleDemos.RestCityDemo;

var fixture = args.Contains("--fixture", StringComparer.Ordinal);
try
{
    using var client = new OpenMeteoClient();
    var weather = fixture ? WeatherSnapshot.Fixture : await client.GetTacomaAsync();
    var (tree, representation) = WeatherRendererPresentation.Build(weather);
    Console.WriteLine($"Renderer-only REST path: {weather.City} ({representation})");
    Console.WriteLine($"Source: {weather.Source}");
    WeatherRendererPresentation.WriteTextTree(tree, Console.Out);
    Console.WriteLine("This executable references the shared REST contract, Renderer, and GUI.Core; it does not reference Visualize.");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Renderer-only demonstration failed: {exception.GetType().Name}: {exception.Message}");
    return 1;
}
return 0;
