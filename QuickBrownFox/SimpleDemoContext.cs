


using System.Diagnostics;
using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class SimpleDemoContext : IStateContext
{
    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "SimpleDemo";
    public int WinPosition { get; } = 10;

    private List<ISimpleAgent> agents = new List<ISimpleAgent>();

    public SimpleDemoContext()
    {
        if (!FSM_API.Interaction.Exists("AppFSM", "Main"))
        {
            //Create an FSM for our application
            FSM_API.Create.CreateProcessingGroup("Main");
            FSM_API.Create.CreateFiniteStateMachine("AppFSM", -1, "Main")
                .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                .State("Shutdown", OnEnterShutdown, null, null)
                .Transition("Executing", "Shutdown", (ctx) => Console.KeyAvailable
                ||agents.Any(s=>s.State.CurrentState == "Mangled")
                || agents.Any(s=>s is QuickBrownFox fox && fox.Position == ((SimpleDemoContext)ctx).WinPosition))
                .BuildDefinition();
        }
        //Now register this context with the "AppFSM"
        FSM_API.Create.CreateInstance("AppFSM", this, "Main");


        IsValid = true;
    }


   



    private void OnEnterShutdown(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            Console.WriteLine("Shutting down the Simple Demo...");
            foreach (var agent in demo.agents)
            {
                FSM_API.Interaction.DestroyInstance(agent.State);
            }
            FSM_API.Interaction.DestroyFiniteStateMachine("AppFSM", "Main");
            FSM_API.Interaction.DestroyFiniteStateMachine("AgentFSM", "Update");
            FSM_API.Interaction.RemoveProcessingGroup("Main");
            FSM_API.Interaction.RemoveProcessingGroup("Update");
            demo.IsValid = false;
        }
    }

    void OnExitExecuting(IStateContext context)
    {
        Console.WriteLine("App has finished executing. Initiating graceful shutdown.");
    }

    
    void OnUpdateExecuting(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            // The main loop for our simulation

            // Step 1: Clear previous frame's vision and collision data
            foreach (var agent in demo.agents)
            {
                agent.CollidedAgents.Clear();
                agent.VisibleAgents.Clear();
            }

            // Step 2: Perform vision and collision checks for all agents
            // Use a nested loop to compare each agent once.
            for (int i = 0; i < demo.agents.Count; i++)
            {
                var currentAgent = demo.agents[i];
                for (int j = i + 1; j < demo.agents.Count; j++)
                {
                    var otherAgent = demo.agents[j];
                    float distance = Math.Abs(currentAgent.Position - otherAgent.Position);

                    // Vision logic: check if agents are within sight range
                    if (distance <= currentAgent.Sight)
                    {
                        currentAgent.VisibleAgents.Add(otherAgent);
                        otherAgent.VisibleAgents.Add(currentAgent);
                        Console.WriteLine($"{((IStateContext)currentAgent).Name} sees {((IStateContext)otherAgent).Name}.");
                    }

                    // Collision logic: check if agents have the same position
                    if (distance == 0 && currentAgent.State.CurrentState != "Jumping"
                        && otherAgent.State.CurrentState != "Jumping")
                    {
                        currentAgent.CollidedAgents.Add(otherAgent);
                        otherAgent.CollidedAgents.Add(currentAgent);
                        Console.WriteLine($"{((IStateContext)currentAgent).Name} has collided with {((IStateContext)otherAgent).Name}!");
                    }
                }
            }

            // Step 3: Let the agents' FSMs handle their own logic
            FSM_API.Interaction.Update("Update");
        }
    }

    void OnEnterExecuting(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            //Define our Agents
            QuickBrownFox fox = new QuickBrownFox(0);
            LazySleepingDog dog = new LazySleepingDog(3);
            demo.RegisterAgent(fox);
            demo.RegisterAgent(dog);
        }
    }

    private void RegisterAgent(ISimpleAgent agent)
    {
        agents.Add(agent);
    }
}