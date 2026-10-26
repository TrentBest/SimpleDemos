# The Singularity Workshop — Article Notebook

This is the editorial notebook for possible future Coder Legion stories across The Singularity Workshop. It defines the **territory worth investigating**, not a publication schedule or a list of claims already proven. Each idea should earn its place through a distinct question, source inspection, executable evidence, measurements, or a concrete demonstration.

Published articles are indexed separately in [Coder Legion Articles](CODER_LEGION_ARTICLES.md). Check that index before drafting so a new story extends the record rather than retelling it.

## Editorial rule

Write as an investigator, not a product brochure: start with a question, inspect the implementation, run a reproducible experiment, distinguish what works today from what is planned, and link to the code and prior articles. Do not claim cross-repository integration just because the architecture intends it. When a feature is unfinished, the story may be about the engineering problem and the proof still needed.

## 1. SimpleDemos: from familiar example to evidence

### 1.1 The Fox Is Moving. But Who's Actually Driving?
**Question:** Which parts of the fox-and-sheepdog experience are driven by compiled C# and FSM_API, and which are illustrative presentation?  
**Coverage:** Walk through the executable console demo and the browser-side illustration; make the boundary explicit. Use this as an evidence-led follow-up to the original fox-and-dog article, not a retelling of the FSM premise.  
**Proof needed:** Run instructions, source references, and an honest description of the browser model's independence from the compiled assembly.

### 1.2 From a Picture to a Trace
**Question:** What would it take for an animation to prove the same behavior that the program executed?  
**Coverage:** Fixed-tick scenario, structured records for tick/agent/state/position/transition reason, assertions, and a preserved sample trace. Explain why source-derived diagrams and animations are not runtime evidence by themselves.  
**Proof needed:** Implement the trace and assertions before presenting a visualization as trace-driven.

### 1.3 One Fox, Many Agents, Different Questions
**Question:** What changes when a small behavior example becomes a multi-agent simulation?  
**Coverage:** Separate the basic fox/dog behavior, advanced multi-agent scene, and stress scenario. Explain which domain logic each includes and excludes.  
**Proof needed:** Reproducible scenario settings and explicit feature boundaries; do not recycle the older 33,000/66,000-agent narratives without new measurements.

### 1.4 What Does This Stress Test Actually Measure?
**Question:** Is a result measuring FSM dispatch, domain behavior, allocation, scheduling, or all of them?  
**Coverage:** Contrast the worker-throughput stress test with the full fox/dog simulation. Identify collision/perception work that is excluded and distinguish setup/composition cost from steady-state ticking.  
**Proof needed:** Exact commit/build/runtime, worker count, warm-up, trial count, measured interval, allocations, and raw output.

### 1.5 A Demonstration That Can Fail
**Question:** What makes a software demonstration falsifiable rather than merely persuasive?  
**Coverage:** Turn expected transitions and positions into assertions; show a deliberate failure; preserve evidence. Establish a repeatable standard for future Workshop demos.  
**Proof needed:** A deterministic test scenario and CI result.

## 2. FSM_API: state-machine runtime and reusable behavior

### 2.1 A Blueprint Is Not an Agent
**Question:** How can multiple contexts reuse a state-machine definition without sharing their runtime identity or domain data?  
**Coverage:** Explain definition versus instance, context ownership, callbacks, transitions, and cross-machine requests using concrete code.  
**Proof needed:** Tests demonstrating independent agents and their separate states.

### 2.2 The Cost of Composing Work
**Question:** What does it cost to create or compose an FSM compared with ticking an already-built one?  
**Coverage:** Separate construction, registration, lookup, and steady-state update measurements. Connect the results to real workload choices instead of a single headline number.  
**Proof needed:** Reproducible benchmark and comparable baselines; explicitly avoid implying old benchmarks measured a different workload.

### 2.3 String Identities, Integer Identities, and the Migration Contract
**Question:** What is gained by integer-backed identities, and what compatibility obligations follow?  
**Coverage:** Compare lookup approaches, mapping costs, handle lifetime, sparse IDs, and the boundary between public names and internal representation.  
**Proof needed:** Correctness tests plus before/after benchmarks on the same scenarios. Treat optimization as a measured result, not a foregone conclusion.

### 2.4 Safe Invalidation Versus Destruction
**Question:** When should an FSM become invalid, and when should its resources be fully destroyed?  
**Coverage:** Handle stability, reuse, cleanup, array reduction, and lifecycle semantics.  
**Proof needed:** Lifecycle tests for repeated create/invalidate/reuse/destroy cycles and documented behavior at edge cases.

