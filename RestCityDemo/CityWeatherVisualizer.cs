using System.Numerics;
using TheSingularityWorkshop.Visualize;
namespace SimpleDemos.RestCityDemo;
public static class CityWeatherVisualizer
{
    public static async Task WriteSvgAsync(WeatherSnapshot weather,string outputPath,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(weather); ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var resources=new VisualizerResourceCatalog(); var commands=new List<VisualizerDrawCommand>();
        AddBar(resources,commands,1,-0.72f,Normalize(weather.TemperatureC,40),new Vector4(0.18f,0.48f,0.86f,1));
        AddBar(resources,commands,2,-0.05f,weather.RelativeHumidityPercent/100f,new Vector4(0.12f,0.62f,0.46f,1));
        AddBar(resources,commands,3,0.62f,Normalize(weather.WindSpeedKmh,60),new Vector4(0.88f,0.48f,0.16f,1));
        var frame=new VisualizerFrame(new VisualizerView(Vector3.Zero,Matrix4x4.Identity,Matrix4x4.Identity),commands);
        var fullPath=Path.GetFullPath(outputPath); Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var backend=new SvgVisualizerBackend(svg=>File.WriteAllText(fullPath,svg),900,520,"#101827");
        await using var visualizer=new Visualizer(backend,resources);
        await visualizer.RenderAsync(frame,cancellationToken);
    }
    private static float Normalize(double value,double maximum)=>(float)Math.Clamp(value/maximum,0.08,1.0);
    private static void AddBar(VisualizerResourceCatalog resources,ICollection<VisualizerDrawCommand> commands,ulong id,float centerX,float height,Vector4 color)
    {
        const float halfWidth=0.19f,bottom=-0.78f; var top=bottom+1.48f*height;
        var vertices=new[]{Vertex(centerX-halfWidth,bottom),Vertex(centerX+halfWidth,bottom),Vertex(centerX+halfWidth,top),Vertex(centerX-halfWidth,top)};
        var geometryId=new VisualizerResourceId(id); var materialId=new VisualizerResourceId(id+100);
        resources.SetGeometry(geometryId,new VisualizerGeometry(1,vertices,new uint[]{0,1,2,0,2,3}));
        resources.SetMaterial(materialId,new VisualizerMaterial(1,color));
        commands.Add(new VisualizerDrawCommand(geometryId,materialId,Matrix4x4.Identity,6,0,0,VisualizerRepresentation.Aggregate));
    }
    private static VisualizerVertex Vertex(float x,float y)=>new(new Vector3(x,y,0),Vector3.UnitZ,Vector2.Zero,Vector4.One);
}
