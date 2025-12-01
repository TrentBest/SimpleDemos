using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public interface IAdvancedAgent : IStateContext
{
    Vector2 Position { get; set; }
    FSMHandle Status { get; }
    List<IAdvancedAgent> VisibleAgents { get; }
    List<IAdvancedAgent> CollidingAgents { get; }
    float SightRange { get; set; }
}

public interface IFoxAgent : IAdvancedAgent
{
    Vector2 FindRandomDestination();
}