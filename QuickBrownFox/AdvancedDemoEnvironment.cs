using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDemoEnvironment : IStateContext
{
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
            foreach (IAdvancedAgent agent in env.GetAgents())
            {
                var agents = env.GetAgents();
                agents.Remove(agent);
                foreach (var agent2 in agents)
                {
                    if (agent.Position.X - agent2.Position.X <= float.Epsilon
                        && agent.Position.Y - agent2.Position.Y <= float.Epsilon)
                    {
                        if (!agent.CollidingAgents.Contains(agent2))
                        {
                            agent.CollidingAgents.Add(agent2);
                        }
                        if (!agent2.CollidingAgents.Contains(agent))
                        {
                            agent2.CollidingAgents.Add(agent);
                        }
                    }
                }

                agents.Where(s => ManhattanDistance(s.Position, agent.Position) <= s.SightRange).ToList();
            }


        }
    }

    private List<IAdvancedAgent> GetAgents()
    {
        return _agents.ToList();
    }

    public void AddAgent(IAdvancedAgent agent)
    {
        _agents.Add(agent);
    }

    public bool IsEmptyAt(Vector2 pos)
    {
        return _agents.Any(s => s.Position.X == pos.X && s.Position.Y == pos.Y);
    }

    public IAdvancedAgent? GetNearestFox(AdvancedDogContext dog)
    {
        return _agents.Where(s => s is AdvancedFoxContext fox).OrderBy(s => ManhattanDistance(s.Position, dog.Position)).FirstOrDefault();
    }

    private float ManhattanDistance(Vector2 position1, Vector2 position2)
    {
        float dx = Math.Abs(position1.x - position2.x);
        float dy = Math.Abs(position1.y - position2.y);

        return dx + dy;
    }

    public bool IsValid { get; set; }
    public string Name { get; set; }
}