# CiAi: observe a real CI run

This demonstration uses the read-only CiAi client from [TheSingularityWorkshop.CiAi](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/tree/development) to inspect an actual GitHub Actions run. It does not run an LLM or perform CI operations.

## Run the demonstration

1. Open [CiAi Live Run Demonstration](https://github.com/TrentBest/SimpleDemos/actions/workflows/ciai-live-demo.yml).
2. Select **Run workflow** on the `development` branch.
3. Enter a public repository in `owner/repository` form and an existing Actions run ID.
4. Inspect the workflow log for the normalized run facts and evidence URL.

For example, use `TrentBest/SimpleDemos` and a recent run ID from its Actions page. A token is not required for public repositories. A private target requires a suitably scoped token stored as the optional `CIAI_GITHUB_TOKEN` repository secret.

## What this proves

- The demonstration repository can consume CiAi from its development branch without copying CiAi source into SimpleDemos.
- CiAi fetches run facts using a read-only GitHub API request.
- The returned repository and run ID are checked against the request.
- Unknown future conclusions remain explicitly unknown and are not treated as success.
- The output includes a GitHub evidence URL so a person can verify the result.

**Scope:** this is the first provider-ingestion proof, not yet an LLM-driven interaction. HeadlessAi transport compatibility is tested separately; ProtocolAi vocabulary composition and permissioned actions remain future work.
