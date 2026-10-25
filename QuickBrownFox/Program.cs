using System.Text.Json;
using TheSingularityWorkshop.FSM_API;

if (args.Length == 2 && args[0] == "--export-advanced-trace")
{
    var originalOutput = Console.Out;
    var frames = new List<object>();
    AdvancedDemoContext traceContext;

    try
    {
        Console.SetOut(TextWriter.Null);
        traceContext = new AdvancedDemoContext(randomSeed: 42, allowConsoleInput: false);
        for (var i = 0; i < 30 && traceContext.IsValid; i++)
        {
            FSM_API.Interaction.Update("Update");
            frames.Add(traceContext.CaptureSnapshot());
        }
    }
    finally
    {
        Console.SetOut(originalOutput);
    }

    var outputPath = Path.GetFullPath(args[1]);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    var trace = new
    {
        schemaVersion = 1,
        source = "SimpleDemos Advanced 2D fox-and-dog simulation running through FSM_API",
        frames
    };
    File.WriteAllText(outputPath, JsonSerializer.Serialize(trace, new JsonSerializerOptions
    {
        WriteIndented = true
    }));

    Console.WriteLine($"Exported {frames.Count} deterministic advanced FSM snapshots to {outputPath}");
    return;
}

if (args.Length == 2 && args[0] == "--export-trace")
{
    SimpleDemoContext.EnableLogging = false;
    var traceContext = new SimpleDemoContext(allowConsoleInput: false);
    var frames = new List<object>();

    // Run the real C# FSMs for a bounded number of updates, stopping at the demo's normal end.
    for (var i = 0; i < 30 && traceContext.IsValid; i++)
    {
        FSM_API.Interaction.Update("Main");
        frames.Add(traceContext.CaptureSnapshot());
    }

    var outputPath = Path.GetFullPath(args[1]);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    var trace = new
    {
        schemaVersion = 1,
        source = "SimpleDemos QuickBrownFox running through FSM_API",
        frames
    };
    File.WriteAllText(outputPath, JsonSerializer.Serialize(trace, new JsonSerializerOptions
    {
        WriteIndented = true
    }));

    Console.WriteLine($"Exported {frames.Count} C# FSM snapshots to {outputPath}");
    return;
}

// ---------------------------------------------------------
// Part 1: Run the Interactive Simple Demo
// ---------------------------------------------------------
Console.WriteLine("Welcome to the Quick Brown Fox Demo!");
SimpleDemoContext appContext = new SimpleDemoContext();

// Run the simple demo loop
do
{
    FSM_API.Interaction.Update("Main");
    // Small sleep to keep the simple demo readable in the console
    Thread.Sleep(500);
} while (appContext.IsValid);


// ---------------------------------------------------------
// Part 2: Offer the High-Performance Stress Test
// ---------------------------------------------------------
Console.WriteLine("\n-------------------------------------------------------------");
Console.WriteLine("Simple Demo Finished.");
Console.WriteLine("-------------------------------------------------------------");
Console.WriteLine("Would you like to run the FSM API Stress Test?");
Console.WriteLine("This will benchmark how many agents your machine can handle at >30 FPS.");
Console.WriteLine("Note: This ignores collision logic to purely test FSM throughput.");
Console.Write("[Y] Yes / [Any other key] Exit: ");

var key = Console.ReadKey().Key;
Console.WriteLine("\n");

if (key == ConsoleKey.Y)
{
    // Initialize the specialized Stress Test Environment
    StressTestContext stressContext = new StressTestContext();

    // Run the stress test loop (Uncapped speed)
    while (stressContext.IsValid)
    {
        stressContext.Update();
    }
}

Console.WriteLine("Press any key to close application...");
Console.ReadKey();