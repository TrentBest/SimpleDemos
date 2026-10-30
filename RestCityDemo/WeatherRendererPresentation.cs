using TheSingularityWorkshop.Renderer;
using TheSingularityWorkshop.Workshop.Gui;
namespace SimpleDemos.RestCityDemo;
public static class WeatherRendererPresentation
{
    public static (GuiNode Tree,RepresentationTarget Representation) Build(WeatherSnapshot weather)
    {
        ArgumentNullException.ThrowIfNull(weather);
        var context=new RendererComputationContext("TacomaWeather",RepresentationTarget.Aggregate);
        using var machine=new RendererComputationMachine(context);
        var representation=machine.Advance();
        var root=GuiBuilders.Panel("tacoma-weather")
            .Property("title",weather.City)
            .Child(GuiBuilders.Text("weather-title",weather.City))
            .Child(GuiBuilders.Text("representation",$"Renderer representation: {representation}"))
            .Child(GuiBuilders.Text("temperature",$"Temperature: {weather.TemperatureC:0.0} °C (feels like {weather.ApparentTemperatureC:0.0} °C)"))
            .Child(GuiBuilders.Text("humidity",$"Relative humidity: {weather.RelativeHumidityPercent}%"))
            .Child(GuiBuilders.Text("wind",$"Wind: {weather.WindSpeedKmh:0.0} km/h"))
            .Child(GuiBuilders.Text("weather-code",$"WMO weather code: {weather.WeatherCode}"))
            .Child(GuiBuilders.Text("coordinates",$"Coordinates: {weather.Latitude:0.0000}, {weather.Longitude:0.0000}"))
            .Child(GuiBuilders.Text("observed-at",$"Observed: {weather.ObservedAt:O} ({weather.TimeZone})"))
            .Child(GuiBuilders.Text("source",$"Source: {weather.Source}"))
            .Build();
        return (root,representation);
    }
    public static void WriteTextTree(GuiNode node,TextWriter writer,int depth=0)
    {
        ArgumentNullException.ThrowIfNull(node); ArgumentNullException.ThrowIfNull(writer);
        writer.WriteLine(node.Text is null ? $"{new string(' ',depth*2)}{node.Kind} [{node.Id}]" : $"{new string(' ',depth*2)}{node.Text}");
        foreach(var child in node.Children) WriteTextTree(child,writer,depth+1);
    }
}
