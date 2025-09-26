using System.Numerics;

using TheSingularityWorkshop.FSM_API;

internal class AdvancedFoxContext : IFoxAgent
{
    public Vector2 Position { get; set; }
    public Vector2 Destination { get; set; }

    public AdvancedFoxContext(Vector2 pos)
    {
        this.Position = pos;
        if(!FSM_API.Interaction.Exists("AdvancedFoxFSM", "Foxes"))
        {
            FSM_API.Create.CreateFiniteStateMachine("AdvancedFoxFSM", -1, "Foxes")
                .State("Idle", null, OnUpdateIdle, null)
                .State("Walking", OnEnterWalking, OnUpdateWalking, OnExitWalking)
                .State("Jumping", OnEnterJumping, OnUpdateJumping, OnExitJumping)
                .State("Fleeing", OnEnterFleeing, OnUpdateFleeing, OnExitFleeing)
                .State("Mangled", OnEnterMangled, OnUpdateManagled, OnExitMangled)
                .Transition("Idle", "Walking", HasDestination)
                .Transition("Walking", "Idle", HasNoDestination)
                .Transition("Walking", "Jumping", ShouldJump)
                .Transition("Jumping", "Walking", JumpComplete)
                .Transition("Walking", "Fleeing", ShouldFlee)
                .Transition("Fleeing", "Walking", ShouldStopFleeing)
                .BuildDefinition();
        }
    }

    private void OnUpdateIdle(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnEnterWalking(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnUpdateWalking(IStateContext context)
    {
        if (context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnExitWalking(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnEnterJumping(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnUpdateJumping(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnExitJumping(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnEnterFleeing(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnUpdateFleeing(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnExitFleeing(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnEnterMangled(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnUpdateManagled(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
    }

    private void OnExitMangled(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }

    }

    private bool HasDestination(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
        return false;
    }

    private bool HasNoDestination(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
        return false;
    }

    private bool ShouldJump(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
    }

    private bool JumpComplete(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
        return false;
    }

    private bool ShouldFlee(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {

        }
        return false;
    }

    private bool ShouldStopFleeing(IStateContext context)
    {
        if(context is AdvancedFoxContext fox)
        {
            return fox.VisibleAgents.Any(agent => agent is AdvancedDogContext dog && dog.Status.CurrentState == "Awake");
        }
        return false;
    }

    public Vector2 FindRandomDestination()
    {
        return new Vector2(0, 0);
    }

    public int SightRange { get; } = 2;
    public List<IAdvancedAgent> VisibleAgents { get; } = new List<IAdvancedAgent>();
    public List<IAdvancedAgent> CollidingAgents { get; } = new List<IAdvancedAgent>();
}