# FSM_COS Stress Benchmark: Measurement Contract

This document defines the first apples-to-apples performance experiment for the Workshop ecosystem. It is a measurement contract, not a claim that FSM_COS is faster or slower.

## Question

For the same synthetic worker workload, what is the difference between:

1. direct FSM_API setup and steady-state updates;
2. preparing/composing the workload through FSM_COS, then running the same host-owned updates?

Composition is a one-time cost. It must be reported separately from steady-state tick cost.

## Workload and control

Use the existing `Stress/Stress.cs` payload as the starting point. Keep its arithmetic, branch, update interval, worker count, and state-transition semantics identical between paths. Preserve the current baseline until the new harness has recorded equivalent work and output.

Each run must record:
- path identifier (`FSM_API` or `FSM_COS`);
- runtime and framework version, OS, CPU, build configuration, and commit;
- worker count and tick interval;
- setup and composition elapsed time;
- warm-up count and measured tick count;
- elapsed tick time and throughput;
- completed worker updates per measured tick and expected worker-update count;
- completed payload-operation count per measured tick and expected payload-operation count;
- allocated bytes per measured tick and managed-memory observations where collected;
- failures or skipped work (the current runner fails closed when worker-update or payload-operation counts differ from the expected workload).

Do not treat FPS alone as the result. The number of completed worker updates and payload operations is the equivalence check.

The current `Stress/FSM_COS` runner records raw per-trial samples, runtime/OS/logical-processor/configuration/commit metadata, worker updates, payload operations, and allocation observations. Before writing the CSV it asserts the expected worker-update and payload-operation counts for every direct and composed trial. The CI artifact is raw evidence for that exact run; a passing equivalence gate does not establish that FSM_COS is faster or that it reproduces the fox-and-dog demonstration.

## Timing boundaries

Measure these phases independently:

| Phase | Included | Excluded |
|---|---|---|
| Construction | Definitions, worker/context creation, registration | Measured steady-state ticks |
| FSM_COS composition | Manifest/catalog resolution, load, arbitration, assembly construction | Repeated tick loop |
| Warm-up | Explicit unmeasured updates before sampling | Reported sample statistics |
| Steady state | Host tick call and the same workload execution | Per-frame reporting/CSV formatting |
| Reporting | Aggregation and file output | Runtime timing interval |

If the host has to perform work that is also required in the direct baseline, keep that work in both paths. Do not charge only one implementation for metrics collection, logging, or rendering.

## Measurement protocol

1. Run Release builds with the same .NET runtime on the same machine.
2. Use a deterministic configuration matrix, beginning with small counts that validate equivalence before increasing load.
3. Warm up each path and workload configuration before collecting samples.
4. Alternate or randomize path order across repeated trials to reduce thermal/order bias.
5. Collect multiple independent samples; report median and spread, not only the fastest run.
6. Keep raw samples in CSV. Derive summary tables from the raw file.
7. Verify equal completed work before comparing time. A faster run that skipped work is not a win.
8. Repeat the same protocol for the dynamic fox/dog Gauntlet only after the worker benchmark is stable.

## Baseline caveats to resolve

The current `Stress/Program.cs` is a useful historical starting point, but its existing output is not yet a strict comparative benchmark:

- It uses one 60-frame batch per configuration, so there is no repeated-sample distribution.
- The development baseline now stops the timer immediately after `FSM_API.Interaction.Update("Stress")` and aggregates worker counters outside the timed interval. Older CSVs were measured with aggregation inside the timer and must remain labeled as historical.
- It records a managed-memory delta, which is not equivalent to total allocation volume and can be affected by GC timing.
- Its worker callback converts an integer to a string, so the workload includes an allocation-heavy operation. Keep this in both paths initially; add a separately named allocation-free payload only as a second experiment.
- The development baseline now creates a manager FSM instance only for the top-level context. Worker contexts bind directly to their worker FSM, avoiding an untracked manager instance per worker. Setup and teardown still need automated validation before setup timings are treated as final.

Do not silently change these behaviors and then compare new results with old numbers. Version the protocol and label historical measurements.

## MicroBundle and host boundary

The stress workload is the MicroBundle. FSM_COS resolves and prepares it into a `RuntimeAssembly`. The host owns the scheduling/tick loop, lifecycle, instrumentation, and output. The repository stores and retrieves artifact bytes; it does not perform composition or execute the loop.

For the first proof, an in-memory catalog is acceptable if the bundle uses the current MicroBundleDomain contracts and the local host can reproduce the run. Filesystem artifact storage and network serving are separate increments, not prerequisites for measuring composition overhead.

