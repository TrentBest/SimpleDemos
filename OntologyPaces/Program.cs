using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheSingularityWorkshop.Ontology;
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

// ProtocolAI owns the vocabulary-to-integer mapping. Ontology remains protocol-neutral.
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
    var reference = protocol.ReferenceByValue(name);
    var index = new OntologyIndex(reference.SymbolId);
    var address = ontology.Address($"bundles/{name}", index);
    var bytes = Encoding.UTF8.GetBytes($"demo-artifact:{name}:1.0.0");
    var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    return new DemoManifestEntry(
        Order: i,
        BundleName: name,
        BundleVersion: "1.0.0",
        SemanticAddress: address.ToString(),
        OntologyIndex: index.Value,
        RepositoryBlob: $"microbundles/artifacts/{reference.SymbolId}/1.0.0/{hash}.bundle",
        ContentHash: hash,
        ConfigurationFile: manifest.ConfigurationFiles.GetValueOrDefault(name),
        Defaults: new Dictionary<string, string>
        {
            ["enabled"] = "true",
            ["mode"] = "default",
            ["speed"] = name == "agents.fox" ? "4" : "1"
        });
}).ToArray();

// Optional configuration overlays defaults; absence means defaults remain untouched.
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

// The fake repository/cache is deliberately local and deterministic. It proves identity,
// integrity, cache-key separation, and manifest order without claiming Azure is contacted.
var repository = new InMemoryArtifactRepository();
foreach (var entry in entries)
{
    var bytes = Encoding.UTF8.GetBytes($"demo-artifact:{entry.BundleName}:{entry.BundleVersion}");
    repository.Add(entry.RepositoryBlob, bytes, entry.ContentHash);
}

var cacheRoot = Path.Combine(Path.GetTempPath(), "SimpleDemos-OntologyPaces", Guid.NewGuid().ToString("N"));
var cache = new VerifiedFileCache(cacheRoot);
var loaded = new List<string>();

foreach (var entry in entries.OrderBy(x => x.Order))
{
    var cacheKey = $"{entry.BundleName}/{entry.BundleVersion}/{entry.ContentHash}";
    var bytes = await cache.GetOrDownloadAsync(
        cacheKey,
        entry.RepositoryBlob,
        entry.ContentHash,
        repository);
    loaded.Add(entry.BundleName);
    Console.WriteLine($"[{entry.Order + 1}] {entry.BundleName,-22} index={entry.OntologyIndex,-5} {entry.SemanticAddress}");
    Console.WriteLine($"    blob:  {entry.RepositoryBlob}");
    Console.WriteLine($"    cache: {cache.GetPath(cacheKey)} ({bytes.Length} bytes)");
}

Require(loaded.SequenceEqual(bundleNames), "Manifest order must be preserved.");
Require(entries.Select(x => x.OntologyIndex).Distinct().Count() == bundleNames.Length,
    "Protocol symbol IDs must resolve to distinct ontology indexes.");
Require(foxEffective["speed"] == "7" && foxEffective["mode"] == "demonstration",
    "Configuration values must override bundle defaults.");
Require(entries.Single(x => x.BundleName == "agents.sheepdog").Defaults["speed"] == "1",
    "Bundles without a configuration file must retain their defaults.");
Require(ontologySet.TryGet(ontologyId, version, out _), "OntologySet should register the definition.");

Console.WriteLine();
Console.WriteLine($"PASS: {loaded.Count} semantic bundle keys resolved; order preserved; overrides applied; cache content verified.");
Console.WriteLine("NOTE: repository transport and cache are deterministic local test doubles, not Azure/AnyApp integration.");

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
    string BundleVersion,
    string SemanticAddress,
    ulong OntologyIndex,
    string RepositoryBlob,
    string ContentHash,
    string? ConfigurationFile,
    IReadOnlyDictionary<string, string> Defaults);

sealed class InMemoryArtifactRepository
{
    private readonly Dictionary<string, byte[]> _artifacts = new(StringComparer.Ordinal);

    public void Add(string blobName, byte[] bytes, string expectedHash)
    {
        var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            throw new InvalidOperationException($"Test artifact hash mismatch for '{blobName}'.");
        _artifacts.Add(blobName, bytes.ToArray());
    }

    public ValueTask<byte[]> DownloadAsync(string blobName)
    {
        if (!_artifacts.TryGetValue(blobName, out var bytes))
            throw new FileNotFoundException($"Repository artifact '{blobName}' was not found.");
        return ValueTask.FromResult(bytes.ToArray());
    }
}

sealed class VerifiedFileCache(string root)
{
    public string GetPath(string cacheKey) =>
        Path.Combine(root, cacheKey.Replace('/', Path.DirectorySeparatorChar) + ".bundle");

    public async Task<byte[]> GetOrDownloadAsync(
        string cacheKey,
        string blobName,
        string expectedHash,
        InMemoryArtifactRepository repository)
    {
        var path = GetPath(cacheKey);
        if (File.Exists(path))
        {
            var cached = await File.ReadAllBytesAsync(path);
            Verify(cached, expectedHash, "cached");
            return cached;
        }

        var downloaded = await repository.DownloadAsync(blobName);
        Verify(downloaded, expectedHash, "downloaded");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, downloaded);
        return downloaded;
    }

    private static void Verify(byte[] bytes, string expectedHash, string source)
    {
        var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
            throw new InvalidDataException($"SHA-256 mismatch for {source} artifact.");
    }
}
