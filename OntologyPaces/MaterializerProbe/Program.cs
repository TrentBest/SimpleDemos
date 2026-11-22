using SimpleDemos.CompiledFixture;
using TheSingularityWorkshop.MicroBundleRepository.Core;
using TheSingularityWorkshop.MicroBundleRepository.FSM_COS;

const ulong fixtureId = 64001;
var assemblyBytes = await File.ReadAllBytesAsync(typeof(FixtureBundle).Assembly.Location);
var payloadBytes = new MicroBundleAssemblyPayload(fixtureId, assemblyBytes).ToBytes();
var artifact = MicroBundleArtifact.Create(fixtureId, "1.0.0", payloadBytes);
var materializer = new AssemblyMicroBundleArtifactMaterializer();

var bundle = materializer.Materialize(artifact);
Require(bundle.Id == fixtureId, "Materializer must return the bundle with the requested ID.");
Require(bundle.Descriptor.Version == "1.0.0", "Materializer must preserve the compiled descriptor.");
Require(bundle.Descriptor.Providers.Any(provider => provider.Id == "simpledemos.fixture"),
    "Materializer must discover the compiled fixture's provider.");

var wrongEnvelope = new MicroBundleAssemblyPayload(fixtureId, assemblyBytes).ToBytes();
var wrongIdArtifact = MicroBundleArtifact.Create(fixtureId + 1, "1.0.0", wrongEnvelope);
Expect<InvalidOperationException>(
    () => materializer.Materialize(wrongIdArtifact),
    "Materializer must reject a payload ID that differs from the artifact ID.");

var noMatchingBundlePayload = new MicroBundleAssemblyPayload(fixtureId + 1, assemblyBytes).ToBytes();
var noMatchingBundleArtifact = MicroBundleArtifact.Create(fixtureId + 1, "1.0.0", noMatchingBundlePayload);
Expect<InvalidOperationException>(
    () => materializer.Materialize(noMatchingBundleArtifact),
    "Materializer must reject an assembly without a bundle matching the artifact ID.");

var malformedPayload = payloadBytes.ToArray();
malformedPayload[0] ^= 0x01;
var malformedArtifact = MicroBundleArtifact.Create(fixtureId, "1.0.0", malformedPayload);
Expect<InvalidDataException>(
    () => materializer.Materialize(malformedArtifact),
    "Materializer must reject malformed FSMB bytes.");

Console.WriteLine("PASS: compiled IMicroBundle assembly materialized through the real FSM_COS adapter.");
Console.WriteLine("PASS: descriptor/provider discovered; mismatched IDs, missing bundle IDs, and malformed payloads rejected.");

static void Require(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

static void Expect<TException>(Action action, string message) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}