### 2.5 Determinism Has a Boundary
**Question:** Which parts of a system can be deterministic when clocks, input, scheduling, and external services remain outside the state machine?  
**Coverage:** Define deterministic state transitions separately from host timing, input order, simulation time, and presentation animation.  
**Proof needed:** A repeatable fixed-tick scenario and explicit assumptions.

## 3. FSM_COS: composition as an operating boundary

### 3.1 FSM_COS Is the Composer, Not Every Component
**Question:** What responsibility belongs to the composition layer, and what remains owned by the runtime, host, or repository?  
**Coverage:** Clarify the boundary among FSM_API execution, FSM_COS composition, host lifecycle/scheduling, and repository artifact storage/resolution.  
**Proof needed:** A small end-to-end scenario with observable boundaries and tests for each handoff.

### 3.2 From a Bundle Manifest to a Running Capability
**Question:** How does a declared bundle become something a host can resolve and execute?  
**Coverage:** Manifest intent, dependency resolution, immutable content identity, composition, validation, and failure reporting.  
**Proof needed:** A working local-provider path first; distinguish implemented local behavior from future Azure-backed behavior.

### 3.3 The Cost of Composition Versus the Cost of Running
**Question:** How much time is spent assembling a workload compared with executing it after assembly?  
**Coverage:** Use separate composition and steady-state metrics; explain why one blended number can mislead.  
**Proof needed:** Repeatable benchmark settings, multiple trials, preserved output, and a clear baseline.

### 3.4 From One Demonstration to an AnyApp
**Question:** What is the smallest real example that proves reusable capabilities can be composed into an application?  
**Coverage:** Build from a narrow, inspectable scenario toward AnyApp/WebApp rather than claiming every layer is already integrated.  
**Proof needed:** A runnable composition example and tests proving the boundaries it crosses.

### 3.5 The Readiness Number Is a Map, Not a Trophy
**Question:** What does “ready” mean for a framework that spans packages, manifests, providers, tests, and demos?  
**Coverage:** Readiness criteria, blocked acceptance paths, reproducibility, release gates, and how the percentage changes when an audit discovers missing proof.  
**Proof needed:** A versioned checklist tied to artifacts and passing checks; no unsupported readiness percentage.

### 3.6 Measure the Front Loader One Phase at a Time
**Question:** What does FSM_COS actually cost before a host receives its RuntimeAssembly?  
**Coverage:** Build the benchmark matrix around distinct phases—manifest validation, dependency closure, catalog lookup, loading, arbitration, assembly, failure paths, and repeated composition—then separate cold/setup costs from reuse and steady-state host execution.  
**Proof needed:** Stable unique benchmark identities, validated fixtures, named workload sizes, raw BenchmarkDotNet artifacts, source/runtime identity, and repeated runs. The benchmark repository's initial scaffold is not itself performance evidence.

## 4. CiAi: CI exposed to AI

### 4.1 CiAi Does Not Mean AI-Powered CI
**Question:** What changes when continuous integration becomes observable and operable through a deliberate interface for AI?  
**Coverage:** Explain the branding and purpose: expose CI state, results, and permitted operations without claiming the build itself is powered by AI.  
**Proof needed:** A real query against a repository workflow with linked run evidence.

### 4.2 From “The Build Is Green” to Evidence You Can Inspect
**Question:** What should an AI report when a workflow succeeds, fails, is skipped, or has not finished?  
**Coverage:** Status normalization, job-level results, links to source runs, timestamps, and truthful uncertainty.  
**Proof needed:** Examples covering success, failure, cancellation, skipped jobs, and in-progress runs.

### 4.3 A Cross-Repository Release Is a Coordination Problem
**Question:** How can multiple dependent repositories coordinate release readiness without silently publishing or outrunning dependencies?  
**Coverage:** Dependency order, dispatch/results, release evidence, explicit approval gates, and recoverable failures.  
**Proof needed:** A test workflow across repositories; package publication must remain opt-in and approval-gated.

### 4.4 The AI May Request; the System Must Authorize
**Question:** How do you let an AI inspect and initiate permitted work without giving it unlimited authority?  
**Coverage:** Read operations versus state-changing operations, permission boundaries, approval, audit trails, and least privilege.  
**Proof needed:** Tests showing forbidden actions are rejected and authorized actions leave an audit trail.

