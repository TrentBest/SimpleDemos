
using TheSingularityWorkshop.FSM_API;

var fsm_cos = new FSM_COS_Context();

do
{
    FSM_API.Interaction.Update("FSM_COS_ProcessGroup");
} while (!fsm_cos.IsComplete);