## Evidence ladder

1. **Stress baseline:** reproduce direct FSM_API work and preserve historical output.
2. **Equivalent MicroBundle:** prove the FSM_COS-composed workload performs the same work.
3. **Separated timing:** report setup/composition separately from steady-state updates.
4. **Repeatability:** automated equivalence checks, repeated raw samples, environment metadata.
5. **Dynamic Gauntlet:** apply the same measurement discipline to fox/dog stress.
6. **Simple fox/dog:** first explanatory FSM_API demo, then the equivalent FSM_COS route.
7. **Advanced 2D fox/dog:** preserve the same observable behavior contract while expanding the world and agent layout.

The goal is evidence that lets us quantify the overhead for a specific use case on a specified machine—not a universal performance claim.


## Next experiment: determine what the apparent 21% means

The first seven-trial hosted run is a **hypothesis generator**. Its reported steady-state medians were 0.1808 ms/tick for direct FSM_API and 0.1436 ms/tick for FSM_COS composition, approximately 20.6% lower median tick time for the composed path in that run. This is not yet a reliable estimate of the true effect: the run had wide ranges and a large composition-startup outlier.

The next question is not simply “can we reproduce 21%?” It is: **where does the difference come from, under what workload, and does it survive the one-time composition cost?**

### Separate the hypotheses

1. **Steady-state effect:** paired per-trial tick-time ratio for identical completed work, with alternating order. Report every pair and the median paired ratio; do not infer a paired effect by comparing two unrelated aggregate medians alone.
2. **Startup cost:** compare full setup/composition time separately. Keep bundle load and the remaining composition path visible.
3. **Amortization / break-even:** for each paired trial where FSM_COS ticks faster, estimate the tick count needed to recover extra composition time:

   `break_even_ticks = max(0, composed_setup_ms - direct_setup_ms) / (direct_mean_tick_ms - composed_mean_tick_ms)`

   If the composed tick is not faster, report “no steady-state break-even” for that pair. This is a model derived from measured means, not a guarantee; show the input values and never hide negative or unstable cases behind a single headline.
4. **Allocation and GC:** retain allocated bytes per tick, but label it as current-thread allocation observed around the tick call. It is not total process allocation and does not by itself prove lower GC pressure.
5. **Workload scaling:** after the current 1,000-worker comparison is repeatable, add a separate named scale experiment (for example 100, 500, 1,000, and 2,500 workers) and keep all workload semantics and equivalence assertions identical. Do not mix scale results with the historical 1,000-worker series.

### Evidence required before describing a speedup

- At least three independent hosted workflow runs, each preserving its raw CSV and commit/environment metadata.
- Equal worker updates and payload operations for every direct/composed pair.
- Paired ratios and medians, plus spread/range; retain all outliers rather than deleting inconvenient samples.
- No regression hidden by combining setup and steady-state time. Report both steady-state and a few explicit lifetimes (e.g. 100, 1,000, and 10,000 ticks) including startup.
- A follow-up local run on a stable, otherwise-idle machine before treating a CI-hosted result as a performance conclusion.

If repeated runs do not reproduce the apparent advantage, that is a useful result: it means the initial 21% was likely noise or environment-sensitive. If the advantage persists, scales consistently, and survives startup amortization, we can narrow the claim to the tested runtime, machine, workload, and lifecycle. A real result can be smaller than 21% and still be valuable.

### Current evidence status

- Behavior-equivalence proof gates: **3/3 passed (100% of these specific gates)** in the recorded SimpleDemos validation.
- Performance conclusion: **exploratory; not yet validated across independent workflow runs**.
- Publication: no performance claim should be presented as a universal speedup, and no package publication is implied by these experiments.


## Evidence review: first seven-trial run and follow-up smoke capture

**Review updated:** 2026-10-25  
**Purpose:** record what the numbers actually say, including the evidence that weakens the original apparent-speedup story. This is an engineering investigation, not a release claim.

### Executive reading

There are two captured seven-pair datasets. Both used 1,000 workers, a tick interval of 1, 20 warm-up ticks, and 200 measured ticks per path/trial. Both ran Release on .NET 8.0.32, Ubuntu 24.04.5 LTS, with 4 logical processors. Every direct/composed pair reported 1,000 worker updates and 3,000 payload operations per tick. The recorded current-thread allocation observation was effectively the same: 96,728 bytes/tick in nearly every row.