### 4.5 Can the Coordinator Explain Its Own Failure?
**Question:** Can a coordinator turn a failing CI run into a useful, verifiable explanation rather than a vague summary?  
**Coverage:** Preserve source evidence, identify failed stage, distinguish root cause from correlation, and suggest the next diagnostic step.  
**Proof needed:** Case studies tied to real workflow logs and reproducible failures.

## 5. ProtocolAi, GrammarAi, and HeadlessAi: contracts and transport

### 5.1 Give AI a Language Software Can Own
**Question:** Why should machine-facing vocabulary and contracts be explicit instead of relying on loose strings?  
**Coverage:** ProtocolAi's mapping of a finite vocabulary to integer identities, schema evolution, validation, and stable boundaries.  
**Proof needed:** Round-trip/compatibility tests and examples of invalid input being rejected.

### 5.2 Headless Does Not Mean Provider-Locked
**Question:** How can a headless core remain useful without hard-coding one provider or transport?  
**Coverage:** Provider-neutral core, optional adapters, dependency direction, and the difference between a core capability and an integration package.  
**Proof needed:** Core tests that run without optional provider dependencies and separate adapter tests.

### 5.3 The Contract Is the Product Boundary
**Question:** What does a system gain when the interface between components is more stable than their implementations?  
**Coverage:** Show how explicit contracts let independently evolving components cooperate while keeping responsibilities clear.  
**Proof needed:** A real consumer and producer using the same contract, including a compatibility or failure case.

## 6. GUI.SVG, Renderer, and Visualize: making behavior observable

### 6.1 When an SVG Becomes a Window into a Running System
**Question:** What changes when a visual document consumes authoritative application state instead of inventing its own animation?  
**Coverage:** A living view that observes data changed by a console app or host; distinguish presentation time from simulation time.  
**Proof needed:** An actual data/trace feed with tests showing the rendered state follows the source of truth.

### 6.2 Semantic Model to SVG Manifestation
**Question:** Can one semantic model produce a useful visual artifact without hand-authoring every picture?  
**Coverage:** Source/semantic model, manifest, renderer-neutral representation, SVG output, and provenance.  
**Proof needed:** Generated artifacts linked to their input model and a drift check.

### 6.3 Renderer for Text, Visualize for the World
**Question:** What does an AI need from a textual representation that a human can see immediately in a visual one?  
**Coverage:** Explore complementary views: Renderer as a textual portrayal and Visualize as a visual world, with shared underlying semantics rather than duplicated truth.  
**Proof needed:** A common input model and a concrete example demonstrating both representations agree.

### 6.4 Can the Diagram Be Regenerated from the Code?
**Question:** How can class diagrams stay connected to implementation without pretending source structure explains user intent?  
**Coverage:** Reflection/AST inputs, normalized model, explicit relationships, source spans and provenance, and human-authored use cases.  
**Proof needed:** A generated UML example with clear target selection and a drift check.

### 6.5 GPU Abstraction Without GPU Marketing
**Question:** What would a real Vulkan/WebGPU abstraction need to preserve across rendering backends?  
**Coverage:** Identify renderer-neutral concepts, capability boundaries, resource lifetime, and backend-specific constraints.  
**Proof needed:** A small cross-backend proof before making portability claims; clearly label exploratory work.

## 7. The Workshop as a connected body of work

### 7.1 From a State Machine to a Composable Application
**Question:** How do the Workshop's layers cooperate in a real use case, and where are the seams?  
**Coverage:** Trace one capability from runtime behavior through composition and presentation, adding CiAi or repository infrastructure only where the example truly uses it.  
**Proof needed:** An end-to-end diagram backed by runnable integration tests, with unimplemented links marked honestly.

### 7.2 A Living Documentation System
**Question:** Can documentation demonstrate real behavior and reveal when the implementation changes?  
**Coverage:** Gallery pages, executable recipes, generated diagrams, SVG examples, CI drift checks, and source links.  
**Proof needed:** A gallery entry generated or verified from the actual example, with a deliberate drift test.

### 7.3 The Cost of Trustworthy Automation
**Question:** What engineering work turns automation from a clever demo into something safe to rely on?  
**Coverage:** Repeatability, explicit permissions, immutable artifacts, provenance, tests at failure boundaries, and human approval for consequential actions.  
**Proof needed:** A concrete workflow with failure injection and auditable results.

### 7.4 The Workshop's Architecture, Told Through One User Journey
**Question:** Can a reader follow one request from an application host through composition, execution, observation, and CI evidence?  
**Coverage:** A narrative map of the larger domain, but only through working paths. This is a later synthesis article, not a substitute for proving each layer.  
**Proof needed:** One complete path, source links, and a table separating demonstrated, in-progress, and planned capabilities.

