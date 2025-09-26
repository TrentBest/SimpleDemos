

using TheSingularityWorkshop.FSM_API;



//Define the application context
SimpleDemoContext appContext = new SimpleDemoContext();

//now we enter a loop which will run indefinitely until the handle becomes invalid.
do
{
    //We make this update call which will then update the app fsm.  The app's
    FSM_API.Interaction.Update("Main");
} while (appContext.IsValid);

Console.Write($"Press the 'A' key to see the AdvancedDemo:\t");
var keyPress = Console.ReadKey();
if (keyPress.KeyChar == 'A' || keyPress.KeyChar == 'a')
{
    AdvancedDemoContext advancedContext = new AdvancedDemoContext();
    do
    {
        foreach (var processGroup in FSM_API.Internal.GetProcessingGroups())
        {
            FSM_API.Interaction.Update(processGroup);
        }
    } while (advancedContext.IsValid);
}
Console.WriteLine($"\n\n Thank you!");
