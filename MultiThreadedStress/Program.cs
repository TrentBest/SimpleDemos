using System;
using System.Linq;
using TheSingularityWorkshop.FSM_API;
using System.Management; // Required for detailed system info (CPU Name, RAM)
using System.Collections.Generic;
using System.Diagnostics;

// --- 1. NEW PROCESSOR CONTEXT CLASS ---
// This FSM context will hold the state and performance data for a single CPU core.
public class ProcessorContext : IStateContext
{

    
    public int CoreIndex { get; set; }
    public double LastTickUtilization { get; set; } = 0.0;
    public long OpsPerFrame { get; set; } = 0;

    // IStateContext Implementation
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    public FSMHandle Status { get; set; }
    public ProcessStartInfo ProcessStartInfo { get; internal set; }
    public Process? Process { get; private set; }

    public ProcessorContext()
    {
        if(!FSM_API.Interaction.Exists("ProcessorFSM", "Processors"))
        {
            FSM_API.Create.CreateProcessingGroup("Processors");
            FSM_API.Create.CreateFiniteStateMachine("ProcessorFSM", -1, "Processors")
                .State("Initialize", OnEnterInitialize, null, null)
                .State("Monitoring", null, OnUpdateMonitoring, null)
                .Transition("Initialize", "Monitoring", (ctx) => true)
                .BuildDefinition();
        }

        IsValid = false;
    }

    private void OnUpdateMonitoring(IStateContext context)
    {
        if(context is ProcessorContext proc)
        {
            if(proc.Process != null && !proc.Process.HasExited)
            {

                Console.WriteLine($"[Core {proc.CoreIndex}] Utilization: {proc.LastTickUtilization:F2}% | Ops/Frame: {proc.OpsPerFrame:N0}");
            }
            else
            {
                Console.WriteLine($"[Core {proc.CoreIndex}] Stress process has exited or is not running.");
            }
        }
    }

    private void OnEnterInitialize(IStateContext context)
    {
        if(context is ProcessorContext proc)
        {
            // Start the external stress process for this core
            proc.Process = Process.Start(proc.ProcessStartInfo);
            Console.WriteLine($"Processor FSM '{proc.Name}' initialized and started stress process on Core {proc.CoreIndex}.");
        }
    }
}

// --- 2. APPLICATION CONTEXT CLASS (IMPLEMENTED) ---
public class ApplicationContext : IStateContext
{
    private Dictionary<string, List<FSM_API.Internal.FsmBucket>> ExecutionSequence { get; } = new Dictionary<string, List<FSM_API.Internal.FsmBucket>>();
    public void AddProcessGroupToExecutionSequence(string processGroup)
    {
        if (!ExecutionSequence.ContainsKey(processGroup))
        {
            ExecutionSequence.Add(processGroup, new List<FSM_API.Internal.FsmBucket>());
            FSM_API.Create.CreateProcessingGroup(processGroup);
        }
    }
    public void RemProcessGroupFromExecutionSequence(string processGroup)
    {
        ExecutionSequence.Remove(processGroup);
    }
    // Global System Information Storage
    public string OSInfo { get; private set; }
    public long TotalRAMMB { get; private set; }
    public string CPUName { get; private set; }

    // FSM Properties
    public bool IsValid { get; set; } = false;
    public string Name { get; set; }
    public FSMHandle Status { get; private set; }
    public string ProcessGroup { get; private set; } = "App";

    // Storage for the per-processor FSM handles
    public FSMHandle[] Processors { get; private set; }

    public ApplicationContext(string[] args)
    {
        Name = args.Length > 0 ? args[0] : "App";

        if (!FSM_API.Interaction.Exists(Name, ProcessGroup))
        {
            FSM_API.Create.CreateProcessingGroup(ProcessGroup);
            FSM_API.Create.CreateFiniteStateMachine(Name, -1, ProcessGroup)
                .State("Initialize", OnEnterInitialize, OnUpdateInitialize, OnExitInitialize)
                .State("Executing", OnEnterExecuting, OnUpdateExecuting, OnExitExecuting)
                .State("Shutdown", OnEnterShutdown, OnUpdateShutdown, OnExitShutdown)
                .Transition("Initialize", "Executing", InitializationCompleted)
                .Transition("Executing", "Shutdown", Shutdown)
                .BuildDefinition();
        }

        Status = FSM_API.Create.CreateInstance(Name, this, ProcessGroup);
        IsValid = true;
    }

    // --- FSM STATE HANDLERS ---

