using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDemoContext : IStateContext
{
    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "AdvancedDemo";

    // Set environment bounds to 100x100
    public int EnvironmentWidth { get; private set; } = 100;
    public int EngironmentHeight { get; private set; } = 100; // Keeping original typo for consistency, updated value
    public int FoxCount { get; private set; } = 15;
    public int DogCount { get; private set; } = 5;

    private AdvancedDemoEnvironment environment;
    private Random random = new Random();

    public AdvancedDemoContext()
    {
        FSM_API.Internal.ResetAPI(true);

        if (!FSM_API.Interaction.Exists("AdvancedDemoFSM", "Update"))
        {
            FSM_API.Create.CreateProcessingGroup("Environment");
            FSM_API.Create.CreateProcessingGroup("Foxes");
            FSM_API.Create.CreateProcessingGroup("Dogs");

            FSM_API.Create.CreateFiniteStateMachine("AdvancedDemoFSM", -1, "Update")
                .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                .State("Shutdown", OnEnterShutdown, null, null)
                .Transition("Executing", "Shutdown", ShouldShutDown)
                .BuildDefinition();
        }

        environment = new AdvancedDemoEnvironment();
        FSM_API.Create.CreateInstance("AdvancedDemoFSM", this, "Update");
        IsValid = true;
    }

    private Vector2 GetRandomPosition(AdvancedDemoContext context)
    {
        // Ensure integer positions
        return new Vector2(
            random.Next(0, context.EnvironmentWidth),
            random.Next(0, context.EngironmentHeight));
    }

    private void OnEnterExecuting(IStateContext context)
    {
        if (context is AdvancedDemoContext adc)
        {
            Console.WriteLine("--- Starting Advanced 2D Simulation (100x100 Grid) ---");
            adc.environment.ClearAgents();

            AdvancedDemoEnvironment ade = adc.environment;

            // 1. Initialize Dogs first
            for (int d = 0; d < DogCount; d++)
            {
                var pos = GetRandomPosition(adc);

                // ade.IsEmptyAt is now corrected to return true if spot is available
                if (ade.IsEmptyAt(pos))
                {
                    var dog = new AdvancedDogContext(d, pos, ade); // Updated constructor
                    ade.AddAgent(dog);
                }
                else
                {
                    d--;//by decrementing we stay at the same dog until we land it 
                }
            }

            // 2. Initialize Foxes
            for (int f = 0; f < FoxCount; f++)
            {
                var pos = GetRandomPosition(adc);

                if (ade.IsEmptyAt(pos))
                {
                    var fox = new AdvancedFoxContext(f, pos, ade); // Updated constructor
                    // The FindRandomDestination logic inside AdvancedFoxContext handles the quadrant exclusion.
                    fox.Destination = fox.FindRandomDestination();
                    ade.AddAgent(fox);
                }
                else
                {
                    f--;
                }
            }
        }
    }

    private void OnUpdateExecuting(IStateContext context)
    {
        Console.WriteLine("\n--- Simulation Step ---");
        // 1. Update the environment (Vision and Collision)
        FSM_API.Interaction.Update("Environment");
        // 2. Update the agents
        FSM_API.Interaction.Update("Dogs");
        FSM_API.Interaction.Update("Foxes");
    }

    private void OnExitExecuting(IStateContext context)
    {
        Console.WriteLine("Execution finished. Initiating shutdown.");
    }

    private void OnEnterShutdown(IStateContext context)
    {
        if (context is AdvancedDemoContext adc)
        {
            Console.WriteLine("Shutting down the Advanced Demo...");

            // Clean up all FSMs and processing groups
            FSM_API.Interaction.DestroyFiniteStateMachine("AdvancedDemoFSM", "Update");
            FSM_API.Interaction.DestroyFiniteStateMachine("AdvancedDemoEnvironmentFSM", "Environment");
            FSM_API.Interaction.DestroyFiniteStateMachine("AdvancedDogFSM", "Dogs");
            FSM_API.Interaction.DestroyFiniteStateMachine("AdvancedFoxFSM", "Foxes");

            FSM_API.Interaction.RemoveProcessingGroup("Environment");
            FSM_API.Interaction.RemoveProcessingGroup("Foxes");
            FSM_API.Interaction.RemoveProcessingGroup("Dogs");

            adc.IsValid = false;
        }
    }

    private bool ShouldShutDown(IStateContext context)
    {
        if (context is AdvancedDemoContext adc)
        {
            // Shutdown if a key is pressed OR if all foxes are in a terminal state
            bool allFoxesAreDone = adc.environment.GetAgents()
                .Where(a => a is AdvancedFoxContext)
                .All(fox => fox.Status.CurrentState == "Idle" || fox.Status.CurrentState == "Mangled");

            return Console.KeyAvailable || allFoxesAreDone;
        }
        return false;
    }
}
