namespace SimpleDemos.RestCityDemo;
public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var mode=ReadOption(args,"--mode")??"combined"; var fixture=args.Contains("--fixture",StringComparer.Ordinal);
        var output=ReadOption(args,"--output")??Path.Combine("artifacts","tacoma-weather.svg");
        if(mode is not ("visualize" or "renderer" or "combined")){Console.Error.WriteLine("Mode must be visualize, renderer, or combined.");return 2;}
        try
        {
            using var client=new OpenMeteoClient();
            var weather=fixture?WeatherSnapshot.Fixture:await client.GetTacomaAsync();
            Console.WriteLine($"REST observation: {weather.City}\nCoordinates: {weather.Latitude:0.0000}, {weather.Longitude:0.0000}\nSource: {weather.Source}");
            if(mode is "renderer" or "combined")
            {
                var(tree,representation)=WeatherRendererPresentation.Build(weather);
                Console.WriteLine($"\nRenderer + GUI.Core textual tree ({representation}):");
                WeatherRendererPresentation.WriteTextTree(tree,Console.Out);
            }
            if(mode is "visualize" or "combined")
            {
                await CityWeatherVisualizer.WriteSvgAsync(weather,output);
                Console.WriteLine($"\nVisualize SVG artifact: {Path.GetFullPath(output)}");
                Console.WriteLine("CPU-side static SVG artifact; not a production GPU renderer.");
            }
            return 0;
        }
        catch(Exception exception){Console.Error.WriteLine($"REST demonstration failed: {exception.GetType().Name}: {exception.Message}");return 1;}
    }
    public static string? ReadOption(string[] args,string name){var i=Array.IndexOf(args,name);return i>=0&&i+1<args.Length?args[i+1]:null;}
}
