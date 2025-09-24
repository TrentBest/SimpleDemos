using TheSingularityWorkshop.FSM_API;

public interface ISimpleAgent
{
    FSMHandle State { get; }
    List<ISimpleAgent> VisibleAgents { get; }
    List<ISimpleAgent> CollidedAgents { get; }
    int Position { get; set; }
    int Speed { get; set; }
    int Sight { get; }
}