using System.Text.Json;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

if (args.Length > 1)
    throw new ArgumentException("Usage: QuickBrownFox.FSM_COS [output-trace-path]");

var outputPath = Path.GetFullPath(args.Length == 1 ? args[0] : "fox-dog-fsm-cos-trace.json");
SimpleDemoContext.EnableLogging = false;

var bundle = new FoxDogMicroBundle();
var catalog = new FoxDogCatalog(bundle);
var manifest = new RuntimeManifest(
    RuntimeId: 50_101,
    Bundles: [new MicroBundleManifestEntry(bundle.Id, "1.0.0")]);

var assembly = new FsmCos(catalog).Execute(manifest);
if (!assembly.TryGetBundle<FoxDogMicroBundle>(bundle.Id, out var loadedBundle) ||
    loadedBundle?.Context is null)
{
    throw new InvalidOperationException("FSM_COS composed the manifest but did not expose the fox-and-dog MicroBundle.");
}

var context = loadedBundle.Context;
var frames = new List<object>();
for (var i = 0; i < 30 && context.IsValid; i++)
{
    FSM_API.Interaction.Update("Main");
    frames.Add(context.CaptureSnapshot());
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var trace = new
{
    schemaVersion = 1,
    source = "SimpleDemos QuickBrownFox running through FSM_COS and FSM_API",
    frames
};
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(trace, new JsonSerializerOptions
{
    WriteIndented = true
}));

Console.WriteLine($"FSM_COS composed the fox-and-dog demo and exported {frames.Count} frames to {outputPath}");

sealed class FoxDogMicroBundle : IMicroBundle
{
    public FoxDogMicroBundle()
    {
        Descriptor = new MicroBundleDescriptor(
            9_002,
            "1.0.0",
            dependencies: Array.Empty<MicroBundleDependency>());
        Dependencies = Array.Empty<MicroBundleDependencyRequest>();
    }

    public MicroBundleDescriptor Descriptor { get; }
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; }
    public SimpleDemoContext? Context { get; private set; }

    public void Load(IMicroBundleLoadContext context) =>
        Context = new SimpleDemoContext(allowConsoleInput: false);

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}

sealed class FoxDogCatalog : IMicroBundleCatalog
{
    private readonly IReadOnlyDictionary<ulong, IMicroBundle> _bundles;

    public FoxDogCatalog(params IMicroBundle[] bundles) =>
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
