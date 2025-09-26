using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDogContext : IAdvancedAgent
{
    public AdvancedDogContext(int id, Vector2 pos, AdvancedDemoEnvironment environment)
    {
        Position = pos;
        if (!FSM_API.Interaction.Exists("AdvancedDogFSM", "Dogs"))
        {
            FSM_API.Create.CreateFiniteStateMachine("AdvancedDogFSM", -1, "Dogs")
                .State("Sleeping", null, null, null)
                .State("Idle", OnEnterIdle, null, null)
                .State("Chasing", OnEnterChasing, OnUpdateChasing, OnExitChasing)
                .State("Mangling", OnEnterMangling, null, null)
                .Transition("Sleeping", "Idle", ShouldWake)
                .Transition("Idle", "Chasing", ShouldChase)
                .Transition("Chasing", "Mangling", IsManglingFox)
                .Transition("Mangling", "Idle", (ctx) => true)
                .Transition("Idle", "Sleeping", ShouldSleep)
                .BuildDefinition();
        }
        Status = FSM_API.Create.CreateInstance("AdvancedDogFSM", this, "Dogs");
        Environment = environment;
        Name = $"Dog[{id}]";
        IsValid = true;
    }

    private void OnEnterIdle(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            var fox = dog.Environment.GetNearestFox(dog);
            if (fox != null)
            {
                dog.Chasing = fox;
            }
        }

    }

    private void OnEnterChasing(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            Console.WriteLine($"{dog.Name} has started chasing:  {dog.Chasing.Name}");
        }
    }

    private void OnUpdateChasing(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            var dogPos = dog.Position;
            var foxPos = dog.Chasing.Position;
            var dx = dogPos.X - foxPos.X;
            var dy = dogPos.Y - foxPos.Y;
            if (dx >= dy)
            {
                var x = dog.Position.X;
                x += dog.Speed;
                dog.Position = new Vector2(x, dog.Position.Y);
            }
            else
            {
                var y = dog.Position.Y;
                y += dog.Speed;
                dog.Position = new Vector2(dog.Position.X, y);
            }
        }
    }

    private void OnExitChasing(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {

        }
    }

    private void OnEnterMangling(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            dog.Chasing.Status.TransitionTo("Mangled");
        }
    }

    private bool ShouldWake(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            return dog.CollidingAgents.Count > 0;
        }
        return true;
    }

    private bool ShouldChase(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            return dog.Chasing != null;
        }
        return true;
    }

    private bool IsManglingFox(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            var dx = dog.Position.X - dog.Chasing.Position.X;
            var dy = dog.Position.Y - dog.Chasing.Position.Y;
            if (dx <= .25f && dy <= .25f)
            {
                return true;
            }
        }
        return false;
    }

    private bool ShouldSleep(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            return !dog.VisibleAgents.Any(agent => agent is AdvancedFoxContext fox);
        }
        return false;
    }

    public int SightRange { get; } = 3;
    private IAdvancedAgent Chasing { get; set; }
    private AdvancedDemoEnvironment Environment { get; }
    public Vector2 Position { get; set; }
    public FSMHandle Status { get; }
    public List<IAdvancedAgent> VisibleAgents { get; } = new List<IAdvancedAgent>();
    public List<IAdvancedAgent> CollidingAgents { get; } = new List<IAdvancedAgent>();
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    public int Speed { get; private set; }
}