## 8. FSM_REST and FSM_Serialization: capability recipes and byte boundaries

### 8.1 An API Description Is Not an API Call
**Question:** What is the difference between describing a remote capability, binding one request, transporting it, and interpreting the response?  
**Coverage:** FSM_REST's operation descriptors, bindings, request construction, transport abstraction, and runtime response. Follow one small example from reusable recipe to response without treating a descriptor as fetched data.  
**Proof needed:** Contract tests for request construction and transport behavior; label any provider-specific importer or GUI integration as separate until verified.

### 8.2 A Serializer Should Not Invent Domain Meaning
**Question:** Which representation rules belong to a domain type, and which belong to a byte-oriented serialization boundary?  
**Coverage:** FSM_Serialization's explicit binary/stream contracts, supported ownership rules, and limits versus general object-graph serialization.  
**Proof needed:** Round-trip, malformed-input, truncation, boundary, and compatibility tests against the current source and package versions.

### 8.3 Benchmarking the Boundary Without Mixing the Lanes
**Question:** How do we compare encoding and buffer strategies without accidentally comparing different code or different work?  
**Coverage:** The benchmark repository's stable SER identities, published-package baseline versus development-source lane, encode/decode separation, buffer ownership, allocations, and retained artifacts.  
**Proof needed:** Green, reproducible runs from both lanes, source/package identity attached to each result, and comparable workloads before naming a winner.

## 9. CiAi's five Cogs: from isolated contracts to a working coordinator

### 9.1 Five Small Cogs Are Not Yet One Machine
**Question:** What becomes possible when capability catalog, request book, result correlation, policy gate, and lifecycle projection are composed—and what is still missing?  
**Coverage:** CogA declares version-pinned capabilities; CogB creates bounded, correlated, idempotent requests; CogC correlates provider results; CogD applies exact-match default-deny policy; CogE projects lifecycle evidence and contradictions. Their README files currently describe isolated, offline contract slices, with cross-Cog and parent-coordinator integration still outstanding.  
**Proof needed:** A tested end-to-end flow with shared contracts, durable identity/provenance, and negative cases; do not describe the isolated cogs as a complete remote execution system.

### 9.2 Permission Is Not Proof, and a Hash Is Not Acceptance
**Question:** Why must authorization, execution outcome, artifact identity, and semantic acceptance remain separate facts?  
**Coverage:** Use CogB/C/D/E boundaries to show that a permitted request does not prove execution, matching metadata does not prove semantic correctness, and an acknowledgement does not prove reclamation.  
**Proof needed:** Tests that inject conflicting, missing, out-of-order, cancelled, and unknown evidence and verify none is silently upgraded to success.

### 9.3 The Observer Must Report the Candidate It Actually Saw
**Question:** Can a read-only CI observer report exact commit, workflow attempts, jobs, and steps without confusing a candidate with the default branch or a later run?  
**Coverage:** Inspect the CiAi release-candidate observer workflow and FSM_COS pilot contract. Keep observation separate from runtime dependency and publication authority.  
**Proof needed:** A real run for a full commit SHA, links to the source workflow evidence, and demonstrated handling of missing, failed, skipped, and in-progress jobs.

## 10. StoryBuilder, Experiences, AnyApp, and WebPage: an Experience across hosts

### 10.1 A Story Has a Build Record and a Fictional Canon
**Question:** How can a creative workbench keep implementation facts separate from the fictional world's accepted facts?  
**Coverage:** StoryBuilder's project identity, premise/manuscript fields, stable-ID timeline events, passage anchors, chronology, and local save/reopen. Distinguish the existing Core slice from the not-yet-implemented AnyApp launch/render integration and interactive authoring surface.  
**Proof needed:** Run the Core tests and local persistence example; show the actual gap register before claiming a usable desktop authoring experience.

### 10.2 An Experience Is Not Its Window
**Question:** What should remain stable when the same Experience gains browser, desktop, or future VR manifestations?  
**Coverage:** Experience identity, immutable artifact identity, session state, protocol envelopes, and explicit bridge contracts. Use the Experiences repository's architecture to frame the boundary, not as proof that every bridge or client exists.  
**Proof needed:** A runnable cross-manifestation path with versioned messages, identity checks, and tested failure handling.

