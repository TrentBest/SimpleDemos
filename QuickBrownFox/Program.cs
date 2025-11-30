

using TheSingularityWorkshop.FSM_API;


Console.WriteLine("Welcome to the Quick Brown Fox Demo!");
//Console.WriteLine($"{FSM_API.Internal.GetProcessingGroups().Count} and {FSM_API.Internal.GetAllFsmHandles().ToArray().Length}");
//Define the application context
SimpleDemoContext appContext = new SimpleDemoContext();
//Console.WriteLine($"{FSM_API.Internal.GetProcessingGroups().Count} and {FSM_API.Internal.GetAllFsmHandles().ToArray().Length}");
//now we enter a loop which will run indefinitely until the handle becomes invalid.

do
{
    //We make this update call which will then update the app fsm.  The app's
    FSM_API.Interaction.Update("Main");
    //Console.WriteLine($"{FSM_API.Internal.GetProcessingGroups().Count} and {FSM_API.Internal.GetAllFsmHandles().ToArray().Length}");
} while (appContext.IsValid);

//Console.Write($"Press the 'A' key to see the AdvancedDemo:\t");
//var keyPress = Console.ReadKey();
//if (keyPress.KeyChar == 'A' || keyPress.KeyChar == 'a')
//{
//    AdvancedDemoContext advancedContext = new AdvancedDemoContext();
//    do
//    {
//        foreach (var processGroup in FSM_API.Internal.GetProcessingGroups())
//        {
//            FSM_API.Interaction.Update(processGroup);
//        }
//    } while (advancedContext.IsValid);
//}
Console.WriteLine($"\n\n Thank you!");
