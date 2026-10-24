# OntologyPaces — SimpleDemos

A runnable contract-level proving ground for **Ontology + ProtocolAI + manifest-driven MicroBundle delivery**.

This is an executable experiment inside SimpleDemos, not a new runtime framework and not a claim that Azure transport or AnyApp runtime materialization is already integrated.

## Run it

```bash
dotnet run --project OntologyPaces/OntologyPaces.csproj
```

The project uses the published Ontology and ProtocolAI packages. The MicroBundleRepository.Core contract is not yet available from NuGet, so this demo uses small local contract-shaped types rather than adding an unpublished package dependency. It defines nine human-readable MicroBundle keys, maps those keys to ProtocolAI integer symbol IDs, uses those IDs as OntologyIndex values, and separately assigns each artifact a physical bundle ID and content-addressed artifact address.

## What this slice proves

- **String identity:** readable bundle names remain useful for authoring, diagnostics, and manifest inspection.
- **Integer identity:** ProtocolAI maps known vocabulary strings to symbol IDs; Ontology consumes the resulting indexes without referencing ProtocolAI.
- **Separate identities:** a ProtocolAI symbol ID is not a MicroBundle's physical bundle ID. An OntologyAddress is semantic metadata, not the blob path.
- **Artifact contract shape:** the demo models the repository's current address/artifact/retrieval shape locally, without claiming it links to the unpublished MicroBundleRepository.Core package.
- **Integrity and local cache behavior:** the in-memory repository implements the real interface; the local file cache verifies SHA-256 and stores bytes under bundle/version/hash.
- **Explicit order:** artifact retrieval and cache preparation follow the manifest order in this baseline.
- **Defaults and overrides:** the optional configuration overlay changes the fox bundle's values; a bundle without configuration retains defaults.

## What this does not claim yet

The repository transport and contract-shaped types used by the demo are local test implementations. The cache is demo-local. The MicroBundleRepository.Core package is not currently published on NuGet, so the demo intentionally avoids depending on it until it can be consumed through an approved, reproducible source/package path. This slice does not contact Azure, deserialize real compiled MicroBundles, invoke FSM_COS composition, or alter AnyApp. Those are separate integration steps.

The Azure repository implementation uses the same artifact address contract and deterministic path. A next slice should exercise it with a controlled storage test or emulator, then establish how verified artifact bytes become materialized `IMicroBundle` instances and how AnyApp adopts a persistent local cache.

Parallel batches are intentionally deferred: first establish correctness of identity, integrity, configuration precedence, and deterministic ordering. Then add parallel download/cache preparation while preserving manifest-defined composition order.

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