### 10.3 Local-First Is a Testable Deployment Claim
**Question:** Can a creator author and reopen a project without a hosted service being available?  
**Coverage:** StoryBuilder's initial local-first target, desktop-host direction, and the difference between a deployment decision and implemented integration.  
**Proof needed:** An offline save/reopen test through the intended host—not merely a Core persistence unit test.

### 10.4 The Landing Page Is a Demonstration, Not an Architecture Diagram
**Question:** Which effects on the current WebPage are implemented today, and which are still a destination?  
**Coverage:** Inspect the actual branch and runtime before repeating the README's evolving visual plan. Reconcile current renderer, GUI, physics, and host code with older descriptions that may no longer match the chosen implementation route.  
**Proof needed:** A current, reproducible walkthrough with a feature-by-feature implementation map and no claims based solely on architectural diagrams.

## 11. MicroBundleDomain, MicroBundleRepository, and FSM_COS: artifact identity end to end

### 11.1 What Exactly Is a MicroBundle?
**Question:** Which identity, dependency, configuration, and lifecycle information must a capability expose—and what remains private to its domain?  
**Coverage:** MicroBundleDomain's neutral contract, using real producer and consumer code to distinguish a domain model from composition policy.  
**Proof needed:** Contract tests and a consumer that builds against the published or explicitly pinned source contract.

### 11.2 A Repository Is Not the Composer
**Question:** How should immutable artifact retrieval stay separate from dependency resolution and runtime assembly?  
**Coverage:** MicroBundleRepository's repository/cache/provider boundary versus FSM_COS's resolution, validation, ordering, arbitration, and assembly responsibilities. The README describes Azure Blob as the intended remote substrate; verify the current implementation before claiming that remote path works.  
**Proof needed:** Exercise the local artifact path, integrity/identity validation, missing-artifact behavior, and only then any remote provider.

### 11.3 From Manifest to Artifact to RuntimeAssembly
**Question:** Can we follow one requested capability through manifest intent, repository retrieval, dependency closure, composition, and host handoff?  
**Coverage:** Tie together only the concrete handoffs that exist; make absent links visible.  
**Proof needed:** One runnable vertical slice with exact artifact identities, testable failures, and separately measured retrieval, composition, and execution costs.

## 12. GUI, Renderer, Visualize, Physics, and Gestures: meaning, perception, and cost

### 12.1 One Semantic GUI, Several Honest Manifestations
**Question:** Which meaning belongs in GUI.Core, and which behavior belongs to Blazor, WPF, or SVG adapters?  
**Coverage:** Use the recursive GuiNode model and adapter boundaries. GUI's current status document says SVG is early, some semantic contracts are not yet stable, WPF still lacks a dedicated test project, and animated docs are illustrations rather than renderer output.  
**Proof needed:** Adapter-by-adapter test evidence and explicit unsupported semantics; never imply feature parity because adapters share a model.

### 12.2 Render Less by Understanding the Observer
**Question:** Can Event Horizons reduce computational work by changing what representation exists, rather than merely lowering polygon count?  
**Coverage:** Renderer’s hypothesis about observer-relative computational detail; separate conceptual architecture from measured implementation.  
**Proof needed:** A bounded benchmark or demo that measures update/render work at each horizon, with a clear baseline and observable correctness.

### 12.3 A Scene Is More Than an Image
**Question:** How should authored environment, physical meaning, rendering, and optional sensory capabilities remain separate but composable?  
**Coverage:** Visualize's environment/scene direction alongside Physics and Gestures contracts. The current Visualize README describes architecture direction, not production volumetric rendering, a selected GPU backend, or working audio/haptic integration.  
**Proof needed:** A small vertical proof with real inputs and outputs; explicitly label any planned backend or modality.

### 12.4 When Does a Picture Become Evidence?
**Question:** How can a visualization be regenerated from authoritative state without drifting into an independent hand-authored simulation?  
**Coverage:** Connect SimpleDemos traces, GUI.SVG output, Renderer textual portrayal, and Visualize scene output only where a common data contract actually exists.  
**Proof needed:** Shared input identity, a reproducible generation command, and a drift/failure test.

## 13. DataWarehouse and Foundry: earn the architecture with a first vertical slice

### 13.1 What Does the Data Warehouse Actually Guarantee?
**Question:** What are the first observable data contracts, ownership rules, and failure semantics of the warehouse?  
**Coverage:** Start from the current repository skeleton and benchmark-repository skeleton. Treat names, diagrams, and initial README files as scaffolding, not implemented storage behavior.  
**Proof needed:** A defined minimal data model, executable write/read or ingest/query path, tests, and retained benchmark output before making performance or durability claims.