    // 1. Initialize State
    private void OnEnterInitialize(IStateContext context)
    {
        if (context is ApplicationContext app)
        {
            // --- A. GATHER MACHINE QUALITY STATS ---
            GatherSystemInformation(app);
            DisplaySystemInformation(app);

            // --- B. CREATE PROCESSOR FSMs ---
            int coreCount = Environment.ProcessorCount;
            app.Processors = new FSMHandle[coreCount];
            Console.WriteLine($"\nInitializing {coreCount} FSM Processors...");

            // Define the basic Processor FSM once
            if (!FSM_API.Interaction.Exists("ProcessorFSM", "Processors"))
            {
                FSM_API.Create.CreateProcessingGroup("Processors"); // Dedicated group for processor monitoring
                FSM_API.Create.CreateFiniteStateMachine("ProcessorFSM", -1, "Processors")
                    .State("Monitoring", null, null, null) // Simple monitoring state
                    .BuildDefinition();
            }
            string fileName = @"C:\Users\theta\Documents\TheSingularityWorkshop\SimpleDemos\Stress\bin\Debug\net8.0\Stress.exe";
            // Create an FSM instance for each logical processor
            for (int i = 0; i < coreCount; i++)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(fileName, new List<string>() { i.ToString() });
                var pCtx = new ProcessorContext
                {
                    Name = $"CPU_{i}",
                    CoreIndex = i,
                    ProcessStartInfo = startInfo
                };
                app.Processors[i] = FSM_API.Create.CreateInstance("ProcessorFSM", pCtx, "Processors");
                pCtx.Status = app.Processors[i];
                
            }
            Console.WriteLine("Processor FSMs initialized and ready for monitoring.");
        }
    }

    private bool InitializationCompleted(IStateContext context)
    {
        // Simple check: transition after one tick to allow initialization to finish
        return true;
    }

    private void OnUpdateInitialize(IStateContext context) { /* Wait one tick */ }
    private void OnExitInitialize(IStateContext context) { /* Cleanup if needed */ }

    public Dictionary<string, FSM_API.Internal.FsmBucket> Execution = new Dictionary<string, FSM_API.Internal.FsmBucket>();

    // 2. Executing State
    private void OnEnterExecuting(IStateContext context)
    {
        Console.WriteLine("\nAPPLICATION: Entering Executing State. Ready for stress tests.");
        if(context is ApplicationContext app)
        {
            var faster = FSM_API.Internal.GetBuckets();
            foreach (var group in faster)
            {
                if (app.Execution.ContainsKey(group.Key))
                {
                    Dictionary<string, FSM_API.Internal.FsmBucket> abc = group.Value;
                    app.Execution[group.Key]=group.Value;
                }
            }
        }
    }

    private void OnUpdateExecuting(IStateContext context)
    {
        // Logic for running the stress test and reporting would go here.
    }

    private bool Shutdown(IStateContext context)
    {
        // For testing, shut down immediately after entering Executing, or based on a condition
        return false; // Stay in Executing until manually triggered
    }

    private void OnExitExecuting(IStateContext context) { /* Cleanup Executing resources */ }


    // 3. Shutdown State
    private void OnEnterShutdown(IStateContext context)
    {
        Console.WriteLine("\nAPPLICATION: Entering Shutdown State. Cleaning up...");
        // Destroy all created processor FSMs
        if (Processors != null)
        {
            foreach (var handle in Processors)
            {
                FSM_API.Interaction.DestroyInstance(handle);
            }
        }
    }

    private void OnUpdateShutdown(IStateContext context) { /* Final cleanup checks */ }
    private void OnExitShutdown(IStateContext context) { /* Application fully exited */ }


    // --- UTILITY METHODS ---

    private void GatherSystemInformation(ApplicationContext app)
    {
        // 1. OS Information
        app.OSInfo = Environment.OSVersion.VersionString +
                     (Environment.Is64BitOperatingSystem ? " (64-bit)" : " (32-bit)");

        // 2. Total RAM (using WMI for a reasonable estimate)
        // NOTE: This uses System.Management, which requires a NuGet package reference.
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                var memoryValues = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (memoryValues != null)
                {
                    // TotalVisibleMemorySize is in KB, convert to MB
                    app.TotalRAMMB = Convert.ToInt64(memoryValues["TotalVisibleMemorySize"]) / 1024;
                }
            }
        }
        catch (PlatformNotSupportedException)
        {
            // Fallback for non-Windows systems or missing WMI access
            app.TotalRAMMB = Environment.WorkingSet / 1024 / 1024; // Use working set as a proxy (not accurate Total RAM)
        }

        // 3. CPU Name (using WMI for the actual marketing name)
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
            {
                var processor = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                if (processor != null)
                {
                    app.CPUName = processor["Name"].ToString().Trim();
                }
            }
        }
        catch (PlatformNotSupportedException)
        {
            app.CPUName = $"Logical Cores: {Environment.ProcessorCount}";
        }
    }

    private void DisplaySystemInformation(ApplicationContext app)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n--- MACHINE QUALITY REPORT ---");
        Console.WriteLine($"OS:             {app.OSInfo}");
        Console.WriteLine($"CPU:            {app.CPUName}");
        Console.WriteLine($"Logical Cores:  {Environment.ProcessorCount}");
        Console.WriteLine($"Total RAM:      {app.TotalRAMMB:N0} MB");
        Console.WriteLine("------------------------------");
        Console.ResetColor();
    }
}

// --- 3. PROGRAM ENTRY POINT ---
class Program
{
    static void Main(string[] args)
    {
        var app = new ApplicationContext(args);

        // Loop is driven by the application's single update thread
        do
        {
            FSM_API.Interaction.Update(app.ProcessGroup);
        } while (app.Status.CurrentState != "Shutdown");
    }
}