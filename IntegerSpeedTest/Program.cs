

using System.Diagnostics;

using TheSingularityWorkshop.FSM_API;
int intMax = 100000;
int j = 0;
Stopwatch sw = Stopwatch.StartNew();
do
{
    j++;
} while (j < intMax);
sw.Stop();

Console.WriteLine($"Integer loop completed in {sw.ElapsedMilliseconds} ms");

var timerContext = new TimerContext("testTimer", "UpdateProcessGroup", intMax);
sw.Restart();
do
{
    FSM_API.Interaction.Update("UpdateProcessGroup");
} while (timerContext.Status.CurrentState != "OFF");
sw.Stop();
Console.WriteLine($"FSM loop completed in {sw.ElapsedMilliseconds} ms");

internal class TimerContext : IStateContext
{
    public FSMHandle Status { get; set; }
    public TimerContext(string timerName, string processGroup, int intMax =1000)
    {
        Name = timerName;
        this.intMax = intMax;
        if (!FSM_API.Interaction.Exists(timerName, processGroup))
        {
            FSM_API.Create.CreateProcessingGroup(processGroup);
            FSM_API.Create.CreateFiniteStateMachine(timerName, -1, processGroup)
                .State("Counting", OnEnterCounting, OnUpdateCounting, OnExitCounting)
                .State("OFF", null, null, null)
                .Transition("Counting", "OFF", IsMaxed)
                .BuildDefinition();
        }
        Status = FSM_API.Create.CreateInstance(timerName, this, processGroup);
        IsValid = true;
    }

    private void OnEnterCounting(IStateContext context)
    {
        if(context is TimerContext timerContext)
        {
            timerContext.Counter = 0;
        }
    }

    private void OnUpdateCounting(IStateContext context)
    {
        if (context is TimerContext timerContext)
        {
            timerContext.Counter++;
        }
    }

    private void OnExitCounting(IStateContext context)
    {
        if (context is TimerContext timerContext)
        {
            
        }
    }

    private bool IsMaxed(IStateContext context)
    {
        if(context is TimerContext timerContext)
        {
            return timerContext.Counter == timerContext.intMax;
        }
        return false;
    }

    public bool IsValid { get; set; } = false;
    public string Name { get; set; }

    private int intMax;

    public int Counter { get; private set; }
}