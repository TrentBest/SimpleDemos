# Demonstrations: prove the behavior, not just the picture

This folder is the evidence trail for demonstrations powered by The Singularity Workshop stack.

The central rule is simple: **a drawing of behavior is not evidence that FSM_API caused that behavior.** Each demonstration must say which parts are executable, which parts are presentation, and what has actually been verified.

## Proof ladder

| Stage | Artifact | What it proves | Status |
|---|---|---|---|
| 0 | [State contract](01-state-contract.svg) | Makes the fox/dog state and cross-agent transition contract inspectable. It is derived from the existing C# demo source. | **Created; diagram only** |
| 1 | Existing `QuickBrownFox` console demo | FSM_API creates state-machine definitions and instances; callbacks and transition conditions execute in C#. | **Existing implementation; needs a reproducible trace capture** |
| 2 | Structured behavior trace | Emits authoritative state/position/transition events from the C# process, with a deterministic scenario and assertions. | **Not implemented yet** |
| 3 | SVG view driven by trace/state data | SVG presentation reflects real output from the FSM_API-driven simulation instead of owning the behavior timeline. | **Not implemented yet** |
| 4 | Interactive browser control | User input changes simulation context; FSM_API decisions change the trace and SVG view. | **Not implemented yet** |
| 5 | Composition and rendering adapters | The same behavior contract can be consumed by GUI.SVG and later Renderer/Visualize or other hosts through explicit adapters. | **Future integration proof** |

Do not mark a stage complete because an SVG looks right. Mark it complete only when its evidence is reproducible.

## First demonstration: fox and sheepdog

The existing C# source is the behavioral starting point:

- `QuickBrownFox/QuickBrownFox.cs` defines `Walking`, `Jumping`, `Fleeing`, and `Mangled` states and transition conditions.
- `QuickBrownFox/LazySleepingDog.cs` defines `Sleeping`, `Awake`, `Chasing`, and `Mangling`; the dog's mangling enter callback requests a transition on the fox.
- `QuickBrownFox/Program.cs` advances the `Main` processing group using `FSM_API.Interaction.Update("Main")`.
- The console project references the published `TheSingularityWorkshop.FSM_API` NuGet package.

The existing browser page, `docs/fox-and-sheepdog.html`, is currently an illustrative browser-side model. Its animation must not be represented as proof that the C# assembly drives it. The 10×10 GUI.SVG scene is also a presentation artifact until it consumes a trace or state stream produced by the simulation.

## Evidence required for the next stage

1. Create a deterministic C# scenario that runs a fixed number of ticks.
2. Emit structured records for tick number, agent identity, state, position, and transition reason where available.
3. Assert expected transitions and positions; fail the run if they diverge.
4. Preserve a sample trace as a test fixture.
5. Build an SVG renderer/view that consumes those records or the same authoritative state model. It must not independently decide when the fox jumps or the dog wakes.
6. Add a browser-facing control path only after the state/trace contract is stable. Keep simulation time separate from wall-clock animation time.

## Interoperability boundary

FSM_API owns state-machine execution. Domain contexts own simulation data. FSM_COS may compose these capabilities when the application is ready to demonstrate that boundary. GUI.SVG owns SVG presentation. Renderer and Visualize should be added only through real, tested integration points. AnyApp/WebApp/MyVR are hosts or consumers—not claims that this small demo already exercises every layer.

The aim is to prove interoperability incrementally, then make the proof visually compelling.
