using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Ontology;
using TheSingularityWorkshop.MicroBundleRepository.Core;
using TheSingularityWorkshop.ProtocolAi;

const ulong ontologyId = 7300;
const ulong protocolId = 7301;
const string version = "1.0.0";

var manifestPath = Path.Combine(AppContext.BaseDirectory, "manifest.example.json");
var manifest = JsonSerializer.Deserialize<DemoManifest>(
    await File.ReadAllTextAsync(manifestPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new InvalidDataException("The example manifest could not be parsed.");
var bundleNames = manifest.LoadOrder;

// ProtocolAI vocabulary IDs are semantic lookup keys, NOT physical repository bundle IDs.
var protocolBuilder = new ProtocolBuilder(protocolId, "SimpleDemos MicroBundle vocabulary");
for (var i = 0; i < bundleNames.Length; i++)
    protocolBuilder.Define((ulong)(9001 + i), $"bundle{i + 1}", bundleNames[i]);
var protocol = protocolBuilder.Build();

var ontology = new OntologyDefinition(
    ontologyId,
    "SimpleDemos",
    version,
    [
        new OntologyLayerDefinition(1, "bundle", 0),
        new OntologyLayerDefinition(2, "capability", 1)
    ]);
ontology.Validate();

var ontologySet = new OntologySet();
ontologySet.Add(ontology);

var entries = bundleNames.Select((name, i) =>
{
    var symbol = protocol.ReferenceByValue(name);
    var index = new OntologyIndex(symbol.SymbolId);
    var semanticAddress = ontology.Address($"bundles/{name}", index);

    // Physical bundle identity is independent of the ProtocolAI symbol ID.
    var bundleId = (ulong)(50001 + i);
    var bytes = Encoding.UTF8.GetBytes($"demo-artifact:{name}:{version}");
    var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    var artifactAddress = new MicroBundleArtifactAddress(bundleId, version, hash);

    return new DemoManifestEntry(
        Order: i,
        BundleName: name,
        BundleId: bundleId,
        SymbolId: symbol.SymbolId,
        SemanticAddress: semanticAddress,
        ArtifactAddress: artifactAddress,
        ConfigurationFile: manifest.ConfigurationFiles.GetValueOrDefault(name),
        Defaults: new Dictionary<string, string>
        {
            ["enabled"] = "true",
            ["mode"] = "default",
            ["speed"] = name == "agents.fox" ? "4" : "1"
        });
}).ToArray();

// Exercise the real Core repository interface with deterministic in-memory transport.
var repository = new InMemoryArtifactRepository();
foreach (var entry in entries)
{
    var bytes = Encoding.UTF8.GetBytes($"demo-artifact:{entry.BundleName}:{version}");
    var artifact = new MicroBundleArtifact(entry.ArtifactAddress, bytes, entry.SemanticAddress);
    await repository.PutAsync(artifact);
}

// Optional configuration overlays defaults; absent configuration leaves defaults untouched.
var fox = entries.Single(x => x.BundleName == "agents.fox");
var foxEffective = fox.Defaults.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
if (fox.ConfigurationFile is { } foxConfigPath)
{
    var fullConfigPath = Path.Combine(AppContext.BaseDirectory, foxConfigPath.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(fullConfigPath))
    {
        var overrides = JsonSerializer.Deserialize<Dictionary<string, string>>(
            await File.ReadAllTextAsync(fullConfigPath))
            ?? throw new InvalidDataException($"Configuration '{foxConfigPath}' could not be parsed.");
        foreach (var pair in overrides)
            foxEffective[pair.Key] = pair.Value;
    }
}

var cacheRoot = Path.Combine(Path.GetTempPath(), "SimpleDemos-OntologyPaces", Guid.NewGuid().ToString("N"));
var cache = new VerifiedFileCache(cacheRoot);
var loaded = new List<string>();

foreach (var entry in entries.OrderBy(x => x.Order))
{
    var artifact = await cache.GetOrDownloadAsync(entry.ArtifactAddress, repository);
    loaded.Add(entry.BundleName);
    Console.WriteLine($"[{entry.Order + 1}] {entry.BundleName,-22} symbol={entry.SymbolId,-5} bundle={entry.BundleId}");
    Console.WriteLine($"    ontology: {entry.SemanticAddress}");
    Console.WriteLine($"    blob:     {GetBlobName(entry.ArtifactAddress)}");
    Console.WriteLine($"    cache:    {cache.GetPath(entry.ArtifactAddress)} ({artifact.Content.Length} bytes)");
}

Require(loaded.SequenceEqual(bundleNames), "Manifest order must be preserved.");
Require(entries.Select(x => x.SymbolId).Distinct().Count() == bundleNames.Length,
    "Protocol symbol IDs must resolve to distinct ontology indexes.");
Require(entries.Select(x => x.BundleId).Distinct().Count() == bundleNames.Length,
    "Physical MicroBundle IDs must be distinct from one another.");
Require(entries.All(x => x.SymbolId != x.BundleId),
    "Protocol symbol IDs and physical bundle IDs must not be conflated.");
Require(foxEffective["speed"] == "7" && foxEffective["mode"] == "demonstration",
    "Configuration values must override bundle defaults.");
Require(entries.Single(x => x.BundleName == "agents.sheepdog").Defaults["speed"] == "1",
    "Bundles without a configuration file must retain their defaults.");
Require(ontologySet.TryGet(ontologyId, version, out _), "OntologySet should register the definition.");

// Prove that content-addressing rejects tampered bytes and that the file cache
// refuses to silently accept a corrupted local copy.
var integrityProbe = entries[0];
var integrityPath = cache.GetPath(integrityProbe.ArtifactAddress);
var originalBytes = await File.ReadAllBytesAsync(integrityPath);
var corruptedBytes = originalBytes.ToArray();
corruptedBytes[0] ^= 0x01;
await File.WriteAllBytesAsync(integrityPath, corruptedBytes);
var cacheRejectedCorruption = false;
try
{
    await cache.GetOrDownloadAsync(integrityProbe.ArtifactAddress, repository);
}
catch (InvalidDataException)
{
    cacheRejectedCorruption = true;
}
Require(cacheRejectedCorruption, "The cache must reject bytes that do not match the expected SHA-256.");
await File.WriteAllBytesAsync(integrityPath, originalBytes);

var constructorRejectedCorruption = false;
try
{
    _ = new MicroBundleArtifact(integrityProbe.ArtifactAddress, corruptedBytes);
}
catch (ArgumentException)
{
    constructorRejectedCorruption = true;
}
Require(constructorRejectedCorruption, "Artifact construction must reject bytes that do not match the address hash.");

Console.WriteLine();
Console.WriteLine($"PASS: {loaded.Count} bundle keys mapped; manifest order preserved; real Core artifact contract exercised; SHA-256 corruption rejected; overrides applied.");
Console.WriteLine("NOTE: repository transport is an in-memory implementation of the real contract; Azure and AnyApp materialization are not yet connected.");

static string GetBlobName(MicroBundleArtifactAddress address) =>
    $"artifacts/{address.BundleId}/{address.Version}/{address.ContentHash}.bundle";

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed record DemoManifest(
    int SchemaVersion,
    string Purpose,
    string[] LoadOrder,
    Dictionary<string, string> ConfigurationFiles);

sealed record DemoManifestEntry(
    int Order,
    string BundleName,
    ulong BundleId,
    ulong SymbolId,
    OntologyAddress SemanticAddress,
    MicroBundleArtifactAddress ArtifactAddress,
    string? ConfigurationFile,
    IReadOnlyDictionary<string, string> Defaults);

sealed class InMemoryArtifactRepository : IMicroBundleRepository
{
    private readonly Dictionary<MicroBundleArtifactAddress, MicroBundleArtifact> _artifacts = [];

    public ValueTask<MicroBundleArtifact?> GetAsync(
        MicroBundleArtifactAddress address,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _artifacts.TryGetValue(address, out var artifact);
        return ValueTask.FromResult(artifact);
    }

    public ValueTask PutAsync(
        MicroBundleArtifact artifact,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(artifact);
        _artifacts.TryAdd(artifact.Address, artifact);
        return ValueTask.CompletedTask;
    }
}

sealed class VerifiedFileCache(string root)
{
    public string GetPath(MicroBundleArtifactAddress address) =>
        Path.Combine(root, address.BundleId.ToString(), address.Version, $"{address.ContentHash}.bundle");

    public async Task<MicroBundleArtifact> GetOrDownloadAsync(
        MicroBundleArtifactAddress address,
        IMicroBundleRepository repository)
    {
        var path = GetPath(address);
        if (File.Exists(path))
        {
            var cached = await File.ReadAllBytesAsync(path);
            Verify(cached, address.ContentHash, "cached");
            return new MicroBundleArtifact(address, cached);
        }

        var artifact = await repository.GetAsync(address)
            ?? throw new FileNotFoundException($"Repository artifact '{address}' was not found.");
        Verify(artifact.Content.Span, address.ContentHash, "downloaded");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, artifact.Content.ToArray());
        return artifact;
    }

    private static void Verify(ReadOnlySpan<byte> bytes, string expectedHash, string source)
    {
        var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"SHA-256 mismatch for {source} artifact.");
    }
}
