<!-- The Singularity Workshop moniker -->
# MultiThreadedStress — measuring FSM_API concurrency

This project is a bounded experiment for one question: **what happens when independent FSM_API processing groups are updated concurrently, rather than having one thread update a shared group?**

It is not yet proof that FSM_API is thread-safe or that every part of the API can be called concurrently. The current string-backed API stores its processing groups, buckets, instances, tick counts, and deferred modifications in shared static structures. Its source comments say the registry should be accessed from a single designated update thread. This experiment deliberately tests concurrent updates of already-created, independent groups to find out whether that boundary is real in practice.

## Run it

Requires the .NET 8 SDK and internet access on first package restore.

```powershell
dotnet run --project MultiThreadedStress/MultiThreadedStress.csproj --configuration Release
```

The default run is intentionally bounded: a worker count capped at eight, 2,000 updates per worker, three measured trials, and one warm-up. It writes `multithreaded_stress.csv` to the current working directory.

Tune a run explicitly:

```powershell
dotnet run --project MultiThreadedStress/MultiThreadedStress.csproj --configuration Release -- --workers 4 --iterations 10000 --trials 5
```

Use `--help` to see accepted bounds. The maximums protect against accidentally launching an unbounded machine-saturating experiment; increase them deliberately when testing a known machine.

## What is compared

- **SerialSharedGroup:** one host thread calls `FSM_API.Interaction.Update` repeatedly for one group containing all worker instances.
- **ConcurrentIndependentGroups:** each worker owns a separate processing group and instance, and worker tasks call `Update(group)` concurrently. All definitions and instances are created before timing; no structural changes are made during measured updates.
- **ParallelControlIndependentCounters:** a non-FSM control that runs the same payload in parallel against worker-local counters. This helps separate general task/payload cost from FSM_API update cost.
- **ParallelControlSharedCounter:** a non-FSM contention control in which all workers increment one shared `long` using `Interlocked.Increment`. This measures a deliberately synchronized shared-state workload; it is not an FSM_API test.

The FSM callbacks and independent control execute the same payload: increment a per-worker integer, convert it to a string, and perform a small branch. The shared-counter control isolates atomic contention and intentionally does not perform the string conversion. Keep those workload differences in mind when comparing timings.

Each measured trial records setup time separately from update time, elapsed time, callback throughput, process-wide allocated-byte delta, expected and observed callback counts, correctness, runtime, OS, logical processor count, and .NET version. Warm-up is excluded from the CSV. The summary reports medians; compare only on the same machine and runtime.

## Capture a repeatable GitHub-hosted run

For an initial set of comparable numbers without needing to run locally:

1. Open the repository's **Actions** tab.
2. Select **MultiThreadedStress Benchmark**.
3. Choose **Run workflow** on the `development` branch.
4. Start with the defaults (4 workers, 10,000 iterations per worker, 5 measured trials).
5. When complete, download the `multithreaded-stress-<run-id>` artifact. It contains the raw `multithreaded_stress.csv` and the console summary.

The workflow runs on GitHub-hosted `ubuntu-24.04`, so treat results as a baseline for that hosted environment, not as your PC's performance. Keep worker count, iteration count, runtime, and OS the same when comparing runs. The CI correctness smoke test is deliberately tiny and is **not** a performance benchmark. These first measurements help us discover bottlenecks and scaling shape; they are not yet statistically rigorous BenchmarkDotNet results, and shared-host load can affect timings.

## Reading the evidence correctly

1. **Correctness first.** Expected callback counts must match observed counts. A failed gate is a result, not a number to average away.
2. **Concurrency second.** Independent groups are the first probe. A successful run is evidence for that workload and environment only; it is not a blanket thread-safety guarantee.
3. **Performance third.** Throughput and allocations are observations, not portable promises. Repeat runs and retain the raw CSV.
4. **Keep API limitations visible.** FSM_API catches some callback exceptions internally. This runner's count gate detects missing or duplicated callback executions; it does not yet expose every internal API error counter. A passing count gate therefore does not prove the absence of all internal errors.
5. **No unsafe structural mutation.** This first runner does not create, destroy, or modify definitions/instances while update tasks are running. That is a separate stress axis and should not be mixed into the first result.

## Next experiments

- Run on different core counts and record the same CSV metadata.
- Strengthen failure capture with task/worker exception reporting and FSM_API internal-error observability where the public API permits it.
- Add a deliberate shared-group concurrent-update probe as a separately labelled, potentially unsafe experiment; do not confuse it with the independent-group path.
- Add lifecycle cleanup and repeated create/destroy scenarios after checking the API's supported lifecycle semantics.
- Compare the string-backed and integer-backed APIs when the integer path is ready, keeping work equivalence and timing boundaries identical.
- Add CI correctness smoke tests with small bounded inputs. CI timing is not a performance baseline.

No NuGet package is published by this project, and its default run does not attempt to saturate the host.

---

<p align="center">
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <strong>The Singularity Workshop</strong>
</p>
