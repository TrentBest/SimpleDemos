using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDogContext : IAdvancedAgent
{
    // IAdvancedAgent properties
    public float SightRange { get; set; } = 3;
    public Vector2 Position { get; set; }
    public FSMHandle Status { get; }
    public List<IAdvancedAgent> VisibleAgents { get; } = new List<IAdvancedAgent>();
    public List<IAdvancedAgent> CollidingAgents { get; } = new List<IAdvancedAgent>();
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    public int Speed { get; private set; } = 1; // Default speed for dogs
    private IAdvancedAgent Chasing { get; set; }
    private AdvancedDemoEnvironment Environment { get; }

    // Updated Constructor
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
            // Speed up if we are chasing
            dog.Speed = 1;
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
            dog.Speed = 3; // Dog runs faster when chasing!
            Console.WriteLine($"{dog.Name} has started chasing: {dog.Chasing.Name}");
        }
    }

    private void OnUpdateChasing(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            if (dog.Chasing == null) return;

            // Calculate 2D direction towards the target fox
            Vector2 direction = Vector2.Normalize(dog.Chasing.Position - dog.Position);

            // Move towards the fox by the set speed
            dog.Position += direction * dog.Speed;

            Console.WriteLine($"{dog.Name} is chasing at {dog.Position}");
        }
    }

    private void OnExitChasing(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            dog.Speed = 1;
        }
    }

    private void OnEnterMangling(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            Console.WriteLine($"{dog.Name} is mangling {dog.Chasing.Name}!");
            dog.Chasing.Status.TransitionTo("Mangled");
        }
    }

    private bool ShouldWake(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            // Wake up if something touches the dog (collision)
            return dog.CollidingAgents.Count > 0;
        }
        return false; // Changed from true to false to only wake on collision
    }

    private bool ShouldChase(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            // Chase if a fox is visible
            return dog.VisibleAgents.Any(agent => agent is AdvancedFoxContext);
        }
        return false; // Changed from true to false
    }

    private bool IsManglingFox(IStateContext context)
    {
        if (context is AdvancedDogContext dog)
        {
            if (dog.Chasing == null) return false;

            // Use 2D distance check for collision with the target fox
            float distance = Vector2.Distance(dog.Position, dog.Chasing.Position);
            if (distance < 0.5f) // Small collision radius
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
            // Sleep if no foxes are visible
            return !dog.VisibleAgents.Any(agent => agent is AdvancedFoxContext);
        }
        return false;
    }
}