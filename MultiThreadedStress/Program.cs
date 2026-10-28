using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using TheSingularityWorkshop.FSM_API;

const string CsvPath = "multithreaded_stress.csv";

var options = Options.Parse(args);
if (options.Help)
{
    Options.PrintHelp();
    return;
}

Console.WriteLine("SimpleDemos — FSM_API multithreaded stress");
Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");
Console.WriteLine($"Workers: {options.Workers}; iterations per worker: {options.Iterations}; trials: {options.Trials}");
Console.WriteLine();
Console.WriteLine("This is an exploratory concurrency test, not a claim that every FSM_API operation is thread-safe.");
Console.WriteLine("Definitions and instances are created before timing. Structural mutation is not performed during the measured ticks.");
Console.WriteLine();

var rows = new List<TrialRow>();

// Warm up both paths. The warm-up is deliberately excluded from all reported measurements.
RunTrial(Scenario.SerialSharedGroup, options, trial: 0, isWarmup: true);
RunTrial(Scenario.ConcurrentIndependentGroups, options, trial: 0, isWarmup: true);
RunTrial(Scenario.ParallelControlIndependentCounters, options, trial: 0, isWarmup: true);
RunTrial(Scenario.ParallelControlSharedCounter, options, trial: 0, isWarmup: true);

for (var trial = 1; trial <= options.Trials; trial++)
{
    rows.Add(RunTrial(Scenario.SerialSharedGroup, options, trial, isWarmup: false));
    rows.Add(RunTrial(Scenario.ConcurrentIndependentGroups, options, trial, isWarmup: false));
    rows.Add(RunTrial(Scenario.ParallelControlIndependentCounters, options, trial, isWarmup: false));
    rows.Add(RunTrial(Scenario.ParallelControlSharedCounter, options, trial, isWarmup: false));
}

WriteCsv(CsvPath, rows);
PrintSummary(rows);
if (rows.Any(row => !row.CorrectnessPassed))
    Environment.ExitCode = 1;
Console.WriteLine();
Console.WriteLine($"Raw trial data: {Path.GetFullPath(CsvPath)}");

TrialRow RunTrial(Scenario scenario, Options config, int trial, bool isWarmup)
{
    var setupWatch = Stopwatch.StartNew();
    var runId = Guid.NewGuid().ToString("N")[..8];
    var workers = new WorkerContext[config.Workers];

    if (scenario == Scenario.SerialSharedGroup)
    {
        var group = $"MTS_Serial_{runId}";
        FSM_API.Create.CreateProcessingGroup(group);
        var definition = $"MTS_Worker_{runId}";
        FSM_API.Create.CreateFiniteStateMachine(definition, 1, group)
            .State("Active", null, OnUpdate, null)
            .BuildDefinition();

        for (var i = 0; i < workers.Length; i++)
        {
            workers[i] = new WorkerContext($"worker-{i}");
            workers[i].Status = FSM_API.Create.CreateInstance(definition, workers[i], group);
        }

        setupWatch.Stop();
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var watch = Stopwatch.StartNew();
        for (var iteration = 0; iteration < config.Iterations; iteration++)
            FSM_API.Interaction.Update(group);
        watch.Stop();
        var allocated = Math.Max(0, GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore);
        return BuildRow(scenario, config, trial, isWarmup, setupWatch.Elapsed.TotalMilliseconds,
            watch.Elapsed, allocated, workers, checked((long)config.Workers * config.Iterations));
    }
    else if (scenario == Scenario.ConcurrentIndependentGroups)
    {
        var groups = new string[config.Workers];
        for (var i = 0; i < workers.Length; i++)
        {
            groups[i] = $"MTS_Parallel_{runId}_{i}";
            var definition = $"MTS_Worker_{runId}_{i}";
            FSM_API.Create.CreateProcessingGroup(groups[i]);
            FSM_API.Create.CreateFiniteStateMachine(definition, 1, groups[i])
                .State("Active", null, OnUpdate, null)
                .BuildDefinition();

            workers[i] = new WorkerContext($"worker-{i}");
            workers[i].Status = FSM_API.Create.CreateInstance(definition, workers[i], groups[i]);
        }

        setupWatch.Stop();
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var watch = Stopwatch.StartNew();
        Parallel.For(0, config.Workers, workerIndex =>
        {
            for (var iteration = 0; iteration < config.Iterations; iteration++)
                FSM_API.Interaction.Update(groups[workerIndex]);
        });
        watch.Stop();
        var allocated = Math.Max(0, GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore);
        return BuildRow(scenario, config, trial, isWarmup, setupWatch.Elapsed.TotalMilliseconds,
            watch.Elapsed, allocated, workers, checked((long)config.Workers * config.Iterations));
    }
    else if (scenario == Scenario.ParallelControlIndependentCounters)
    {
        for (var i = 0; i < workers.Length; i++)
            workers[i] = new WorkerContext($"worker-{i}");

        setupWatch.Stop();
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var watch = Stopwatch.StartNew();
        Parallel.For(0, config.Workers, workerIndex =>
        {
            var worker = workers[workerIndex];
            for (var iteration = 0; iteration < config.Iterations; iteration++)
                ExecutePayload(worker);
        });
        watch.Stop();
        var allocated = Math.Max(0, GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore);
        return BuildRow(scenario, config, trial, isWarmup, setupWatch.Elapsed.TotalMilliseconds,
            watch.Elapsed, allocated, workers, checked((long)config.Workers * config.Iterations));
    }
    else
    {
        long sharedCounter = 0;
        setupWatch.Stop();
        var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
        var watch = Stopwatch.StartNew();
        Parallel.For(0, config.Workers, _ =>
        {
            for (var iteration = 0; iteration < config.Iterations; iteration++)
                Interlocked.Increment(ref sharedCounter);
        });
        watch.Stop();
        var allocated = Math.Max(0, GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore);
        // Store the aggregate in one slot so the common correctness gate compares the
        // total operation count without pretending the shared counter is per-worker data.
        workers[0] = new WorkerContext("shared-counter") { DataValue = sharedCounter };
        for (var i = 1; i < workers.Length; i++)
            workers[i] = new WorkerContext($"worker-{i}");
        return BuildRow(scenario, config, trial, isWarmup, setupWatch.Elapsed.TotalMilliseconds,
            watch.Elapsed, allocated, workers, checked((long)config.Workers * config.Iterations));
    }
}

