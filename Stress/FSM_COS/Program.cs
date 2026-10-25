using System.Diagnostics;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_COS;
using TheSingularityWorkshop.MicroBundleDomain;

const int workerCount = 1_000;
const int tickInterval = 1;
const int warmupTicks = 20;
const int measuredTicks = 200;
const int trials = 3;
const string outputPath = "fsm_cos_stress_comparison.csv";

Console.WriteLine("SimpleDemos — direct FSM_API vs FSM_COS-composed stress MicroBundle");
Console.WriteLine($"Workers={workerCount}; interval={tickInterval}; warm-up={warmupTicks}; measured ticks={measuredTicks}; trials={trials}");
Console.WriteLine("Timing excludes per-worker metric aggregation and CSV output.");

var results = new List<BenchResult>();
for (var trial = 1; trial <= trials; trial++)
{
    // Alternate order to reduce consistent first-run/JIT bias.
    if (trial % 2 == 1)
    {
        results.Add(RunDirect(trial));
        results.Add(RunFsmCos(trial));
    }
    else
    {
        results.Add(RunFsmCos(trial));
        results.Add(RunDirect(trial));
    }
}

await File.WriteAllTextAsync(outputPath,
    "Path,Trial,Workers,TickInterval,WarmupTicks,MeasuredTicks,SetupOrCompositionMs,BundleLoadMs,KernelResidualMs,MeanTickMs,TicksPerSecond,PayloadOpsPerTick,AllocatedBytesPerTick\n" +
    string.Join("\n", results.Select(result => result.ToCsv())) + "\n");

Console.WriteLine();
Console.WriteLine("Path       Trial   Setup/compose ms   Mean tick ms   Ticks/sec   Payload ops/tick   Allocated bytes/tick");
foreach (var result in results)
{
    Console.WriteLine($"{result.Path,-10} {result.Trial,5} {result.SetupOrCompositionMs,17:F3} {result.MeanTickMs,14:F4} {result.TicksPerSecond,11:F1} {result.PayloadOpsPerTick,18:N0} {result.AllocatedBytesPerTick,22:N0}");
}
Console.WriteLine($"Raw results: {Path.GetFullPath(outputPath)}");
Console.WriteLine("Composition/setup and steady-state execution are separate columns; this is an initial local sample, not a universal benchmark claim.");

BenchResult RunDirect(int trial)
{
    var group = $"Stress.Direct.{trial}";
    Stress? workload = null;
    double setupMs;
    try
    {
        var setup = Stopwatch.StartNew();
        workload = new Stress(workerCount, tickInterval, group);
        workload.SpawnWorkers();
        setup.Stop();
        setupMs = setup.Elapsed.TotalMilliseconds;

        return Measure("FSM_API", trial, group, workload, setupMs, bundleLoadMs: 0, compositionMs: setupMs);
    }
    finally
    {
        if (workload is not null)
            Cleanup(workload, tickInterval, group);
    }
}

BenchResult RunFsmCos(int trial)
{
    var group = $"Stress.FSM_COS.{trial}";

    // Include bundle, catalog, and manifest preparation in the full composition/startup
    // measurement. Bundle.Load is timed separately so the residual is interpretable.
    var composition = Stopwatch.StartNew();
    var bundle = new StressMicroBundle(workerCount, tickInterval, group);
    var catalog = new StressCatalog(bundle);
    var manifest = new RuntimeManifest(
        RuntimeId: (ulong)(50_000 + trial),
        Bundles: [new MicroBundleManifestEntry(bundle.Id, "1.0.0")]);

    RuntimeAssembly assembly = new FsmCos(catalog).Execute(manifest);
    composition.Stop();

    if (!assembly.TryGetBundle<StressMicroBundle>(bundle.Id, out var loadedBundle) || loadedBundle?.Workload is null)
        throw new InvalidOperationException("FSM_COS composed the manifest but the stress MicroBundle was not retrievable.");

    try
    {
        return Measure(
            "FSM_COS",
            trial,
            group,
            loadedBundle.Workload,
            setupOrCompositionMs: composition.Elapsed.TotalMilliseconds,
            bundleLoadMs: loadedBundle.LoadMilliseconds,
            compositionMs: composition.Elapsed.TotalMilliseconds);
    }
    finally
    {
        if (loadedBundle.Workload is not null)
            Cleanup(loadedBundle.Workload, tickInterval, group);
    }
}

