using System.Diagnostics;
using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class SimpleDemoContext : IStateContext
{
    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "SimpleDemo";
    public int WinPosition { get; } = 10;
    public int CurrentFrame { get; private set; } = 0;
    public static bool EnableLogging { get; internal set; } = true;

    private List<ISimpleAgent> agents = new List<ISimpleAgent>();

    public SimpleDemoContext()
    {
        if (!FSM_API.Interaction.Exists("AppFSM", "Main"))
        {
            FSM_API.Create.CreateProcessingGroup("Main");
            FSM_API.Create.CreateFiniteStateMachine("AppFSM", -1, "Main")
                .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                .State("Shutdown", OnEnterShutdown, null, null)
                .Transition("Executing", "Shutdown", (ctx) => Console.KeyAvailable
                || agents.Any(s => s.State.CurrentState == "Mangled")
                || agents.Any(s => s is QuickBrownFox fox && fox.Position == ((SimpleDemoContext)ctx).WinPosition))
                .BuildDefinition();
        }
        FSM_API.Create.CreateInstance("AppFSM", this, "Main");
        IsValid = true;
    }

    private void OnEnterShutdown(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            Console.WriteLine("Shutting down the Simple Demo...");
            foreach (var agent in demo.agents) FSM_API.Interaction.DestroyInstance(agent.State);
            demo.IsValid = false;
        }
    }

    void OnExitExecuting(IStateContext context) { Console.WriteLine("Basic Demo has finished executing."); }

    void OnUpdateExecuting(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            // Reset Lists
            foreach (var agent in demo.agents) { agent.CollidedAgents.Clear(); agent.VisibleAgents.Clear(); }

            // Detection Logic
            foreach (var agent in agents)
            {
                // Sleeping agents don't look for others, but others can look for them
                if (agent.State.CurrentState != "Sleeping")
                {
                    var dupAgents = agents.ToList();
                    dupAgents.Remove(agent);
                    foreach (var otherAgent in dupAgents)
                    {
                        // Debug Output
                        //Console.WriteLine("\n= = = = = = = = = = = =");
                        //Console.WriteLine($"{agent.Name}:  Current State:  {agent.State.CurrentState}");
                        //Console.WriteLine($"{otherAgent.Name}:  Current State:  {otherAgent.State.CurrentState}");
                        //Console.WriteLine("= = = = = = = = = = = =\n");
                        // Vision Check (+X Direction Only)
                        if (agent.Position < otherAgent.Position && agent.Position + agent.Sight >= otherAgent.Position)
                        {
                            //Console.WriteLine($"{agent.Name} @ {agent.Position} sees {otherAgent.Name} @ {otherAgent.Position}");
                            agent.VisibleAgents.Add(otherAgent);
                        }


                       
                       
                        

                        if (agent.Position==otherAgent.Position && (agent.State.CurrentState != "Jumping" && otherAgent.State.CurrentState != "Jumping"))
                        {
                            //Console.WriteLine($"!!! COLLISION DETECTED between {agent.Name} and {otherAgent.Name} !!!");
                            agent.CollidedAgents.Add(otherAgent);
                            otherAgent.CollidedAgents.Add(agent);
                        }
                    }
                }
            }

            FSM_API.Interaction.Update("Update");
            demo.CurrentFrame++;
        }
    }

    void OnEnterExecuting(IStateContext context)
    {
        if (context is SimpleDemoContext demo)
        {
            LazySleepingDog dog = new LazySleepingDog(3);
            QuickBrownFox fox = new QuickBrownFox(0);
            demo.RegisterAgent(dog);
            demo.RegisterAgent(fox);
        }
    }

    private void RegisterAgent(ISimpleAgent agent) { agents.Add(agent); }
}