static void ExecutePayload(WorkerContext worker)
{
    var value = Interlocked.Increment(ref worker.DataValue);
    var payload = value.ToString(CultureInfo.InvariantCulture);
    if ((value & 1) == 0)
        _ = Math.Abs(value);
    worker.LastPayloadLength = payload.Length;
}

static void OnUpdate(IStateContext stateContext)
{
    if (stateContext is not WorkerContext worker)
        throw new InvalidOperationException($"Unexpected FSM context type: {stateContext.GetType().FullName}");

    // Preserve the string-backed work that is expected to become a useful comparison
    // point for the future integer-backed FSM_API path.
    ExecutePayload(worker);
}

static TrialRow BuildRow(Scenario scenario, Options config, int trial, bool warmup,
    double setupMilliseconds, TimeSpan elapsed, long allocatedBytes, WorkerContext[] workers, long expected)
{
    var completed = workers.Sum(worker => Interlocked.Read(ref worker.DataValue));
    var passed = completed == expected;
    if (!passed && !warmup)
        Console.WriteLine($"CORRECTNESS FAILURE: {scenario}; expected {expected:N0}, observed {completed:N0}");

    if (!warmup)
    {
        var opsPerSecond = elapsed.TotalSeconds > 0 ? completed / elapsed.TotalSeconds : 0;
        Console.WriteLine($"{scenario,-30} trial {trial}: {elapsed.TotalMilliseconds,10:F2} ms | {opsPerSecond,14:N0} callbacks/s | correctness: {(passed ? "PASS" : "FAIL")}");
    }

    return new TrialRow(
        DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
        scenario.ToString(), trial, config.Workers, config.Iterations, expected, completed,
        elapsed.TotalMilliseconds, elapsed.TotalSeconds > 0 ? completed / elapsed.TotalSeconds : 0,
        setupMilliseconds, allocatedBytes, passed, RuntimeInformation.FrameworkDescription,
        RuntimeInformation.OSDescription, Environment.ProcessorCount,
        Environment.Version.ToString(), warmup);
}

static void PrintSummary(List<TrialRow> rows)
{
    Console.WriteLine();
    Console.WriteLine("Summary (median elapsed time; compare only on the same machine/runtime):");
    foreach (var group in rows.GroupBy(row => row.Scenario))
    {
        var elapsed = group.Select(row => row.ElapsedMilliseconds).OrderBy(value => value).ToArray();
        var throughput = group.Select(row => row.CallbacksPerSecond).OrderBy(value => value).ToArray();
        Console.WriteLine($"  {group.Key,-30} median {Median(elapsed):F2} ms | median {Median(throughput):N0} callbacks/s | correctness {(group.All(row => row.CorrectnessPassed) ? "PASS" : "FAIL")}");
    }
}

