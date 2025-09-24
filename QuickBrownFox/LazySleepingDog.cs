

using TheSingularityWorkshop.FSM_API;

internal class LazySleepingDog : IStateContext, ISimpleAgent
{
    public int Position { get; set; }
    public int Speed { get; set; } = 1;
    public int Sight { get; } = 2;
    public LazySleepingDog(int position)
    {
        this.Position = position;
        if(!FSM_API.Interaction.Exists("LazyDogFSM", "Update"))
        {
            FSM_API.Create.CreateFiniteStateMachine("LazyDogFSM", -1, "Update")
                .State("Sleeping", OnEnterSleeping, OnUpdateSleeping, OnExitSleeping)
                .State("Awake", OnEnterAwake, OnUpdateAwake, OnExitAwake)
                .State("Chasing", OnEnterChasing, OnUpdateChasing, OnExitChasing)
                .State("Mangling", OnEnterMangling, OnUpdteMangling, OnExitMangling)
                .Transition("Sleeping", "Awake", IsAwake)
                .Transition("Awake", "Chasing", ShouldChase)
                .Transition("Chasing", "Mangling", IsMangling)
                .BuildDefinition();
        }
        State = FSM_API.Create.CreateInstance("LazyDogFSM", this, "Update");
        IsValid = true;
    }

    private void OnEnterSleeping(IStateContext context)
    {
        if(context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has started sleeping!");
        }
    }

    private void OnUpdateSleeping(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} is sleeping!");
        }
    }

    private void OnExitSleeping(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has stopped sleeping!");
        }
    }

    private void OnEnterAwake(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has started awake!");
        }
    }

    private void OnUpdateAwake(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} is awake!");
        }
    }

    private void OnExitAwake(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} is leaving the awake state!");
        }
    }

    private void OnEnterChasing(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has started chasing!");
            dog.Speed = 3;
        }
    }

    private void OnUpdateChasing(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            dog.Position += dog.Speed;
            Console.WriteLine($"{dog.Name} is chasing:  {dog.Position}");
        }
    }

    private void OnExitChasing(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has stopped chasing!");
            dog.Speed = 1;
        }
    }

    private void OnEnterMangling(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has started Mangling the fox!");
            dog.CollidedAgents.FirstOrDefault()?.State.TransitionTo("Mangled");
        }
    }

    private void OnUpdteMangling(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} is Mangling the fox!");
        }
    }

    private void OnExitMangling(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            Console.WriteLine($"{dog.Name} has stopped mangling the fox!");
        }
    }

    private bool IsAwake(IStateContext context)
    {
        if (context is LazySleepingDog dog)
        {
            return dog.CollidedAgents.Any();
        }
        return false;
    }

    private bool ShouldChase(IStateContext context)
    {
        if(context is LazySleepingDog dog)
        {
            return dog.VisibleAgents.Any(s => s is QuickBrownFox p);
        }
        return false;
    }

    private bool IsMangling(IStateContext context)
    {
        if( context is LazySleepingDog dog)
        {
            return dog.CollidedAgents.Any(s => s is QuickBrownFox p);
        }
        return false;
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "Lazy Sleeping Dog";
    public List<ISimpleAgent> VisibleAgents { get; } = new List<ISimpleAgent>();
    public List<ISimpleAgent> CollidedAgents { get; } = new List<ISimpleAgent>();
    public FSMHandle State { get; }

}