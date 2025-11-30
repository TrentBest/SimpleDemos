
using System;

using TheSingularityWorkshop.FSM_API;

public class QuickBrownFox : IStateContext, ISimpleAgent
{
    public int Position { get; set; }
    public int Speed { get; set; } = 1;
    public int Sight { get; } = 2;
    private int JumpEnd = 0;

    public QuickBrownFox(int position)
    {
        this.Position = position;
        if (!FSM_API.Interaction.Exists("QuickBrownFoxFSM", "Update"))
        {
            FSM_API.Create.CreateFiniteStateMachine("QuickBrownFoxFSM", -1, "Update")
                .State("Walking", 
                ctx => (ctx as QuickBrownFox)?.OnEnterWalking(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnUpdateWalking(ctx),
                ctx => (ctx as QuickBrownFox)?.OnExitWalking(ctx))
                .State("Jumping", 
                ctx => (ctx as QuickBrownFox)?.OnEnterJump(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnUpdateJumping(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnExitJumping(ctx))
                .State("Fleeing", 
                ctx => (ctx as QuickBrownFox)?.OnEnterFleeing(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnUpdateFleeing(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnExitFleeing(ctx))
                .State("Mangled", 
                ctx => (ctx as QuickBrownFox)?.OnEnterMangled(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnUpdateMangled(ctx), 
                ctx => (ctx as QuickBrownFox)?.OnExitMangled(ctx))
                .Transition("Walking", "Jumping", ShouldJump)
                .Transition("Jumping", "Walking", ShouldLand)
                .Transition("Walking", "Fleeing", ShouldFlee)
                .BuildDefinition();
        }
        State = FSM_API.Create.CreateInstance("QuickBrownFoxFSM", this, "Update");
        IsValid = true;
    }

    private void OnEnterJump(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.JumpEnd = fox.Position + 2;
        }
    }

    private void OnEnterWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Speed = 1;
            Log($"ENTER Walking..... Speed set to {fox.Speed}.");
        }
    }

    // Helper to control logging centrally
    private void Log(string message)
    {
        if (SimpleDemoContext.EnableLogging) Console.WriteLine($"[FOX]: {message}");
    }

    // --- TRANSITIONS ---

    private bool ShouldLand(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            return true;
        }
        return false;
    }

    private bool ShouldJump(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            foreach (var visible in fox.VisibleAgents)
            {
                var distance = visible.Position - fox.Position;
                
                if (distance > 0 && distance <= 1)
                {
                    Log($"Condition Met: Jump! Obstacle detected at Dist {distance}");
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
            bool collision = fox.CollidedAgents.Any(s => s is LazySleepingDog);
            if (collision) Log("Condition Met: Flee (Dog Collision!)");
            return collision;
        }
        return false;
    }

    // --- STATES ---

    //private void OnEnterWalking(IStateContext context)
    //{
    //    if (context is QuickBrownFox fox)
    //    {
    //        fox.Speed = 1;
    //        Log($"ENTER Walking. Speed set to {fox.Speed}.");
    //    }
    //}

    private void OnUpdateWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Log($"UPDATE Walking. Moved to {fox.Position}.");
        }
    }

    private void OnExitWalking(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log("EXIT Walking.");
    }

    private void OnEnterJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            // Jump logic: Position + 2 clears a single unit obstacle
            fox.JumpEnd = fox.Position + 2;
            fox.Position+= fox.Speed;
            Log($"ENTER Jumping! Launching from {fox.Position}. Target: {fox.JumpEnd}.");
        }
    }
    
    private void OnUpdateJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Log($"UPDATE Jumping (Airborne). Pos: {fox.Position}.");
        }
    }

    private void OnExitJumping(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log($"EXIT Jumping. Landed at {fox.Position}.");
    }

    private void OnEnterFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Speed = 2;
            Log("ENTER Fleeing! Speed increased to 2.");
        }
    }

    private void OnUpdateFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox)
        {
            fox.Position += fox.Speed;
            Log($"UPDATE Fleeing. Pos: {fox.Position}.");
        }
    }

    private void OnExitFleeing(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log("EXIT Fleeing.");
    }

    private void OnEnterMangled(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log("ENTER Mangled. (Oh no!)");
    }

    private void OnUpdateMangled(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log("UPDATE Mangled. x_x");
    }

    private void OnExitMangled(IStateContext context)
    {
        if (context is QuickBrownFox fox) Log("EXIT Mangled.");
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; } = "The Quick Brown Fox";
    public FSMHandle State { get; private set; }
    public List<ISimpleAgent> VisibleAgents { get; private set; } = new List<ISimpleAgent>();
    public List<ISimpleAgent> CollidedAgents { get; private set; } = new List<ISimpleAgent>();
}