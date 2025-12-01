using TheSingularityWorkshop.FSM_API;

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