using System.IO;
using System.Windows;
using SimpleDemos.RestCityDemo;
using TheSingularityWorkshop.GUI.WPF;
using DemoProgram = SimpleDemos.RestCityDemo.Program;
namespace SimpleDemos.RestCityDemo.Wpf;
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            var mode=DemoProgram.ReadOption(args,"--mode")??"renderer";
            if(mode is not ("renderer" or "combined"))throw new ArgumentException("WPF host mode must be renderer or combined.");
            var fixture=args.Contains("--fixture",StringComparer.Ordinal);
            var output=RestCityDemo.Program.ReadOption(args,"--output")??Path.Combine("artifacts","tacoma-weather.svg");
            using var client=new OpenMeteoClient();
            var weather=fixture?WeatherSnapshot.Fixture:client.GetTacomaAsync().GetAwaiter().GetResult();
            var(tree,representation)=WeatherRendererPresentation.Build(weather);
            if(mode=="combined")CityWeatherVisualizer.WriteSvgAsync(weather,output).GetAwaiter().GetResult();
            var app=new Application();
            var window=new WpfWindowBuilder().WithTitle($"Tacoma Weather — Renderer {representation}").WithSize(760,560).WithContent(WpfGuiRenderer.Render(tree)).Build();
            return app.Run(window);
        }
        catch(Exception exception){MessageBox.Show(exception.Message,"REST City Demo",MessageBoxButton.OK,MessageBoxImage.Error);return 1;}
    }
}
