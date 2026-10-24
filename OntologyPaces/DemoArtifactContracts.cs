using System.Security.Cryptography;
using TheSingularityWorkshop.Ontology;

namespace SimpleDemos.OntologyPaces;

internal readonly record struct DemoArtifactAddress
{
    public DemoArtifactAddress(ulong bundleId, string version, string contentHash)
    {
        if (bundleId == 0)
            throw new ArgumentOutOfRangeException(nameof(bundleId));
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        if (version.Contains('/') || version.Contains('\\'))
            throw new ArgumentException("Version must be a single storage-safe path segment.", nameof(version));
        if (contentHash.Length != 64 || contentHash.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("Content hash must be a 64-character SHA-256 value.", nameof(contentHash));
        BundleId = bundleId;
        Version = version;
        ContentHash = contentHash.ToLowerInvariant();
    }

    public ulong BundleId { get; }
    public string Version { get; }
    public string ContentHash { get; }
    public override string ToString() => $"{BundleId}/{Version}/{ContentHash}";
}

internal sealed class DemoArtifact
{
    private readonly byte[] _content;

    public DemoArtifact(DemoArtifactAddress address, ReadOnlyMemory<byte> content, OntologyAddress? semanticAddress = null)
    {
        var actualHash = Convert.ToHexString(SHA256.HashData(content.Span)).ToLowerInvariant();
        if (!string.Equals(actualHash, address.ContentHash, StringComparison.Ordinal))
            throw new ArgumentException("Artifact bytes do not match their SHA-256 address.", nameof(content));
        Address = address;
        SemanticAddress = semanticAddress;
        _content = content.ToArray();
    }

    public DemoArtifactAddress Address { get; }
    public OntologyAddress? SemanticAddress { get; }
    public ReadOnlyMemory<byte> Content => _content;
}

internal interface IDemoArtifactRepository
{
    ValueTask<DemoArtifact?> GetAsync(DemoArtifactAddress address, CancellationToken cancellationToken = default);
    ValueTask PutAsync(DemoArtifact artifact, CancellationToken cancellationToken = default);
}
