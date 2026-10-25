# OntologyPaces — SimpleDemos

A runnable contract-level proving ground for **Ontology + ProtocolAI + manifest-driven MicroBundle delivery**.

This is an executable experiment inside SimpleDemos, not a new runtime framework. It deliberately keeps the demo's current in-memory transport separate from production adapters.

## Run it

The core project references the actual MicroBundleRepository Core source. From the SimpleDemos repository root, check out that repository into the expected sibling folder at the same pinned commit used by CI:

```bash
git clone https://github.com/TrentBest/TheSingularityWorkshop.MicroBundleRepository.git MicroBundleRepository
git -C MicroBundleRepository checkout 6098a455bb77009688021b41a2faa415c3392526
dotnet run --project OntologyPaces/OntologyPaces.csproj
```

The separate compiled-materializer probe is also part of CI and can be run with:

```bash
dotnet run --project OntologyPaces/MaterializerProbe/MaterializerProbe.csproj
```

The demo uses the published Ontology and ProtocolAI packages. CI checks out the real MicroBundleRepository source at the pinned commit, keeping integration reproducible without assuming that a NuGet package is published. It defines nine human-readable MicroBundle keys, maps those keys to ProtocolAI integer symbol IDs, uses those IDs as OntologyIndex values, and separately assigns each artifact a physical bundle ID and content-addressed artifact address.

## What this slice proves

- **String identity:** readable bundle names remain useful for authoring, diagnostics, and manifest inspection.
- **Integer identity:** ProtocolAI maps known vocabulary strings to symbol IDs; Ontology consumes the resulting indexes without referencing ProtocolAI.
- **Separate identities:** a ProtocolAI symbol ID is not a MicroBundle's physical bundle ID. An OntologyAddress is semantic metadata, not the blob path.
- **Real Core contract:** the demo uses `MicroBundleArtifactAddress`, `MicroBundleArtifact`, and `IMicroBundleRepository` from MicroBundleRepository.Core, not demo-local replicas.
- **Integrity and local cache behavior:** the in-memory repository implements the real interface; the local file cache verifies SHA-256 and stores bytes under bundle/version/hash.
- **Explicit order:** artifact retrieval and cache preparation follow the manifest order in this baseline.
- **Defaults and overrides:** the optional configuration overlay changes the fox bundle's values; a bundle without configuration retains defaults.
- **Real compiled assembly materialization:** CI compiles a fixture implementing `IMicroBundle`, packages its assembly bytes in the actual FSMB envelope, and invokes the real `AssemblyMicroBundleArtifactMaterializer` from the FSM_COS adapter. It verifies the discovered bundle descriptor/provider and rejection of mismatched artifact/envelope IDs, missing matching bundle IDs, and malformed FSMB bytes.

## Integration boundary: proven building block, end-to-end flow still open

At the pinned MicroBundleRepository commit, the repository contains these adapters:

- `src/Azure/TheSingularityWorkshop.MicroBundleRepository.Azure/AzureMicroBundleRepository.cs` implements `IMicroBundleRepository` using Azure Blob Storage and `DefaultAzureCredential`.
- `src/REST/TheSingularityWorkshop.MicroBundleRepository.FSM_COS/AssemblyMicroBundleArtifactMaterializer.cs` converts an assembly artifact into an `IMicroBundle`.
- `src/REST/TheSingularityWorkshop.MicroBundleRepository.FSM_COS/RestMicroBundleCatalog.cs` preloads a dependency closure and exposes loaded bundles through the FSM_COS catalog interface.

The current demo still uses an in-memory repository and a demo-local SHA-256-verified file cache. The compiled materializer probe proves assembly-to-`IMicroBundle` materialization independently, but it does **not yet connect repository retrieval/cache verification to materialization and catalog resolution in one end-to-end path**. It also does not instantiate Azure, validate Azure credentials/storage, consume a published adapter package, or change AnyApp's compiled-in catalog.

## Next integration milestone

Wire the existing verified artifact retrieval path to the compiled FSMB fixture and materializer, then prove the resulting bundle through the FSM_COS catalog contract. Keep Azure as a separate opt-in integration test requiring explicitly configured storage credentials. Only after that end-to-end contract path passes should AnyApp replace its hard-coded catalog or the demo add parallel download/cache preparation.

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
                           FSMB envelope + compiled assembly
                                          |
                                          v
                           FSM_COS materializer / catalog
                                          |
                                          v
                         defaults + optional configuration
                                          |
                                          v
                             manifest-ordered composition
```

Ontology is optional semantic structure, not a replacement for physical artifact identity or the runtime composition contract.