BenchResult Measure(
    string path,
    int trial,
    string group,
    Stress workload,
    double setupOrCompositionMs,
    double bundleLoadMs,
    double compositionMs)
{
    // Same settling update and warm-up policy for both paths.
    FSM_API.Interaction.Update(group);
    for (var i = 0; i < warmupTicks; i++)
        FSM_API.Interaction.Update(group);

    var elapsedTicks = 0L;
    long payloadOps = 0;
    long allocatedBytes = 0;

    for (var i = 0; i < measuredTicks; i++)
    {
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        FSM_API.Interaction.Update(group);
        var end = Stopwatch.GetTimestamp();
        var allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

        elapsedTicks += end - start;
        allocatedBytes += allocatedAfter - allocatedBefore;

        // Metrics aggregation is deliberately outside the measured update interval.
        foreach (var handle in workload.Workers)
        {
            if (handle.Context is Stress worker)
            {
                payloadOps += worker.LastFrameOps;
                worker.LastFrameOps = 0;
            }
        }
    }

    var meanTickMs = elapsedTicks * 1000.0 / Stopwatch.Frequency / measuredTicks;
    var ticksPerSecond = meanTickMs > 0 ? 1000.0 / meanTickMs : 0;
    return new BenchResult(
        path,
        trial,
        workerCount,
        tickInterval,
        warmupTicks,
        measuredTicks,
        setupOrCompositionMs,
        bundleLoadMs,
        path == "FSM_COS" ? Math.Max(0, compositionMs - bundleLoadMs) : 0,
        meanTickMs,
        ticksPerSecond,
        payloadOps / measuredTicks,
        allocatedBytes / measuredTicks);
}

static void Cleanup(Stress workload, int interval, string group)
{
    foreach (var worker in workload.Workers)
        FSM_API.Interaction.DestroyInstance(worker);

    FSM_API.Interaction.DestroyInstance(workload.Status);
    FSM_API.Interaction.DestroyFiniteStateMachine($"WorkerFSM_{interval}", group);
    FSM_API.Interaction.DestroyFiniteStateMachine("StressManagerFSM", group);
    workload.IsValid = false;
}

sealed record BenchResult(
    string Path,
    int Trial,
    int Workers,
    int TickInterval,
    int WarmupTicks,
    int MeasuredTicks,
    double SetupOrCompositionMs,
    double BundleLoadMs,
    double KernelResidualMs,
    double MeanTickMs,
    double TicksPerSecond,
    long PayloadOpsPerTick,
    long AllocatedBytesPerTick)
{
    public string ToCsv() =>
        $"{Path},{Trial},{Workers},{TickInterval},{WarmupTicks},{MeasuredTicks}," +
        $"{SetupOrCompositionMs:F6},{BundleLoadMs:F6},{KernelResidualMs:F6}," +
        $"{MeanTickMs:F6},{TicksPerSecond:F3},{PayloadOpsPerTick},{AllocatedBytesPerTick}";
}

sealed class StressMicroBundle : IMicroBundle
{
    private readonly int _workerCount;
    private readonly int _tickInterval;
    private readonly string _groupName;

    public StressMicroBundle(int workerCount, int tickInterval, string groupName)
    {
        _workerCount = workerCount;
        _tickInterval = tickInterval;
        _groupName = groupName;
        Descriptor = new MicroBundleDescriptor(
            9_001,
            "1.0.0",
            dependencies: Array.Empty<MicroBundleDependency>());
        Dependencies = Array.Empty<MicroBundleDependencyRequest>();
    }

    public MicroBundleDescriptor Descriptor { get; }
    public ulong Id => Descriptor.Id;
    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; }
    public Stress? Workload { get; private set; }
    public double LoadMilliseconds { get; private set; }

    public void Load(IMicroBundleLoadContext context)
    {
        var timer = Stopwatch.StartNew();
        Workload = new Stress(_workerCount, _tickInterval, _groupName);
        Workload.SpawnWorkers();
        timer.Stop();
        LoadMilliseconds = timer.Elapsed.TotalMilliseconds;
    }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex) => false;
}

sealed class StressCatalog : IMicroBundleCatalog
{
    private readonly IReadOnlyDictionary<ulong, IMicroBundle> _bundles;

    public StressCatalog(params IMicroBundle[] bundles) =>
        _bundles = bundles.ToDictionary(bundle => bundle.Id);

    public bool TryResolve(ulong bundleId, string version, out IMicroBundle? bundle)
    {
        if (_bundles.TryGetValue(bundleId, out var found) &&
            string.Equals(found.Descriptor.Version, version, StringComparison.Ordinal))
        {
            bundle = found;
            return true;
        }

        bundle = null;
        return false;
    }

    public bool TryResolve(ulong bundleId, out IMicroBundle? bundle) =>
        _bundles.TryGetValue(bundleId, out bundle);
}