### 13.2 Benchmarking Before There Is a Benchmarkable Contract
**Question:** What should a benchmark suite refuse to measure until workload semantics are stable?  
**Coverage:** Define stable benchmark identities, datasets, correctness gates, and output provenance before comparing data strategies.  
**Proof needed:** A runnable baseline with validated fixtures, named scenarios, and raw output tied to an exact source revision.

### 13.3 Foundry: What Is the Smallest Useful Artifact Pipeline?
**Question:** Which build, validation, identity, and publication steps must an artifact pipeline prove before it can be trusted?  
**Coverage:** Investigate the new repository's intended responsibility against concrete upstream/downstream consumers; do not infer a complete pipeline from an initial commit.  
**Proof needed:** One bounded artifact from input through validation to a verifiable output, with failure cases and explicit publication authorization.

## Repository reality check — reviewed 2026-10-26

The first-pass inventory checked latest commits and sampled README, status, and workflow content across 2026-active repositories plus FSM_API. The latest commit is a navigation hint, not proof of a feature: several repositories' newest commits are shared-documentation updates, while some new repositories still contain only initial scaffolding. Recheck source, tests, workflows, and branch-specific status before every story.

Repository map for story discovery:

- **Runtime and composition:** FSM_API, FSM_COS, FSM_REST, FSM_Serialization, MicroBundleDomain, and MicroBundleRepository.
- **AI-facing contracts and CI:** ProtocolAi, GrammarAi, HeadlessAi, CiAi, and CiAi.CogA–CogE.
- **Experiences and hosts:** AnyApp, Experiences, StoryBuilder, WebPage, WebApp, and MyVR.
- **Semantic UI, perception, and simulation:** GUI, Renderer, Visualize, Physics, and Gestures.
- **Performance evidence:** FSM_COS_Benchmarks, FSM_Serialization_Benchmarks, MicroBundleDomain_Benchmarks, and Renderer.Benchmarks.
- **Early data/artifact work:** DataWarehouse, Benchmarks.DataWarehouse, and Foundry.
- **Editorial and runnable examples:** SimpleDemos, plus FSM_API as the explicit older-repository exception.

Current editorial guardrails from that review:

- **CiAi:** the coordinator README is only a heading; its current observer workflow is a manual, read-only release-candidate observation slice. CogA–CogE have useful isolated contracts, but their own READMEs identify parent integration, durable storage, authenticated dispatch, and end-to-end orchestration as outstanding.
- **StoryBuilder:** the Core model and local save/reopen slice exist; AnyApp launch/render integration and the interactive authoring surface are explicitly not implemented yet.
- **FSM_Serialization_Benchmarks:** the published-package and development-source lanes are intentionally distinct. The latest README reports the development-source benchmark correction still needs a green CI run.
- **GUI:** semantic Core and adapters exist at different maturity levels; GUI.SVG remains early and must not be presented as a proven, feature-complete renderer.
- **Visualize / Renderer / Physics:** current docs include substantial architectural direction; claims about production rendering, GPU backend selection, and sensory integration require concrete executable evidence.
- **DataWarehouse / its benchmark repository / Foundry:** latest commits are initial scaffolding or README-only changes; no mature behavior should be presumed.
- **FSM_API:** retain it as the explicit historical exception and keep its 2026 integer-backed V2 work and benchmark evidence separate from older published-package behavior.

## 14. Suggested investigation order

This is a provisional order based on evidence dependencies, not a promise of publication dates:

1. **SimpleDemos evidence contract:** identify exactly what the existing C# demo runs and what it does not prove.
2. **Deterministic trace:** implement fixed ticks, structured records, and assertions.
3. **Honest performance comparison:** separate FSM execution, domain work, setup/composition, and steady-state costs.
4. **FSM_COS composition boundary:** demonstrate one real composed workload with measurable handoffs.
5. **CiAi observability:** report an actual workflow state and link directly to evidence.
6. **Living SVG:** render authoritative trace/state data, rather than an independent browser simulation.
7. **Cross-layer story:** only after the individual boundaries have passed their own tests.

## Notebook status vocabulary

- **Idea:** potentially interesting; not yet investigated.
- **Investigating:** source inspection or experiment underway.
- **Needs proof:** the premise is interesting but the evidence is not yet sufficient.
- **Demonstrated:** runnable example and relevant tests exist.
- **Published:** article exists and is linked from the published index.

Nothing in this notebook should be treated as a claim that every proposed capability is implemented. Update status only when code, tests, or a published article justify it.
