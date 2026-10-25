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

## 8. Suggested investigation order

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
