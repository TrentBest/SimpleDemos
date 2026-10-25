# SimpleDemos demonstration gallery

This page is the guided path through the repository. Each stage answers a different question; do not treat a picture, a successful build, a behavioral equivalence check, and a performance benchmark as interchangeable evidence.

## 1. See the behavior

**Start:** [Fox-and-sheepdog browser playground](../docs/fox-and-sheepdog.html)  
**Companion:** [Animated number-line SVG](../docs/assets/fox-dog-number-line.svg)

Explore the fox, sleeping dog, and behavior parameters in the browser. This is a hand-modeled illustration, not a live execution of the C# FSM_API assembly.

**Shows:** how visible behavior can be explained in terms of states, conditions, and reactions.  
**Does not prove:** that the browser simulation is executing the production C# FSM or that the two implementations are behaviorally identical.

## 2. Run the real FSM_API sample

From the repository root, with the .NET 8 SDK installed:

```powershell
dotnet run --project QuickBrownFox/QuickBrownFox.csproj --configuration Release
```

This executable uses the published `TheSingularityWorkshop.FSM_API` NuGet package (version `1.0.13`). It runs the original fox-and-dog sample and writes its observable behavior to the console.

To export a bounded, replayable trace instead:

```powershell
dotnet run --project QuickBrownFox/QuickBrownFox.csproj --configuration Release -- --export-trace docs/fox-fsm-trace.json
```

The trace records the actual C# run's frames, agent states, positions, and speeds. Replay it using `samples/PrimitiveGallery/fsm-trace-replay.html` in a local checkout of [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize).

**Shows:** actual FSM_API execution and a portable observation of that run.  
**Does not prove:** anything about FSM_COS composition until compared with the corresponding composed trace.

## 3. Compare direct execution with FSM_COS composition

The dedicated runner is `QuickBrownFox/FSM_COS/Program.cs`.

```powershell
dotnet run --project QuickBrownFox/FSM_COS/QuickBrownFox.FSM_COS.csproj --configuration Release -- fox-dog-fsm-cos-trace.json
```

The runner requires the `TheSingularityWorkshop.FSM_COS` repository checked out beside `SimpleDemos`. The `fsm-cos-stress-comparison` GitHub Actions workflow runs both direct and composed paths and compares their frame snapshots.

**Shows:** the same sample's observable behavior is preserved through the tested composition path when the direct and composed trace comparison passes.  
**Does not prove:** that FSM_COS owns the host's update loop, improves performance, or preserves every possible workload.

## 4. Scale up to the advanced 2D world

The advanced example models a 100 × 100 environment with multiple foxes and dogs. The direct path is:

```powershell
dotnet run --project QuickBrownFox/QuickBrownFox.csproj --configuration Release -- --export-advanced-trace advanced-fox-dog-direct-trace.json
```

The FSM_COS-composed path is:

```powershell
dotnet run --project QuickBrownFox/FSM_COS/Advanced/Advanced.FSM_COS.csproj --configuration Release -- advanced-fox-dog-fsm-cos-trace.json
```

The composed runner also requires the sibling FSM_COS checkout. CI compares the bounded, fixed-seed traces, including state, position, visibility, and collision observations.

**Shows:** equivalence for the bounded advanced scenario and observations covered by the automated comparison.  
**Does not prove:** full-scale simulation performance, all seeds, all agent counts, or universal equivalence.

## 5. Measure the worker workload

The original direct baseline is `Stress/Program.cs`. The FSM_COS comparison host is `Stress/FSM_COS/Program.cs`.

```powershell
dotnet run --project Stress/Stress.csproj --configuration Release
dotnet run --project Stress/FSM_COS/Stress.FSM_COS.csproj --configuration Release
```

The FSM_COS comparison runner needs the sibling FSM_COS checkout. It emits raw trial data and separates setup/composition costs from steady-state ticking. See the [benchmark protocol and current interpretation](../docs/FSM_COS_STRESS_BENCHMARK.md) before drawing conclusions.

**Current evidence:** two captures are not enough to establish a repeatable FSM_COS speedup. Paired outcomes varied between captures; allocation observations did not establish an advantage. Keep performance claims provisional until controlled order, additional independent runs, phase instrumentation, and robust statistics are in place.

**Does not prove:** a universal speedup or overhead percentage, behavior under every lifecycle/scaling pattern, or a product-readiness level.

## 6. Cross-repository orchestration proof

The [CiAi live-run demonstration](02-ciai-live-run.md) uses a GitHub Actions workflow to observe a known-good run from CiAi.

**Shows:** a read-only cross-repository run-evidence workflow.  
**Does not prove:** LLM-driven decisions, arbitrary tool execution, or a production orchestration loop.

---

## Evidence vocabulary

- **Illustration:** communicates a model; may be independently hand-authored.
- **Build/run:** source compiles and the named executable completes.
- **Trace:** values exported from a particular execution.
- **Equivalence check:** the tested observable outputs match under the comparison's stated inputs and scope.
- **Benchmark:** measured results under a documented workload and environment; conclusions remain bounded by the experimental design.

When linking a demonstration elsewhere, state which evidence level it provides and what remains unproven.
