using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

using TheSingularityWorkshop.FSM_API;

class Program
{
    // Data container for our report
    struct BenchResult
    {
        public int Interval;
        public int Agents;
        public double FPS;
        public long OpsPerFrame;
        public long MemoryMB;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("=== The Singularity Workshop: Final Baseline Profiler ===");
        Console.WriteLine("Generating artifacts for Coder Legion Article...");

        string csvPath = "benchmark_data.csv";
        string tablePath = "article_table.txt";
        List<BenchResult> allResults = new List<BenchResult>();

        // Configuration
        var rounds = new[]
        {
            new { Interval = 1, StartLoad = 50000, Increment = 5000 },
            new { Interval = 2, StartLoad = 80000, Increment = 5000 },
            new { Interval = 3, StartLoad = 120000, Increment = 10000 } // Aggressive steps
        };

        double minFpsThreshold = 30.0;
        int framesPerBatch = 60;

        // Initialize CSV with Header
        using (StreamWriter sw = new StreamWriter(csvPath, false))
        {
            sw.WriteLine("Interval,Agents,FPS,OpsFrame,MemoryMB");
        }

        foreach (var round in rounds)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n>>> ROUND START: INTERVAL {round.Interval} <<<");
            Console.ResetColor();

            int currentLoad = round.StartLoad;
            bool roundActive = true;

            while (roundActive)
            {
                // 1. Setup
                Console.Write($"[Init {currentLoad}]... ");
                var stressContext = new Stress(currentLoad, round.Interval);
                stressContext.SpawnWorkers();
                FSM_API.Interaction.Update("Stress"); // Settle tick
                Console.WriteLine("Ready.");

                // 2. Execution
                long totalOps = 0;
                double totalTimeMs = 0;
                Stopwatch batchTimer = new Stopwatch();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                long startMem = GC.GetTotalMemory(true);

                for (int frame = 0; frame < framesPerBatch; frame++)
                {
                    batchTimer.Restart();

                    FSM_API.Interaction.Update("Stress");

                    // Aggregate metrics
                    foreach (var wHandle in stressContext.Workers)
                    {
                        if (wHandle.Context is Stress wCtx)
                        {
                            totalOps += wCtx.LastFrameOps;
                            wCtx.LastFrameOps = 0;
                        }
                    }

                    batchTimer.Stop();
                    totalTimeMs += batchTimer.Elapsed.TotalMilliseconds;
                }

                // 3. Analysis
                double avgFrameTime = totalTimeMs / framesPerBatch;
                double fps = 1000.0 / avgFrameTime;
                long opsPerFrame = totalOps / framesPerBatch;
                long memUsed = (GC.GetTotalMemory(false) - startMem) / 1024 / 1024;

                // Log to Console
                Console.WriteLine($"  -> {fps:F2} FPS | {opsPerFrame:N0} Ops | {memUsed} MB Delta");

                // Record Data
                var result = new BenchResult
                {
                    Interval = round.Interval,
                    Agents = currentLoad,
                    FPS = fps,
                    OpsPerFrame = opsPerFrame,
                    MemoryMB = memUsed
                };
                allResults.Add(result);

                // Append to CSV immediately
                using (StreamWriter sw = new StreamWriter(csvPath, true))
                {
                    sw.WriteLine($"{result.Interval},{result.Agents},{result.FPS:F2},{result.OpsPerFrame},{result.MemoryMB}");
                }

                // 4. Cleanup
                foreach (var w in stressContext.Workers) FSM_API.Interaction.DestroyInstance(w);
                FSM_API.Interaction.DestroyInstance(stressContext.Status);
                stressContext.IsValid = false;

                // 5. Threshold Check
                if (fps < minFpsThreshold)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[LIMIT REACHED] {currentLoad} Agents @ {fps:F2} FPS");
                    Console.ResetColor();
                    roundActive = false;
                }
                else
                {
                    currentLoad += round.Increment;
                }
            }

            // Cleanup Definition to prevent collisions
            FSM_API.Interaction.DestroyFiniteStateMachine($"WorkerFSM_{round.Interval}", "Stress");
        }

        // Generate Article Table
        GenerateMarkdownTable(allResults, tablePath);

        Console.WriteLine("\nBenchmark Complete. Files generated.");
        Console.WriteLine($"1. {csvPath}");
        Console.WriteLine($"2. {tablePath}");
        Console.ReadKey();
    }

    static void GenerateMarkdownTable(List<BenchResult> results, string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("### Stress Test Results (Baseline API)");
        sb.AppendLine("| Interval | Agents | FPS | Logic Ops/Frame | Mem Delta (MB) |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var r in results)
        {
            // Highlight rows near the limit (below 35 FPS) in Bold
            if (r.FPS < 35.0)
            {
                sb.AppendLine($"| {r.Interval} | **{r.Agents}** | **{r.FPS:F2}** | {r.OpsPerFrame:N0} | {r.MemoryMB} |");
            }
            else
            {
                sb.AppendLine($"| {r.Interval} | {r.Agents} | {r.FPS:F2} | {r.OpsPerFrame:N0} | {r.MemoryMB} |");
            }
        }

        File.WriteAllText(path, sb.ToString());
    }
}