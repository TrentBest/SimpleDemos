using TheSingularityWorkshop.FSM_API;

public class FSM_COS_Context : IStateContext
{
    // PROXY DRIVER: This allows us to "swap" control sources (Local vs Arcade)
    public ISystemDriver Driver { get; set; } = new LocalDriver();

    public FSM_COS_Context()
    {
        Name = "FSM_COS_Context";

        // 1. DEFINE THE LOGIC (The Blueprint)
        // We define the shape of the logic before we worry about memory.
        if (!FSM_API.Interaction.Exists("FSM_COS", "FSM_COS_ProcessGroup"))
        {
            FSM_API.Create.CreateProcessingGroup("FSM_COS_ProcessGroup");
            FSM_API.Create.CreateFiniteStateMachine("FSM_COS", -1, "FSM_COS_ProcessGroup")
                .State("Active", OnEnterActive, OnUpdateActive, null) // Initialize memory on Enter
                .State("Idle", null, OnUpdateIdle, null)
                .Transition("Active", "Idle", ctx => ((FSM_COS_Context)ctx).Driver.ShouldIdle())
                .Transition("Idle", "Active", ctx => !((FSM_COS_Context)ctx).Driver.ShouldIdle())
                .BuildDefinition();
        }

        // 2. BIND THE INSTANCE
        Status = FSM_API.Create.CreateInstance("FSM_COS", this, "FSM_COS_ProcessGroup");
        IsValid = true;
    }

    public FSMHandle Status { get; private set; }
    public bool IsValid { get; set; } = true;
    public string Name { get; set; }

    // --- STATES ---

    private void OnEnterActive(IStateContext ctx)
    {
        // 3. LAZY MEMORY IGNITION
        // We only reserve the massive memory block if we are actually going to use it.
        if (!FSM_Memory.IsInitialized)
        {
            FSM_Memory.Initialize();
        }
    }

    private void OnUpdateActive(IStateContext ctx)
    {
        // High Performance Allocation via the "Bay" system
        var agent = FSM_Memory.Allocate<AgentData>();

        // Pass the agent to the Driver (Local or Networked) to handle
        Driver.ProcessAgent(agent);
    }

    private void OnUpdateIdle(IStateContext ctx)
    {
        FSM_Memory.PerformIdleMaintenance();
    }
}