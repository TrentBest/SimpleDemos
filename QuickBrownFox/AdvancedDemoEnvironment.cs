using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDemoEnvironment : IStateContext
{
    // Updated environment bounds to 100x100
    public int EnvironmentWidth { get; private set; } = 100;
    public int EngironmentHeight { get; private set; } = 100;

    private List<IAdvancedAgent> _agents = new List<IAdvancedAgent>();

    public AdvancedDemoEnvironment()
    {
        if (!FSM_API.Interaction.Exists("AdvancedDemoEnvironmentFSM", "Environment"))
        {
            FSM_API.Create.CreateFiniteStateMachine("AdvancedDemoEnvironmentFSM", -1, "Environment")
                .State("Existing", null, OnUpdateExisting, null)
                .BuildDefinition();
        }
        FSM_API.Create.CreateInstance("AdvancedDemoEnvironmentFSM", this, "Environment");
        Name = "AdvancedDemoEnvironment";
        IsValid = true;
    }

    private void OnUpdateExisting(IStateContext context)
    {
        if (context is AdvancedDemoEnvironment env)
        {
            // Reset vision and collision data for all agents
            foreach (var agent in env._agents)
            {
                agent.CollidingAgents.Clear();
                agent.VisibleAgents.Clear();
            }

            // Perform vision and collision checks for all agents
            // Using a nested loop with index to avoid duplicate checks
            for (int i = 0; i < env._agents.Count; i++)
            {
                var currentAgent = env._agents[i];
                for (int j = i + 1; j < env._agents.Count; j++)
                {
                    var otherAgent = env._agents[j];

                    // Collision logic (exact integer position match for grid-based agents)
                    if (Vector2.Distance(currentAgent.Position, otherAgent.Position) < 0.5f
                        && currentAgent.Status.CurrentState != "Jumping"
                        && otherAgent.Status.CurrentState != "Jumping")
                    {
                        currentAgent.CollidingAgents.Add(otherAgent);
                        otherAgent.CollidingAgents.Add(currentAgent);
                    }

                    // Vision logic (using Manhattan distance)
                    if (ManhattanDistance(currentAgent.Position, otherAgent.Position) <= currentAgent.SightRange)
                    {
                        currentAgent.VisibleAgents.Add(otherAgent);
                    }
                    if (ManhattanDistance(currentAgent.Position, otherAgent.Position) <= otherAgent.SightRange)
                    {
                        otherAgent.VisibleAgents.Add(currentAgent);
                    }
                }
            }
        }
    }

    // New helper method for agent clearing
    public void ClearAgents()
    {
        _agents.Clear();
    }

    public List<IAdvancedAgent> GetAgents()
    {
        return _agents.ToList();
    }

    public void AddAgent(IAdvancedAgent agent)
    {
        _agents.Add(agent);
    }

    // Corrected IsEmptyAt: returns TRUE if NO agent is occupying the space (it IS empty).
    public bool IsEmptyAt(Vector2 pos)
    {
        // Use a small distance tolerance for 2D float comparison (for integer positions, 0.5f is fine)
        return !_agents.Any(s => Vector2.Distance(s.Position, pos) < 0.5f);
    }

    public IAdvancedAgent? GetNearestFox(AdvancedDogContext dog)
    {
        return _agents.Where(s => s is AdvancedFoxContext fox).OrderBy(s => ManhattanDistance(s.Position, dog.Position)).FirstOrDefault();
    }

    private float ManhattanDistance(Vector2 position1, Vector2 position2)
    {
        float dx = Math.Abs(position1.X - position2.X);
        float dy = Math.Abs(position1.Y - position2.Y);

        return dx + dy;
    }

    public bool IsValid { get; set; }
    public string Name { get; set; }
}