static double Median(double[] values) => values.Length % 2 == 0
    ? (values[values.Length / 2 - 1] + values[values.Length / 2]) / 2d
    : values[values.Length / 2];

static void WriteCsv(string path, List<TrialRow> rows)
{
    using var writer = new StreamWriter(path, false);
    writer.WriteLine("TimestampUtc,Scenario,Trial,Workers,IterationsPerWorker,ExpectedCallbacks,CompletedCallbacks,ElapsedMilliseconds,CallbacksPerSecond,SetupMilliseconds,AllocatedBytes,CorrectnessPassed,Runtime,OS,LogicalProcessors,DotnetVersion,IsWarmup");
    foreach (var row in rows)
    {
        writer.WriteLine(string.Join(",", new[]
        {
            Csv(row.TimestampUtc), Csv(row.Scenario), row.Trial.ToString(CultureInfo.InvariantCulture),
            row.Workers.ToString(CultureInfo.InvariantCulture), row.IterationsPerWorker.ToString(CultureInfo.InvariantCulture),
            row.ExpectedCallbacks.ToString(CultureInfo.InvariantCulture), row.CompletedCallbacks.ToString(CultureInfo.InvariantCulture),
            row.ElapsedMilliseconds.ToString("F4", CultureInfo.InvariantCulture),
            row.CallbacksPerSecond.ToString("F2", CultureInfo.InvariantCulture), row.SetupMilliseconds.ToString("F4", CultureInfo.InvariantCulture),
            row.AllocatedBytes.ToString(CultureInfo.InvariantCulture), row.CorrectnessPassed.ToString(),
            Csv(row.Runtime), Csv(row.OS), row.LogicalProcessors.ToString(CultureInfo.InvariantCulture),
            Csv(row.DotnetVersion), row.IsWarmup.ToString()
        }));
    }
}

static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

enum Scenario
{
    SerialSharedGroup,
    ConcurrentIndependentGroups,
    ParallelControlIndependentCounters,
    ParallelControlSharedCounter
}

sealed class WorkerContext : IStateContext
{
    public WorkerContext(string name) => Name = name;
    public long DataValue;
    public int LastPayloadLength;
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }
    public FSMHandle Status { get; set; } = null!;
}

sealed record TrialRow(
    string TimestampUtc, string Scenario, int Trial, int Workers, int IterationsPerWorker,
    long ExpectedCallbacks, long CompletedCallbacks, double ElapsedMilliseconds,
    double CallbacksPerSecond, double SetupMilliseconds, long AllocatedBytes, bool CorrectnessPassed,
    string Runtime, string OS, int LogicalProcessors, string DotnetVersion, bool IsWarmup);

sealed record Options(int Workers, int Iterations, int Trials, bool Help)
{
    public static Options Parse(string[] args)
    {
        var workers = Math.Clamp(Environment.ProcessorCount, 2, 8);
        var iterations = 2_000;
        var trials = 3;
        var help = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--workers":
                    workers = ReadPositive(args, ref i, "--workers", 1, 64);
                    break;
                case "--iterations":
                    iterations = ReadPositive(args, ref i, "--iterations", 1, 1_000_000);
                    break;
                case "--trials":
                    trials = ReadPositive(args, ref i, "--trials", 1, 25);
                    break;
                case "--help":
                case "-h":
                    help = true;
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[i]}'. Use --help for usage.");
            }
        }

        return new Options(workers, iterations, trials, help);
    }

    private static int ReadPositive(string[] args, ref int index, string option, int min, int max)
    {
        if (++index >= args.Length || !int.TryParse(args[index], out var value) || value < min || value > max)
            throw new ArgumentException($"{option} requires an integer from {min} to {max}.");
        return value;
    }

    public static void PrintHelp()
    {
        Console.WriteLine("MultiThreadedStress");
        Console.WriteLine("  dotnet run --project MultiThreadedStress/MultiThreadedStress.csproj -c Release");
        Console.WriteLine("Options:");
        Console.WriteLine("  --workers N       Worker count (1-64; default: bounded by logical CPU count)");
        Console.WriteLine("  --iterations N    Ticks per worker (1-1000000; default: 2000)");
        Console.WriteLine("  --trials N        Measured trials (1-25; default: 3)");
        Console.WriteLine("  --help            Show this help");
    }
}
