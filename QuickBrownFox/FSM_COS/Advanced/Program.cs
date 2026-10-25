using System.Text.Json;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

if (args.Length > 1)
    throw new ArgumentException("Usage: Advanced.FSM_COS [output-trace-path]");

var outputPath = Path.GetFullPath(args.Length == 1 ? args[0] : "advanced-fox-dog-fsm-cos-trace.json");
var originalOutput = Console.Out;
var frames = new List<object>();

try
{
    Console.SetOut(TextWriter.Null);
    var bundle = new AdvancedFoxDogMicroBundle();
    var catalog = new AdvancedFoxDogCatalog(bundle);
    var manifest = new RuntimeManifest(
        RuntimeId: 50_102,
        Bundles: [new MicroBundleManifestEntry(bundle.Id, "1.0.0")]);

    var assembly = new FsmCos(catalog).Execute(manifest);
    if (!assembly.TryGetBundle<AdvancedFoxDogMicroBundle>(bundle.Id, out var loadedBundle) ||
        loadedBundle?.Context is null)
    {
        throw new InvalidOperationException("FSM_COS composed the manifest but did not expose the advanced fox-and-dog MicroBundle.");
    }

    var context = loadedBundle.Context;
    for (var i = 0; i < 30 && context.IsValid; i++)
    {
        FSM_API.Interaction.Update("Update");
        frames.Add(context.CaptureSnapshot());
    }
}
finally
{
    Console.SetOut(originalOutput);
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var trace = new
{
    schemaVersion = 1,
    source = "SimpleDemos Advanced 2D fox-and-dog simulation running through FSM_COS and FSM_API",
    frames
};
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(trace, new JsonSerializerOptions
{
    WriteIndented = true
}));

Console.WriteLine($"FSM_COS composed the advanced 2D fox-and-dog demo and exported {frames.Count} frames to {outputPath}");

sealed class AdvancedFoxDogMicroBundle : IMicroBundle
{
    public AdvancedFoxDogMicroBundle()
    {
        Descriptor = new MicroBundleDescriptor(
            9_003,
            "1.0.0",
            dependencies: Array.Empty<MicroBundleDependency>());
        Dependencies = Array.Empty<MicroBundleDependencyRequest>();
    }

    public MicroBundleDescriptor Descriptor { get; }
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; }
    public AdvancedDemoContext? Context { get; private set; }

    public void Load(IMicroBundleLoadContext context) =>
        Context = new AdvancedDemoContext(randomSeed: 42, allowConsoleInput: false);

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}

sealed class AdvancedFoxDogCatalog : IMicroBundleCatalog
{
    private readonly IReadOnlyDictionary<ulong, IMicroBundle> _bundles;

    public AdvancedFoxDogCatalog(params IMicroBundle[] bundles) =>
        _bundles = bundles.ToDictionary(bundle => bundle.Id);

    public bool TryResolve(ulong bundleId, string version, out IMicroBundle? bundle)
    {
        if (_bundles.TryGetValue(bundleId, out var found) &&
            string.Equals(found.Descriptor.Version, version, StringComparison.Ordinal))
        {
            bundle = found;
            return true;
        }

        bundle = null;
        return false;
    }

    public bool TryResolve(ulong bundleId, out IMicroBundle? bundle) =>
        _bundles.TryGetValue(bundleId, out bundle);
}
