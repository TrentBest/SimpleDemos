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

A small C#/.NET playground for learning how finite state machines (FSMs) coordinate behavior. Start with one fox and one sleeping dog, inspect the larger 2D multi-agent implementation, or run the separate worker-throughput benchmark.

**The point is not to animate a fox. The point is to make behavior explicit:** agents have state, conditions trigger transitions, and state callbacks determine what happens next.

## Export a trace from the real C# FSM

The browser playground above is an independent illustration. To create replayable evidence from the **actual C# FSM_API run**, execute this from the repository root:

```powershell
dotnet run --project QuickBrownFox/QuickBrownFox.csproj -- --export-trace docs/fox-fsm-trace.json
```

This bounded mode disables console chatter, runs the real processing groups and agent FSMs, and writes per-frame agent state, position, and speed snapshots to the requested JSON path. It stops when the sample reaches its normal end or after 30 updates. The trace is an exported observation of the C# run, not an animation by itself. Open `samples/PrimitiveGallery/fsm-trace-replay.html` from a local checkout of [Visualize](https://github.com/TrentBest/TheSingularityWorkshop.Visualize), choose the JSON file you exported, and scrub or play the recorded frames. The browser replays recorded values; it does not execute FSM_API.

## 🔵 00 · The problem

A fox walks toward a goal. A dog is asleep in its path. The fox can see obstacles, jump, collide, or flee; the dog can wake, chase, and catch the fox. The outcome depends on what each agent can observe and which transition conditions become true.

Without an explicit model, this behavior can quickly become a tangle of nested `if` statements. Here, each agent's behavior is organized into named states and transitions.

## 🟩 01 · What this demonstrates

- **FSM definitions:** build named states, transition conditions, and enter/update/exit callbacks.
- **Context-driven behavior:** each FSM operates on an ordinary C# context implementing `IStateContext`.
- **Multiple instances:** the same named FSM definition can control individual agents.
- **Processing groups:** the demo separates application-level work from agent updates.
- **Interaction between FSMs:** when the dog catches the fox, the dog's behavior requests a transition on the fox's FSM.
- **A larger simulation:** the repository also contains an advanced 2D simulation implementation with multiple foxes and dogs.
- **A separate worker-throughput benchmark:** explores high-volume FSM update throughput. This is a workload experiment, not a portable benchmark result.

## 🟧 02 · Why FSM_API?

FSM_API separates *what state an agent is in* from *the data describing that agent*. The fox does not need to know how the application loop is implemented; it defines behavior and exposes context. The application coordinates sensing and updates.

That makes the behavior inspectable and easier to extend: add a state, define a transition, and implement the callbacks without rewriting the entire simulation loop.

## 🔺 03 · How the agents behave

### 🦊 The quick brown fox

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Walking
    Walking --> Jumping : ShouldJump
    Jumping --> Walking : ShouldLand
    Walking --> Fleeing : ShouldFlee
    Mangled : externally requested state
    note right of Mangled
      Dog requests this state
      on a successful catch.
      No normal transition is declared.
    end note
```

The basic fox definition includes `Walking`, `Jumping`, `Fleeing`, and `Mangled`. It jumps when an obstacle is close, returns to walking when its jump completes, and flees after a dog collision. The dog can directly request the fox's `Mangled` state.

### 🐕 The lazy sleeping dog

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Sleeping
    Sleeping --> Awake : collision wakes dog
    Awake --> Chasing : fox is visible
    Chasing --> Mangling : fox collision
    Mangling --> [*]
```

The dog starts asleep. A collision wakes it; seeing the fox allows it to chase; a collision while chasing triggers its mangling behavior and requests the fox's `Mangled` state.

### 🦊🐕 Many agents, shared environment

<p align="center">
  <img src="docs/assets/many-foxes-and-dogs.svg" alt="Schematic multi-agent world with multiple foxes and dogs, showing example visibility ranges and shared environment sensing." width="100%" />
</p>

The advanced source files extend the same idea into a 100 × 100 2D environment with multiple foxes and dogs, destinations, visibility, and collision checks. These files are an advanced implementation to explore; **the current console entry point launches the basic demo and optional stress test, not the advanced simulation**. That distinction is intentional here so the documentation does not claim an execution path that the current entry point does not provide.

