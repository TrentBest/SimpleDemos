# OntologyPaces — SimpleDemos

A runnable contract-level proving ground for **Ontology + ProtocolAI + manifest-driven MicroBundle delivery**.

This is an executable experiment inside SimpleDemos, not a new runtime framework. It deliberately keeps the demo's current in-memory transport separate from production adapters.

## Run it

The project references the actual MicroBundleRepository Core source. From the SimpleDemos repository root, check out that repository into the expected sibling folder at the same pinned commit used by CI:

```bash
git clone https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository.git MicroBundleRepository
git -C MicroBundleRepository checkout 6098a455bb77009688021b41a2faa415c3392526
dotnet run --project OntologyPaces/OntologyPaces.csproj
```

The demo uses the published Ontology and ProtocolAI packages. CI checks out the real `TheSingularityWorkshop.MicroBundleRepository.Core` project at the pinned source commit, keeping integration reproducible without assuming that a NuGet package is published. It defines nine human-readable MicroBundle keys, maps those keys to ProtocolAI integer symbol IDs, uses those IDs as OntologyIndex values, and separately assigns each artifact a physical bundle ID and content-addressed artifact address.

## What this slice proves

- **String identity:** readable bundle names remain useful for authoring, diagnostics, and manifest inspection.
- **Integer identity:** ProtocolAI maps known vocabulary strings to symbol IDs; Ontology consumes the resulting indexes without referencing ProtocolAI.
- **Separate identities:** a ProtocolAI symbol ID is not a MicroBundle's physical bundle ID. An OntologyAddress is semantic metadata, not the blob path.
- **Real Core contract:** the demo uses `MicroBundleArtifactAddress`, `MicroBundleArtifact`, and `IMicroBundleRepository` from MicroBundleRepository.Core, not demo-local replicas.
- **Integrity and local cache behavior:** the in-memory repository implements the real interface; the local file cache verifies SHA-256 and stores bytes under bundle/version/hash.
- **Explicit order:** artifact retrieval and cache preparation follow the manifest order in this baseline.
- **Defaults and overrides:** the optional configuration overlay changes the fox bundle's values; a bundle without configuration retains defaults.

## Integration boundary: implemented elsewhere, not wired into this demo

At the pinned MicroBundleRepository commit, the repository already contains additional adapters:

- `src/Azure/TheSingularityWorkshop.MicroBundleRepository.Azure/AzureMicroBundleRepository.cs` implements `IMicroBundleRepository` using Azure Blob Storage and `DefaultAzureCredential`.
- `src/REST/TheSingularityWorkshop.MicroBundleRepository.FSM_COS/AssemblyMicroBundleArtifactMaterializer.cs` converts a verified assembly artifact into an `IMicroBundle`.
- `src/REST/TheSingularityWorkshop.MicroBundleRepository.FSM_COS/RestMicroBundleCatalog.cs` preloads a dependency closure and exposes loaded bundles through the FSM_COS catalog interface.

OntologyPaces currently exercises **Core only**: its repository is in-memory and its SHA-256-verified file cache is demo-local. It does not yet instantiate the Azure adapter, produce or load real compiled MicroBundle assembly payloads, call the FSM_COS catalog/materializer, or change AnyApp's compiled-in catalog. These are existing integration building blocks, but end-to-end wiring and evidence are still outstanding. The CI checkout compiles against the Core project; it does not establish NuGet package consumption or validate Azure credentials/storage.

## Next integration milestone

Add a deterministic test fixture containing a real serialized MicroBundle assembly payload, then prove the sequence from artifact address through repository retrieval and hash verification to materialization and FSM_COS catalog resolution. Keep Azure as a separate opt-in integration test requiring explicitly configured storage credentials. Only after that contract path passes should AnyApp replace its hard-coded catalog or the demo add parallel download/cache preparation.

Parallel batches remain deferred until correctness of identity, integrity, configuration precedence, materialization, and manifest ordering is demonstrated.

## Architecture under test

```text
manifest bundle key (string)
          |
          v
ProtocolAI vocabulary -> integer symbol ID
          |
          v
OntologyAddress + OntologyIndex
          |
          +--------------------------------+
          |                                |
          v                                v
semantic meaning                  MicroBundle artifact address
                                          |
                                          v
                               repository retrieves bytes
                                          |
                                          v
                                  SHA-256 verification
                                          |
                                          v
                                      local cache
                                          |
                                          v
                         defaults + optional configuration
                                          |
                                          v
                             manifest-ordered composition
```

Ontology is optional semantic structure, not a replacement for physical artifact identity or the runtime composition contract.
