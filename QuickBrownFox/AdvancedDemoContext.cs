
using System.Numerics;

using TheSingularityWorkshop.FSM_API;

public class AdvancedDemoContext : IStateContext
{
    public bool IsValid { get; set; }
    public string Name { get; set; }
    public int EnvironmentWidth { get; private set; }
    public int EngironmentHeight { get; private set; }
    public int FoxCount { get; private set; }
    public int DogCount { get; private set; }

    public AdvancedDemoContext()
    {
        FSM_API.Internal.ResetAPI(true);//Hard reset
        //For this demo, we are going to introduce the second axis for our
        //agents, and the foxes will be assigned a random destination and
        //then they will walk to that point, jumping over any sleeping dogs.
        //Because this is to be a stress test, we want to have a lot of foxes,
        //a lot of dogs... 
        if(!FSM_API.Interaction.Exists("AdvancedDemoFSM", "Update"))
        {
            FSM_API.Create.CreateFiniteStateMachine("Environment");
            FSM_API.Create.CreateProcessingGroup("Foxes");
            FSM_API.Create.CreateProcessingGroup("Dogs");

            FSM_API.Create.CreateFiniteStateMachine("AdvancedDemoFSM", -1, "Update")
                .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                .State("Shutdown", OnEnterShutdown, null, null)
                .Transition("Executing", "Shutdown", ShouldShutDown)
                .BuildDefinition();
        }
    }

    private void OnEnterExecuting(IStateContext context)
    {
        if(context is AdvancedDemoContext adc)
        {
            var environmentWidth = adc.EnvironmentWidth;
            var environmentHeight = adc.EngironmentHeight;
            var foxCount = adc.FoxCount;
            var dogCount = adc.DogCount;
            AdvancedDemoEnvironment ade = new AdvancedDemoEnvironment();
            for (int d = 0; d < dogCount; d++)
            {
                //Need a random position that isn't already occupied by a dog.
                var pos = new Vector2 (0, 0);
                if (ade.IsEmptyAt(pos))
                {
                    var dog = new AdvancedDogContext(pos);
                    ade.AddAgent(dog);
                }
                else
                {
                    d--;//by decrementing we stay at the same dog until we land it 
                    //somewhere empty.
                }
            }
            for (int f = 0; f < foxCount; f++)
            {
                //Need a random position that isn't already occupied by a dog or fox.
                var pos = new Vector2(0, 0);
                if (ade.IsEmptyAt(pos))
                {
                    var fox = new AdvancedFoxContext(pos);
                    ade.AddAgent(fox);
                }
            }
        }
    }

    private void OnUpdateExecuting(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnExitExecuting(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private void OnEnterShutdown(IStateContext context)
    {
        throw new NotImplementedException();
    }

    private bool ShouldShutDown(IStateContext context)
    {
        throw new NotImplementedException();
    }
}