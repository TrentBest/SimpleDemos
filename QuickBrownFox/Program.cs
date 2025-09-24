

using TheSingularityWorkshop.FSM_API;



//Define the application context
SimpleDemoContext appContext = new SimpleDemoContext();

//now we enter a loop which will run indefinitely until the handle becomes invalid.
do
{
    //We make this update call which will then update the app fsm.  The app's
    FSM_API.Interaction.Update("Main");
} while (appContext.IsValid);