The first dataset's **ratio of path medians** suggests FSM_COS tick time was 20.66% lower (0.143573 ms vs 0.180824 ms). But this headline is misleading when read as a stable effect: its **median of paired trial improvements was only +2.24%**, the paired range was -6.76% to +40.47%, and the 7 pairs include a direct-path tick outlier at 0.241184 ms and a very large FSM_COS composition outlier at 24.632 ms.

The follow-up smoke capture reverses the headline: path medians were 0.215057 ms for FSM_COS and 0.213863 ms for FSM_API, while the **median paired improvement was -0.56%**; FSM_COS was faster in only 2 of 7 pairs. The paired range was -11.51% to +50.98%. It also contains a 35.2523 ms FSM_COS setup/composition result and a 3.1445 ms setup result. These are not numbers to smooth away; they show the experiment is highly sensitive to startup/JIT/runtime/host noise and that seven samples from a single hosted run are not enough.

**Current conclusion: we have proven equivalent work and matching observable demo behavior; we have not proven a repeatable FSM_COS steady-state speedup.** The most valuable performance result so far is a more precise experimental question, not a percentage to advertise.

### Capture identity and evidence links

| Capture | Workflow run | Commit | Artifact | How to interpret |
|---|---|---|---|---|
| A — original seven-trial capture | [40162369632](https://github.com/TrentBest/SimpleDemos/actions/runs/40162369632) | `82fa35031137f0d8687794c39651290b1a8a7288` | [artifact 12587570080](https://api.github.com/repos/TrentBest/SimpleDemos/actions/artifacts/12587570080/zip) | Original exploratory data; contains a 24.632 ms composition outlier |
| B — paired-analyzer workflow smoke capture | [40164113545](https://github.com/TrentBest/SimpleDemos/actions/runs/40164113545) | `c727eb8e99273c78fd3bf92d2b27dc815609f921` | [artifact 12588155073](https://api.github.com/repos/TrentBest/SimpleDemos/actions/artifacts/12588155073/zip) | Verifies the analyzer and artifact pipeline; timing rows are diagnostic, not an independent controlled replication |

Capture B is a new capture on a later commit and the workflow passed, but it is explicitly a smoke/validation run, not a pre-registered independent replication on a stable machine. Treating A and B as a formal combined sample would be inappropriate.

### Controlled workload and volume

Per path, per trial:

| Quantity | Value |
|---|---:|
| Workers | 1,000 |
| Warm-up ticks | 20 (excluded from measured tick statistics) |
| Measured ticks | 200 |
| Worker updates per measured tick | 1,000 |
| Payload operations per measured tick | 3,000 |
| Worker updates per trial | 200,000 |
| Payload operations per trial | 600,000 |
| Worker updates across 7 trials | 1,400,000 |
| Payload operations across 7 trials | 4,200,000 |
| Runtime | .NET 8.0.32 |
| OS | Ubuntu 24.04.5 LTS |
| Logical processors | 4 |
| Build | Release |

Those totals apply separately to FSM_API and FSM_COS in each capture: 1.4 million worker updates and 4.2 million payload operations per path. Across both paths, each seven-trial capture checks 2.8 million worker updates and 8.4 million payload operations. This is a useful work-equivalence gate, not a claim about application-scale throughput.

### Summary statistics

All tick figures below are milliseconds per tick. Setup figures are milliseconds. Throughput is the runner's recorded ticks/second.

| Metric | Capture A: FSM_API | Capture A: FSM_COS | Capture B: FSM_API | Capture B: FSM_COS |
|---|---:|---:|---:|---:|
| Median tick time | 0.180824 | 0.143573 | 0.213863 | 0.215057 |
| Mean tick time | 0.167957 | 0.149734 | 0.176789 | 0.170938 |
| Minimum tick time | 0.050687 | 0.048100 | 0.065917 | 0.066738 |
| Maximum tick time | 0.241184 | 0.212063 | 0.277082 | 0.308975 |
| Median ticks/second (approx. inverse of median tick) | 5,530 | 6,965 | 4,675 | 4,650 |
| Median setup/composition | 0.545500 | 0.547600 | 0.759100 | 0.806900 |
| Minimum setup/composition | 0.320500 | 0.358000 | 0.464600 | 0.516300 |
| Maximum setup/composition | 4.271500 | 24.632000 | 5.912700 | 35.252300 |
| Median bundle-load component | 0.000000 | 0.345400 | 0.000000 | 0.524100 |
| Maximum bundle-load component | 0.000000 | 3.821500 | 0.000000 | 3.049000 |
| Median composition residual | 0.000000 | 0.070900 | 0.000000 | 0.084300 |
| Maximum composition residual | 0.000000 | 24.288900 | 0.000000 | 34.728200 |
| Median observed allocated bytes/tick | 96,728 | 96,728 | 96,728 | 96,728 |
| Allocation min–max | 96,728–96,731 | 96,728–96,728 | 96,728–96,731 | 96,728–96,728 |

**Important distinction:** the median of individual recorded throughput values is not mathematically guaranteed to equal the reciprocal of median tick time. The table labels the throughput row as an approximate inverse to avoid implying they are identical statistics; use the raw CSV for exact throughput summaries.

### Every paired tick result

Improvement is calculated per trial as `(FSM_API tick − FSM_COS tick) / FSM_API tick × 100`. Positive means FSM_COS ticked faster for that pair; negative means slower. Do not delete the outliers.

| Trial | A direct ms | A FSM_COS ms | A paired improvement | B direct ms | B FSM_COS ms | B paired improvement |
|---:|---:|---:|---:|---:|---:|---:|
| 1 | 0.180824 | 0.143405 | +20.69% | 0.261898 | 0.251077 | +4.13% |
| 2 | 0.144522 | 0.143353 | +0.81% | 0.215980 | 0.216629 | -0.30% |
| 3 | 0.141103 | 0.150641 | -6.76% | 0.213863 | 0.215057 | -0.56% |
| 4 | 0.241184 | 0.143573 | +40.47% | 0.277082 | 0.308975 | -11.51% |
| 5 | 0.216932 | 0.212063 | +2.24% | 0.136157 | 0.066738 | +50.98% |
| 6 | 0.200444 | 0.207000 | -3.27% | 0.065917 | 0.068521 | -3.95% |
| 7 | 0.050687 | 0.048100 | +5.10% | 0.066629 | 0.069572 | -4.42% |
| **Median paired improvement** | | **+2.24%** | | | **-0.56%** | |
| **Range** | | **-6.76% to +40.47%** | | | **-11.51% to +50.98%** | |
| **FSM_COS faster pairs** | | **5 / 7** | | | **2 / 7** | |

The first run's +20.66% headline comes from comparing the two path medians, not from the median paired effect. Pairing each trial changes the story materially. The second capture demonstrates why we should not headline a ratio of aggregate medians: its paired result is slightly negative and highly dispersed.

### Setup and composition cost: visible numbers, not one blended score

FSM_COS setup/composition includes bundle load plus a residual composition path. Direct FSM_API has no corresponding bundle-load or composition residual in this harness, so those cells are zero by construction; that does **not** mean direct setup is free.

| Trial | A direct setup ms | A FSM_COS setup ms | A extra setup ms | B direct setup ms | B FSM_COS setup ms | B extra setup ms |
|---:|---:|---:|---:|---:|---:|---:|
| 1 | 4.271500 | 24.632000 | +20.360500 | 5.912700 | 35.252300 | +29.339600 |
| 2 | 0.767100 | 0.531600 | -0.235500 | 1.046800 | 0.881700 | -0.165100 |
| 3 | 0.320500 | 0.358000 | +0.037500 | 0.464600 | 0.536900 | +0.072300 |
| 4 | 0.545500 | 0.396200 | -0.149300 | 1.185800 | 0.516300 | -0.669500 |
| 5 | 0.587700 | 3.966600 | +3.378900 | 0.759100 | 3.144500 | +2.385400 |
| 6 | 0.533800 | 0.612200 | +0.078400 | 0.580400 | 0.669900 | +0.089500 |
| 7 | 0.516200 | 0.547600 | +0.031400 | 0.605300 | 0.806900 | +0.201600 |

These measurements show why setup must be reported independently. The composition cost varies from sub-millisecond to tens of milliseconds in these hosted captures. We do not yet know how much is JIT/cold-start, host contention, filesystem/runtime initialization, or intrinsic composition work. The current residual is a subtraction (`SetupOrCompositionMs - BundleLoadMs`), not a separately instrumented phase timer, so it must not be described as a pure composition measurement.

### Allocation observation

The recorded value was 96,728 bytes/tick in all but a few direct-path rows at 96,731 bytes/tick. The observed FSM_COS-minus-FSM_API delta was -3 bytes in trial 1 and 0 bytes in the other pairs in both captures. This is too small and too narrowly measured to conclude that either path allocates less in a meaningful end-to-end sense.

The field is a current-thread allocation observation around the tick call, not total process allocation, not bytes allocated over the entire lifecycle, and not a direct measurement of GC pause time or GC pressure. Keep the string-conversion payload identical in both paths. Add a separate allocation-light workload later rather than silently changing this one.

### What we know, what we do not know

**Known with evidence**
- The comparison harness checks equal worker count, worker updates, payload operations, and measured ticks for every direct/composed pair; both captures satisfy these checks.
- Both captures use 1,000 workers × 200 measured ticks, with 1,000 worker updates and 3,000 payload operations per tick.
- The direct and FSM_COS-composed basic fox/dog demos produced 11 identical observable frames in the recorded behavior gate.
- The direct and FSM_COS-composed advanced 2D fox/dog demos produced 30 identical observable frames in the recorded behavior gate.
- The combined behavior-equivalence gates passed 3/3 in the recorded validation. This is 100% of those three gates, not overall product readiness.
- The workflow run [40164113545](https://github.com/TrentBest/SimpleDemos/actions/runs/40164113545) passed, including build, behavior trace comparisons, comparison smoke run, paired analysis, and artifact upload.

**Not known yet**
- Whether FSM_COS is reliably faster, slower, or statistically indistinguishable in steady state for this workload.
- Whether the extreme timings are JIT/cold-start, CPU scheduling/host contention, GC, filesystem effects, or another cause. Current logs do not isolate them.
- Whether the apparent tick-time difference generalizes across worker counts, runtime versions, operating systems, machines, or real applications.
- Whether setup/composition cost is stable enough to estimate break-even for a real application lifetime.
- Whether current-thread allocation observations correlate with total allocation or GC behavior.
- Whether the same effect exists in the dynamic fox/dog Gauntlet, advanced 2D simulation, or a workload without string conversion.

### What we are doing next (in order)

1. **Preserve raw evidence.** Keep both original CSVs and the generated paired CSV/report tied to workflow run IDs and commit SHAs. Never overwrite raw rows with summaries.
2. **Instrument startup phases.** Add explicit phase timers for definition/build, bundle retrieval/load, catalog resolution, arbitration, assembly construction, and first tick; separate cold and warm execution. Avoid inferring phase costs solely by subtracting two timers.
3. **Control order and warm-up.** Alternate or randomize direct/composed execution order per trial; run multiple warm-up windows; document process reuse and whether each trial shares a process. Keep Release configuration and runtime pinned.
4. **Add robust summaries.** Report paired ratios, median, median absolute deviation (MAD), quartiles/IQR, min/max, faster/slower counts, and confidence intervals from a predeclared bootstrap or equivalent method. Preserve all outliers and label them.
5. **Repeat across independent hosted runs.** Target at least 3 independent workflow runs on the same workload/protocol before drawing a provisional conclusion; 5 or more would provide a better view of run-to-run variance. The current A/B captures are useful diagnostics, not sufficient controlled replications.
6. **Measure lifetime economics.** For each path, report setup plus N steady-state ticks for N = 1, 100, 1,000, and 10,000, with the same useful work completed. Include first-tick and warm steady-state values. Only calculate break-even when measured steady-state savings are positive and report the inputs and uncertainty.
7. **Scale separately.** After repeatability at 1,000 workers, add a separate matrix such as 100, 500, 1,000, and 2,500 workers. Do not mix scale changes with this baseline series.
8. **Validate locally.** Repeat on an otherwise-idle stable machine, recording CPU, OS, runtime, power profile if known, and exact commit. CI results remain valuable for regression detection but are not a substitute for controlled local benchmarking.
9. **Run the same discipline on real demonstrations.** Once the worker benchmark is stable, benchmark dynamic fox/dog Gauntlet and advanced 2D scenarios with trace-equivalence gates and explicit work counts.

### Decision rule for future claims

Until the next evidence gates pass, use language like: “In exploratory hosted runs, equivalent-work paired results varied from -11.51% to +50.98%; the median paired effect was +2.24% in one capture and -0.56% in a follow-up smoke capture. No repeatable speedup has been established.”

A provisional performance claim requires at least 3 independent, protocol-matched runs, paired results that are stable in direction and magnitude, setup and steady-state reported separately, explicit lifetime comparisons, raw CSVs retained, and local confirmation. A general claim requires broader machine/runtime/workload coverage. If the result is neutral, that is still valuable: FSM_COS composition may provide architectural value without imposing meaningful steady-state tick overhead, but that claim also needs appropriately narrow evidence.
