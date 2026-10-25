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
- completed payload-operation count;
- allocated bytes and managed-memory observations;
- failures, skipped ticks, and actual number of worker updates.

Do not treat FPS alone as the result. The number of completed worker updates and payload operations is the equivalence check.

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
- It times each frame separately and then scans every worker handle to aggregate counters; aggregation cost is inside the measured frame.
- It records a managed-memory delta, which is not equivalent to total allocation volume and can be affected by GC timing.
- Its worker callback converts an integer to a string, so the workload includes an allocation-heavy operation. Keep this in both paths initially; add a separately named allocation-free payload only as a second experiment.
- The worker setup path should be audited for accidental manager-instance creation and cleanup before treating setup cost as representative.

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
