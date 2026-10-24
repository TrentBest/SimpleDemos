# OntologyPaces — SimpleDemos

A runnable contract-level proving ground for **Ontology + ProtocolAI + manifest-driven MicroBundle delivery**.

This is deliberately an executable experiment inside SimpleDemos, not a new runtime framework and not a claim that the Azure repository or AnyApp cache has already been integrated.

## Run it

```bash
dotnet run --project OntologyPaces/OntologyPaces.csproj
```

The project uses the published Ontology and ProtocolAI packages. It defines nine human-readable MicroBundle keys, maps those keys to ProtocolAI integer symbol IDs, uses those IDs as `OntologyIndex` values, and gives each semantic address a distinct repository blob locator and content hash.

## What the first slice proves

- **String identity:** readable bundle names remain useful for authoring, diagnostics, and manifest inspection.
- **Integer identity:** ProtocolAI deterministically maps known strings to symbol IDs; Ontology consumes the resulting integers without referencing ProtocolAI.
- **Semantic identity is not storage location:** an `OntologyAddress` is not the blob path. The manifest entry holds both.
- **Explicit order:** entries load in manifest order in this baseline.
- **Integrity and local cache behavior:** the deterministic local repository test double returns bytes; the cache verifies SHA-256 and stores content under a bundle/version/hash key.
- **Defaults and overrides:** a sample optional configuration overlay changes the fox bundle's values; a bundle without a configuration file retains its defaults.

## What this does not claim yet

The repository and cache are local test doubles. This first slice does not contact Azure, deserialize real compiled MicroBundles, invoke FSM_COS composition, or alter AnyApp. The next integration step should replace the test doubles with the actual `IMicroBundleRepository` / artifact contracts and AnyApp's cache boundary only after their current API contracts are inspected and aligned.

Parallel batches are intentionally not enabled yet: first establish the correctness of identity, integrity, configuration precedence, and deterministic ordering. Then add parallel download/cache preparation while preserving manifest-defined composition order.

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
          +------------------------------+
          |                              |
          v                              v
semantic meaning                  repository blob locator
                                         |
                                         v
                              version + SHA-256 verification
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

The ontology is optional semantic structure, not a replacement for the repository's physical artifact address or the runtime's composition contract.