## ⭐ 04 · See it in a minute

### Requirements

- .NET 8 SDK
- Internet access on first restore, to retrieve the published FSM_API NuGet package

### Run the fox-and-dog demo

From the repository root:

```bash
dotnet run --project QuickBrownFox/QuickBrownFox.csproj
```

This executable demo references the published `TheSingularityWorkshop.FSM_API` NuGet package (version `1.0.13`) through `QuickBrownFox.csproj`. Restore resolves the package containing the compiled FSM_API assembly; the project does not depend on a developer-specific local DLL path. Watch the console output as the fox walks, detects the dog, jumps, and updates its position. The dog reacts to the shared environment's visibility and collision results. The demo ends when the fox reaches its goal, is mangled, or a key is available to stop the run.

When prompted, press **Y** to start the in-process *Gauntlet* stress simulation; press any other key to skip it. The Gauntlet can grow its population dynamically and may consume substantial CPU, so skip it for the first tour.

### Restore and build the solution

```bash
dotnet restore SimpleDemos.slnx
dotnet build SimpleDemos.slnx --no-restore
```

The separate worker-throughput benchmark is launched with `dotnet run --project Stress/Stress.csproj`. It writes `benchmark_data.csv` and `article_table.txt` into its working directory and can attempt very large worker counts. Run it deliberately, not as part of a quick tour. Results depend on the machine, runtime, and workload; do not treat one run as a universal performance claim.

`MultiThreadedStress/` is currently a scaffold that prints `Hello, World!`; it is not yet a multithreaded FSM demonstration.

### Explore the implementation

| Start here | What to look for |
| :--- | :--- |
| [`Program.cs`](QuickBrownFox/Program.cs) | Application loop and optional stress-test prompt |
| [`SimpleDemoContext.cs`](QuickBrownFox/SimpleDemoContext.cs) | Environment sensing, processing groups, and shutdown |
| [`QuickBrownFox.cs`](QuickBrownFox/QuickBrownFox.cs) | Fox states, callbacks, and transition conditions |
| [`LazySleepingDog.cs`](QuickBrownFox/LazySleepingDog.cs) | Dog states and cross-FSM transition request |
| [`AdvancedDemoContext.cs`](QuickBrownFox/AdvancedDemoContext.cs) | Advanced multi-agent simulation setup |
| [`AdvancedDemoEnvironment.cs`](QuickBrownFox/AdvancedDemoEnvironment.cs) | Shared environment and agent sensing |
| [`Stress/Stress.cs`](Stress/Stress.cs) | Stress-test worker context and update workload |

### The pattern in miniature

An FSM definition describes behavior once; each instance supplies its own context:

```csharp
FSM_API.Create.CreateFiniteStateMachine("QuickBrownFoxFSM", -1, "Update")
    .State("Walking", OnEnterWalking, OnUpdateWalking, OnExitWalking)
    .State("Jumping", OnEnterJump, OnUpdateJumping, OnExitJumping)
    .State("Fleeing", OnEnterFleeing, OnUpdateFleeing, OnExitFleeing)
    .Transition("Walking", "Jumping", ShouldJump)
    .Transition("Jumping", "Walking", ShouldLand)
    .Transition("Walking", "Fleeing", ShouldFlee)
    .BuildDefinition();
```

This is an excerpt of the pattern in the source, not a standalone program: the callbacks and predicates are methods on the fox context. Read the full implementation for the data each condition inspects.

### Documentation and theory

- [FSM_API repository](https://github.com/TrentBest/FSM_API) — source, guides, and tests
- [FSM_API on NuGet](https://www.nuget.org/packages/TheSingularityWorkshop.FSM_API/) — install the package
- [FSM builder deep dive](https://github.com/TrentBest/FSM_API/blob/development/Documentation/User%20Guide/04_FSM_Builder_Deep_Dive.md) — states, transitions, and construction
- [The Singularity Workshop](https://github.com/TrentBest) — related projects and experiments

---

<p align="center">
  <strong>Understand the behavior. Inspect the state. Change the system.</strong><br/>
  <a href="https://github.com/TrentBest">The Singularity Workshop</a>
</p>
