


using TheSingularityWorkshop.FSM_API;

public class QuickBrownFox : IStateContext, ISimpleAgent
{
    public int Position { get; set; }
    public int Speed { get; set; } = 1;
    public int Sight { get; } = 3;
    private int JumpEnd = 0;
    public QuickBrownFox(int position)
    {
        this.Position = position;
        if (!FSM_API.Interaction.Exists("QuickBrownFoxFSM", "Update"))
        {
            FSM_API.Create.CreateFiniteStateMachine("QuickBrownFoxFSM", -1, "Update")
                .State("Walking", OnEnterWalking, OnUpdateWalking, OnExitWalking)
                .State("Jumping", OnEnterJumping, OnUpdateJumping, OnExitJumping)
                .State("Fleeing", OnEnterFleeing, OnUpdateFleeing, OnExitFleeing)
                .State("Mangled", OnEnterMangled, OnUpdateMangled, OnExitManagled)
                .Transition("Walking", "Jumping", ShouldJump)
                .Transition("Jumping", "Walking", ShouldLand)
                .Transition("Walking", "Fleeing", ShouldFlee)
                .BuildDefinition();
        }
        State = FSM_API.Create.CreateInstance("QuickBrownFoxFSM", this, "Update");
        IsValid = true;
    }

    private bool ShouldLand(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            return fox.Position >= fox.JumpEnd;
        }
        return false;
    }

    private void OnEnterWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Speed = 1;
            Console.WriteLine($"{fox.Name} has started walking at a speed of:  {fox.Speed}!");
        }
    }

    private void OnUpdateWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Console.WriteLine($"{fox.Name} is walking:  {fox.Position}");
        }
    }

    private void OnExitWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has stopped walking.");
        }
    }

    private void OnEnterJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has started jumping at:  {fox.Position}!");
            JumpEnd = fox.Position + 2;
        }
    }

    private void OnUpdateJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Console.WriteLine($"{fox.Name} is jumping:  {fox.Position}!");
        }
    }

    private void OnExitJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has finished jumping:  {fox.Position}!");
        }
    }

    private void OnEnterFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has started to flee!");
            fox.Speed = 2;
        }
    }

    private void OnUpdateFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Console.WriteLine($"{fox.Name} is fleeing:  {fox.Position}!");
        }
    }

    private void OnExitFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has stopped fleeing!");
        }
    }

    private void OnEnterMangled(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has started being mangled!");
        }
    }

    private void OnUpdateMangled(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} is being mangled!");
        }
    }

    private void OnExitManagled(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            Console.WriteLine($"{fox.Name} has been mangled!");
        }
    }

    private bool ShouldJump(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            foreach (var visible in fox.VisibleAgents)
            {
                var distance = visible.Position - fox.Position;
                if(distance <= 2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool ShouldFlee(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            return fox.CollidedAgents.Any(s => s is LazySleepingDog dog);
        }
        return false;
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "The Quick Brown Fox";
    public FSMHandle State { get; private set; }
    public List<ISimpleAgent> VisibleAgents { get; private set; } = new List<ISimpleAgent>();
    public List<ISimpleAgent> CollidedAgents { get; private set; } = new List<ISimpleAgent>();
}