<!-- The Singularity Workshop moniker -->
<p align="center">
  <a href="https://github.com/TrentBest">
    <img src="https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg" alt="The Singularity Workshop — animated workshop moniker" width="720" />
  </a>
</p>

<p align="center">
  <strong>Small simulations. Real state machines. Observable behavior.</strong><br/>
  <a href="https://github.com/TrentBest/FSM_API">FSM_API</a> ·
  <a href="https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API/">NuGet</a> ·
  <a href="https://github.com/TrentBest">The Singularity Workshop</a>
</p>

---

# SimpleDemos: The Fox, the Dog, and the FSM

<p align="center">
  <img src="docs/assets/fox-dog-number-line.svg" alt="Animated fox walking along a number line, spotting a sleeping sheepdog, jumping over it, and turning around to repeat." width="100%" />
</p>

**[Interactive fox-and-sheepdog playground source](docs/fox-and-sheepdog.html)** — clone the repo and open this HTML file in a browser to arm the distraction, tune behavior parameters, and watch the cartoon dust cloud, ghost, and tombstone sequence. GitHub displays the HTML source rather than running it in the README. For now, the browser playground is an explicitly modeled illustration; it does **not** execute the C# FSM_API assembly. The console project below does use the published FSM_API NuGet package.

**[Interactive visual primitive playground](docs/visual-primitives.html)** — clone the repo and open this HTML file in a browser to explore the drawing-side visual-test progression: one point, many points (up to 10,000), connected points, one line, and many lines. Change counts, colours, sizes, and position spread. GitHub shows the HTML source rather than running it in the README. This SVG browser experiment is separate from the C# FSM_API console demo and from Visualize's C# backend contracts.

**[Open the guided demonstration gallery](demonstrations/README.md)** — follow the progression from illustration, to actual FSM_API execution and replayable traces, to FSM_COS equivalence checks, advanced simulation, and measured stress workloads. Each stage states what its evidence does—and does not—establish.

**[New: Drone Construction living proof](projects/DroneConstruction/README.md)** — run the real deterministic construction simulation in CI, download the generated animated HTML artifact, and inspect the evidence and limits. This is a small real-snapshot showcase, not a claim of 100,000 construction agents or production GPU rendering.

A small C#/.NET playground for learning how finite state machines (FSMs) coordinate behavior. Start with one fox and one sleeping dog, inspect the larger 2D multi-agent implementation, run the separate worker-throughput benchmark, or explore the Drone Construction vertical slice.

**The point is not to animate a fox. The point is to make behavior explicit:** agents have state, conditions trigger transitions, and state callbacks determine what happens next.

## Export a trace from the real C# FSM

The browser playground above is an independent illustration. To create replayable evidence from the **actual C# FSM_API run**, execute this from the repository root:

```powershell
dotnet run --project QuickBrownFox/QuickBrownFox.csproj -- --export-trace docs/fox-fsm-trace.json
```

This bounded mode disables console chatter, runs the real processing groups and agent FSMs, and writes per-frame agent state, position, and speed snapshots to the requested JSON path. It stops when the sample reaches its normal end or after 30 updates. The trace is an exported observation of the C# run, not an animation by itself. Open `samples/PrimitiveGallery/fsm-trace-replay.html` from a local checkout of [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize), choose the JSON file you exported, and scrub or play the recorded frames. The browser replays recorded values; it does not execute FSM_API.

## FSM_COS stress comparison

The first source-integrated comparison runner is in [`Stress/FSM_COS`](Stress/FSM_COS/Program.cs). It composes the existing worker workload as a MicroBundle through FSM_COS, then lets the local console host own warm-up, ticking, timing, aggregation, and CSV output.

This project references the sibling FSM_COS repository source so the experiment can move with the active development branch before any NuGet release is assumed. Clone `TheSingularityWorkshop.FSM_COS` beside `SimpleDemos`, then run:

~~~powershell
dotnet run --project Stress/FSM_COS/Stress.FSM_COS.csproj --configuration Release
~~~

The runner uses an in-memory catalog to isolate composition from artifact delivery. It emits `fsm_cos_stress_comparison.csv` in the current working directory. Direct setup, FSM_COS composition, bundle-load time, and steady-state updates are reported separately. The CI equivalence gate requires **1,000 worker updates and 3,000 payload operations per measured tick** from both paths before it writes the CSV. Raw trial rows include runtime, OS, logical processor count, configuration, commit, and allocation observations; the console reports minimum, median, and maximum across seven trials.

[The latest passing CI run](https://github.com/TrentBest/SimpleDemos/actions/runs/40172944305) validates the build, execution, work-equivalence gate, and artifact upload. Its raw CSV is an early seven-trial sample with startup outliers, not a statistically conclusive benchmark. It does **not** establish a universal speedup or overhead claim.

The same workflow preserves direct and FSM_COS-composed fox-and-dog trace files so the identical-frame comparison can be inspected and replayed.

### Run the original fox-and-dog demo through FSM_COS

The separate [`QuickBrownFox/FSM_COS`](QuickBrownFox/FSM_COS/Program.cs) runner loads the existing `SimpleDemoContext` as a MicroBundle, then lets the host drive the same `Main` processing group and export observable frames:

```powershell
dotnet run --project QuickBrownFox/FSM_COS/QuickBrownFox.FSM_COS.csproj --configuration Release -- fox-dog-fsm-cos-trace.json
```

This source-integrated runner expects the FSM_COS repository checked out beside `SimpleDemos`, as described above. The validation workflow also exports the direct FSM_API trace and compares its frame snapshots with the FSM_COS-composed trace. The current CI check passed with identical observable frames. This demonstrates behavior preservation through composition; it does not claim FSM_COS owns the update loop or that the composition layer is faster.

### Run the advanced 2D fox-and-dog simulation through FSM_COS

The advanced 100 × 100 simulation now has a separate [FSM_COS runner](QuickBrownFox/FSM_COS/Advanced/Program.cs). It uses a fixed seed and a bounded 30-frame trace so the direct and composed paths can be compared reproducibly:

```powershell
dotnet run --project QuickBrownFox/FSM_COS/Advanced/Advanced.FSM_COS.csproj --configuration Release -- advanced-fox-dog-fsm-cos-trace.json
```

The direct control is `dotnet run --project QuickBrownFox/QuickBrownFox.csproj --configuration Release -- --export-advanced-trace advanced-fox-dog-direct-trace.json`. CI compares the full observable snapshots for all foxes and dogs, including state, position, visibility, and collision data. The passing run produced identical frames and preserves both trace files in the advanced trace artifact.

The direct baseline remains in [`Stress/Program.cs`](Stress/Program.cs). The measurement rules and known historical-baseline caveats are in [`FSM_COS stress benchmark protocol`](docs/FSM_COS_STRESS_BENCHMARK.md).

## AI-facing CI proof

SimpleDemos also hosts the first cross-repository [CiAi live-run demonstration](demonstrations/02-ciai-live-run.md). The [GitHub Actions workflow](https://github.com/TrentBest/SimpleDemos/actions/workflows/ciai-live-demo.yml) runs automatically on pushes to `development`, observing a known-good run from CiAi. Inspect its logs for the read-only facts and evidence URL. This is provider-ingestion proof—not yet an LLM-driven or action-executing workflow.

## 🔵 00 · The problem

A fox walks toward a goal. A dog is asleep in its path. The fox can see obstacles, jump, collide, or flee; the dog can wake, chase, and catch the fox. The outcome depends on what each agent can observe and which transition conditions become true.

Without an explicit model, this behavior can quickly become a tangle of nested `if` statements. Here, each agent's behavior is organized in terms of explicit state, transition conditions, and callbacks.
